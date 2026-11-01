using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Interfaces.Servicios;
using SB.SistemaNomina.Aplicacion.Modelos.EntidadesGubernamentales;
using SB.SistemaNomina.Dominio.Constantes;

namespace SB.SistemaNomina.API.Controllers;

[ApiController]
[Route("api/entidades-gubernamentales")]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
public sealed class EntidadesGubernamentalesController : ControllerBase
{
    private const string RUTA_OBTENER = "ObtenerEntidadGubernamental";

    private readonly IEntidadGubernamentalServicio _entidadServicio;

    public EntidadesGubernamentalesController(IEntidadGubernamentalServicio entidadServicio)
    {
        _entidadServicio = entidadServicio;
    }

    /// <summary>Lista paginada; filtros por nombre, sector y poder del Estado.</summary>
    [HttpGet]
    [ProducesResponseType<ResultadoPaginado<EntidadGubernamentalRespuesta>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResultadoPaginado<EntidadGubernamentalRespuesta>>> ListarAsync(
        [FromQuery] FiltroEntidadesGubernamentales filtro,
        CancellationToken tokenCancelacion) =>
        Ok(await _entidadServicio.ListarAsync(filtro, tokenCancelacion));

    [HttpGet("{id:guid}", Name = RUTA_OBTENER)]
    [ProducesResponseType<EntidadGubernamentalRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EntidadGubernamentalRespuesta>> ObtenerPorIdAsync(Guid id, CancellationToken tokenCancelacion) =>
        Ok(await _entidadServicio.ObtenerPorIdAsync(id, tokenCancelacion));

    [HttpPost]
    [Authorize(Roles = NombresRoles.ADMINISTRADOR)]
    [ProducesResponseType<EntidadGubernamentalRespuesta>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EntidadGubernamentalRespuesta>> CrearAsync(
        GuardarEntidadGubernamentalSolicitud solicitud,
        CancellationToken tokenCancelacion)
    {
        var entidad = await _entidadServicio.CrearAsync(solicitud, tokenCancelacion);
        return CreatedAtRoute(RUTA_OBTENER, new { id = entidad.Id }, entidad);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = NombresRoles.ADMINISTRADOR)]
    [ProducesResponseType<EntidadGubernamentalRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EntidadGubernamentalRespuesta>> ActualizarAsync(
        Guid id,
        GuardarEntidadGubernamentalSolicitud solicitud,
        CancellationToken tokenCancelacion) =>
        Ok(await _entidadServicio.ActualizarAsync(id, solicitud, tokenCancelacion));

    /// <summary>Borrado lógico (EstaActiva = false).</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = NombresRoles.ADMINISTRADOR)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DesactivarAsync(Guid id, CancellationToken tokenCancelacion)
    {
        await _entidadServicio.DesactivarAsync(id, tokenCancelacion);
        return NoContent();
    }
}
