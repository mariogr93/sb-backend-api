using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.SistemaNomina.Aplicacion.Interfaces.Servicios;
using SB.SistemaNomina.Aplicacion.Modelos.Usuarios;
using SB.SistemaNomina.Dominio.Constantes;

namespace SB.SistemaNomina.API.Controllers;

/// <summary>Administración de usuarios: solo Admin.</summary>
[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = NombresRoles.ADMINISTRADOR)]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
public sealed class UsuariosController : ControllerBase
{
    private const string RUTA_OBTENER = "ObtenerUsuario";

    private readonly IUsuarioServicio _usuarioServicio;

    public UsuariosController(IUsuarioServicio usuarioServicio)
    {
        _usuarioServicio = usuarioServicio;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<UsuarioRespuesta>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UsuarioRespuesta>>> ListarAsync(CancellationToken tokenCancelacion) =>
        Ok(await _usuarioServicio.ListarAsync(tokenCancelacion));

    [HttpGet("{id:guid}", Name = RUTA_OBTENER)]
    [ProducesResponseType<UsuarioRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioRespuesta>> ObtenerPorIdAsync(Guid id, CancellationToken tokenCancelacion) =>
        Ok(await _usuarioServicio.ObtenerPorIdAsync(id, tokenCancelacion));

    [HttpPost]
    [ProducesResponseType<UsuarioRespuesta>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioRespuesta>> CrearAsync(
        CrearUsuarioSolicitud solicitud,
        CancellationToken tokenCancelacion)
    {
        var usuario = await _usuarioServicio.CrearAsync(solicitud, tokenCancelacion);
        return CreatedAtRoute(RUTA_OBTENER, new { id = usuario.Id }, usuario);
    }

    /// <summary>Actualiza nombre, rol y estado; no permite perder el último administrador.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<UsuarioRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<UsuarioRespuesta>> ActualizarAsync(
        Guid id,
        ActualizarUsuarioSolicitud solicitud,
        CancellationToken tokenCancelacion) =>
        Ok(await _usuarioServicio.ActualizarAsync(id, solicitud, tokenCancelacion));

    [HttpPut("{id:guid}/contrasena")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarContrasenaAsync(
        Guid id,
        CambiarContrasenaSolicitud solicitud,
        CancellationToken tokenCancelacion)
    {
        await _usuarioServicio.CambiarContrasenaAsync(id, solicitud, tokenCancelacion);
        return NoContent();
    }

    /// <summary>Desactiva; no permite desactivarse a sí mismo.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DesactivarAsync(Guid id, CancellationToken tokenCancelacion)
    {
        await _usuarioServicio.DesactivarAsync(id, tokenCancelacion);
        return NoContent();
    }
}
