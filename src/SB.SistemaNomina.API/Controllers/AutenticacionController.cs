using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SB.SistemaNomina.API.Extensiones;
using SB.SistemaNomina.Aplicacion.Interfaces.Servicios;
using SB.SistemaNomina.Aplicacion.Modelos.Autenticacion;

namespace SB.SistemaNomina.API.Controllers;

[ApiController]
[Route("api/autenticacion")]
[Produces("application/json")]
public sealed class AutenticacionController : ControllerBase
{
    private readonly IAutenticacionServicio _autenticacionServicio;

    public AutenticacionController(IAutenticacionServicio autenticacionServicio)
    {
        _autenticacionServicio = autenticacionServicio;
    }

    /// <summary>Inicia sesión y devuelve un token JWT.</summary>
    /// <response code="200">Token, fecha de expiración y datos del usuario.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="401">Usuario o contraseña incorrectos.</response>
    /// <response code="429">Demasiados intentos.</response>
    [HttpPost("iniciar-sesion")]
    [AllowAnonymous]
    [EnableRateLimiting(ExtensionesProteccion.POLITICA_INICIO_SESION)]
    [ProducesResponseType<InicioSesionRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<InicioSesionRespuesta>> IniciarSesionAsync(
        IniciarSesionSolicitud solicitud,
        CancellationToken tokenCancelacion) =>
        Ok(await _autenticacionServicio.IniciarSesionAsync(solicitud, tokenCancelacion));
}
