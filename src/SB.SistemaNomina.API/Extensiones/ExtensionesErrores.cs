using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using SB.SistemaNomina.Aplicacion.Comun;

namespace SB.SistemaNomina.API.Extensiones;

/// <summary>Errores generados por el framework (400 automático, 401/403, 404 de ruta) en español y con traceId.</summary>
public static class ExtensionesErrores
{
    public const string CLAVE_TRAZA = "traceId";

    private const string CLAVE_CUERPO = "solicitud";
    private const string MENSAJE_VALOR_INVALIDO = "El valor enviado no es válido o falta.";

    private static readonly Dictionary<int, string> _titulosPorEstado = new()
    {
        [StatusCodes.Status400BadRequest] = "Solicitud inválida.",
        [StatusCodes.Status401Unauthorized] = "No autorizado.",
        [StatusCodes.Status403Forbidden] = "Acceso denegado.",
        [StatusCodes.Status404NotFound] = "Recurso no encontrado.",
        [StatusCodes.Status405MethodNotAllowed] = "Método no permitido.",
        [StatusCodes.Status415UnsupportedMediaType] = "Tipo de contenido no soportado.",
        [StatusCodes.Status429TooManyRequests] = "Demasiadas solicitudes.",
        [StatusCodes.Status500InternalServerError] = "Error interno del servidor."
    };

    private static readonly Dictionary<int, string> _detallesPorEstado = new()
    {
        [StatusCodes.Status401Unauthorized] = "Debe iniciar sesión con un token válido.",
        [StatusCodes.Status403Forbidden] = "No tiene permiso para esta operación."
    };

    public static IServiceCollection AddProblemasEnEspanol(this IServiceCollection servicios) =>
        servicios.AddProblemDetails(opciones => opciones.CustomizeProblemDetails = contexto =>
        {
            var problema = contexto.ProblemDetails;
            var estado = problema.Status ?? contexto.HttpContext.Response.StatusCode;

            problema.Title = _titulosPorEstado.GetValueOrDefault(estado, problema.Title ?? string.Empty);
            problema.Detail ??= _detallesPorEstado.GetValueOrDefault(estado);
            problema.Instance ??= contexto.HttpContext.Request.Path;
            problema.Extensions[CLAVE_TRAZA] = contexto.HttpContext.TraceIdentifier;
        });

    /// <summary>Respuestas sin cuerpo (401, 403, 404 de ruta): siempre ProblemDetails JSON, sin depender del Accept.</summary>
    public static IApplicationBuilder UseProblemasCodigoEstado(this IApplicationBuilder aplicacion) =>
        aplicacion.UseStatusCodePages(async contextoCodigo =>
        {
            var contexto = contextoCodigo.HttpContext;
            var problema = contexto.RequestServices
                .GetRequiredService<ProblemDetailsFactory>()
                .CreateProblemDetails(contexto, contexto.Response.StatusCode);
            await contexto.Response.WriteAsJsonAsync(problema, problema.GetType(), options: null, "application/problem+json", contexto.RequestAborted);
        });

    /// <summary>400 automático de [ApiController]: mensajes propios (no los del serializador) y registro Warning.</summary>
    public static IMvcBuilder ConfigurarValidacionAutomatica(this IMvcBuilder constructor) =>
        constructor.ConfigureApiBehaviorOptions(opciones => opciones.InvalidModelStateResponseFactory = contexto =>
        {
            // Cuerpo ausente llega como "" y como el nombre del parámetro: se agrupan en una sola clave.
            var errores = contexto.ModelState
                .Where(entrada => entrada.Value?.Errors.Count > 0)
                .Select(entrada => string.IsNullOrEmpty(entrada.Key) ? CLAVE_CUERPO : entrada.Key)
                .Distinct(StringComparer.Ordinal)
                .ToDictionary(clave => clave, _ => new[] { MENSAJE_VALOR_INVALIDO });

            var problema = contexto.HttpContext.RequestServices
                .GetRequiredService<ProblemDetailsFactory>()
                .CreateValidationProblemDetails(contexto.HttpContext, contexto.ModelState, StatusCodes.Status400BadRequest);
            problema.Title = MensajesValidacion.SOLICITUD_INVALIDA;
            problema.Errors = errores;

            contexto.HttpContext.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(ExtensionesErrores))
                .LogWarning("Solicitud con formato inválido en {Metodo} {Ruta}: campos {Campos}.",
                    contexto.HttpContext.Request.Method, contexto.HttpContext.Request.Path, errores.Keys);

            return new BadRequestObjectResult(problema) { ContentTypes = { "application/problem+json" } };
        });
}
