using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SB.SistemaNomina.API.Seguridad;
using SB.SistemaNomina.Aplicacion.Interfaces.Seguridad;
using SB.SistemaNomina.Aplicacion.Interfaces.Servicios;
using SB.SistemaNomina.Servicios.Autenticacion;

namespace SB.SistemaNomina.API.Extensiones;

public static class ExtensionesAutenticacion
{
    private const string MENSAJE_CONFIGURACION_INVALIDA =
        "Autenticacion: Emisor, Audiencia, ClaveFirma (mínimo 32 bytes) y DuracionTokenMinutos (> 0) son obligatorios.";

    private const string MENSAJE_SESION_REVOCADA = "El usuario del token no existe, está inactivo o cambió de rol.";

    private static readonly TimeSpan _toleranciaReloj = TimeSpan.FromSeconds(30);

    /// <summary>JWT Bearer, autorización (todo requiere autenticación salvo [AllowAnonymous]) y usuario actual.</summary>
    public static IServiceCollection AddAutenticacionJwt(this IServiceCollection servicios, IConfiguration configuracion)
    {
        // Falla al arrancar si la configuración es insegura.
        servicios.AddOptions<OpcionesAutenticacion>()
            .Bind(configuracion.GetSection(OpcionesAutenticacion.SECCION))
            .Validate(EsConfiguracionValida, MENSAJE_CONFIGURACION_INVALIDA)
            .ValidateOnStart();

        servicios.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        servicios.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<OpcionesAutenticacion>>((opcionesJwt, opcionesAutenticacion) =>
            {
                var autenticacion = opcionesAutenticacion.Value;
                opcionesJwt.MapInboundClaims = false;
                opcionesJwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = autenticacion.Emisor,
                    ValidateAudience = true,
                    ValidAudience = autenticacion.Audiencia,
                    ValidateLifetime = true,
                    ClockSkew = _toleranciaReloj,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(autenticacion.ClaveFirma)),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role
                };
                opcionesJwt.Events = new JwtBearerEvents { OnTokenValidated = ValidarSesionVigenteAsync };
            });

        servicios.AddAuthorizationBuilder()
            .SetFallbackPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim(JwtRegisteredClaimNames.Sub)
                .Build());

        servicios.AddHttpContextAccessor();
        servicios.AddScoped<IUsuarioActual, UsuarioActual>();

        return servicios;
    }

    // Revoca tokens de usuarios desactivados o con rol cambiado antes de que expiren.
    private static async Task ValidarSesionVigenteAsync(TokenValidatedContext contexto)
    {
        var principal = contexto.Principal;
        var vigente = Guid.TryParse(principal?.FindFirstValue(JwtRegisteredClaimNames.Sub), out var usuarioId)
            && await contexto.HttpContext.RequestServices.GetRequiredService<IAutenticacionServicio>()
                .SesionVigenteAsync(usuarioId, principal!.FindFirstValue(ClaimTypes.Role), contexto.HttpContext.RequestAborted);

        if (!vigente)
        {
            contexto.Fail(MENSAJE_SESION_REVOCADA);
        }
    }

    private static bool EsConfiguracionValida(OpcionesAutenticacion opciones) =>
        !string.IsNullOrWhiteSpace(opciones.Emisor)
        && !string.IsNullOrWhiteSpace(opciones.Audiencia)
        && Encoding.UTF8.GetByteCount(opciones.ClaveFirma) >= OpcionesAutenticacion.LONGITUD_MINIMA_CLAVE_BYTES
        && opciones.DuracionTokenMinutos > 0;
}
