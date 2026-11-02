using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SB.SistemaNomina.API.Extensiones;

/// <summary>CORS y límite de solicitudes.</summary>
public static class ExtensionesProteccion
{
    public const string POLITICA_CORS = "Frontend";
    public const string POLITICA_INICIO_SESION = "InicioSesion";

    private const string SECCION_ORIGENES = "Cors:OrigenesPermitidos";
    private const string PARTICION_ANONIMA = "anonimo";
    private const string MENSAJE_LIMITE_INVALIDO = "LimiteSolicitudes: PermisosPorVentana y VentanaSegundos deben ser mayores que cero.";
    private const string DETALLE_LIMITE_EXCEDIDO = "Demasiadas solicitudes. Intente de nuevo más tarde.";

    public static IServiceCollection AddProteccionHttp(this IServiceCollection servicios, IConfiguration configuracion)
    {
        var origenes = configuracion.GetSection(SECCION_ORIGENES).Get<string[]>() ?? [];
        servicios.AddCors(opciones => opciones.AddPolicy(POLITICA_CORS, politica => politica
            .WithOrigins(origenes)
            .AllowAnyHeader()
            .AllowAnyMethod()));

        var limites = configuracion.GetSection(OpcionesLimiteSolicitudes.SECCION).Get<OpcionesLimiteSolicitudes>() ?? new();
        if (!EsValida(limites.General) || !EsValida(limites.InicioSesion))
        {
            throw new InvalidOperationException(MENSAJE_LIMITE_INVALIDO);
        }

        servicios.AddRateLimiter(opciones =>
        {
            opciones.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            opciones.OnRejected = EscribirRechazoAsync;

            // Usuario autenticado: por nombre; anónimo: por IP.
            opciones.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(contexto =>
                RateLimitPartition.GetFixedWindowLimiter(
                    contexto.User.Identity?.IsAuthenticated == true
                        ? $"usuario:{contexto.User.Identity.Name}"
                        : $"ip:{DireccionIp(contexto)}",
                    _ => CrearVentana(limites.General)));

            opciones.AddPolicy(POLITICA_INICIO_SESION, contexto =>
                RateLimitPartition.GetFixedWindowLimiter(DireccionIp(contexto), _ => CrearVentana(limites.InicioSesion)));
        });

        return servicios;
    }

    private static bool EsValida(VentanaLimite ventana) => ventana.PermisosPorVentana > 0 && ventana.VentanaSegundos > 0;

    private static FixedWindowRateLimiterOptions CrearVentana(VentanaLimite ventana) => new()
    {
        PermitLimit = ventana.PermisosPorVentana,
        Window = TimeSpan.FromSeconds(ventana.VentanaSegundos),
        QueueLimit = 0
    };

    private static string DireccionIp(HttpContext contexto) =>
        contexto.Connection.RemoteIpAddress?.ToString() ?? PARTICION_ANONIMA;

    private static async ValueTask EscribirRechazoAsync(OnRejectedContext rechazo, CancellationToken tokenCancelacion)
    {
        var contexto = rechazo.HttpContext;
        if (rechazo.Lease.TryGetMetadata(MetadataName.RetryAfter, out var reintento))
        {
            contexto.Response.Headers.RetryAfter = ((int)reintento.TotalSeconds).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        contexto.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(ExtensionesProteccion))
            .LogWarning("Límite de solicitudes excedido en {Ruta} desde {Ip}.", contexto.Request.Path, DireccionIp(contexto));

        var problema = new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Demasiadas solicitudes.",
            Detail = DETALLE_LIMITE_EXCEDIDO,
            Instance = contexto.Request.Path
        };
        problema.Extensions[ExtensionesErrores.CLAVE_TRAZA] = contexto.TraceIdentifier;
        await contexto.Response.WriteAsJsonAsync(problema, problema.GetType(), options: null, "application/problem+json", tokenCancelacion);
    }
}
