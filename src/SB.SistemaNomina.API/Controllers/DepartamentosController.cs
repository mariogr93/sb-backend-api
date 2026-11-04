using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.SistemaNomina.Aplicacion.Interfaces.Servicios;
using SB.SistemaNomina.Aplicacion.Modelos.Departamentos;
using SB.SistemaNomina.Dominio.Constantes;

namespace SB.SistemaNomina.API.Controllers;

[ApiController]
[Route("api/departamentos")]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
public sealed class DepartamentosController : ControllerBase
{
    private const string RUTA_OBTENER = "ObtenerDepartamento";

    private readonly IDepartamentoServicio _departamentoServicio;

    public DepartamentosController(IDepartamentoServicio departamentoServicio)
    {
        _departamentoServicio = departamentoServicio;
    }

    /// <summary>Departamentos ordenados por nombre.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<DepartamentoRespuesta>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DepartamentoRespuesta>>> ListarAsync(
        [FromQuery] bool incluirInactivos,
        CancellationToken tokenCancelacion) =>
        Ok(await _departamentoServicio.ListarAsync(incluirInactivos, tokenCancelacion));

    [HttpGet("{id:guid}", Name = RUTA_OBTENER)]
    [ProducesResponseType<DepartamentoRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DepartamentoRespuesta>> ObtenerPorIdAsync(Guid id, CancellationToken tokenCancelacion) =>
        Ok(await _departamentoServicio.ObtenerPorIdAsync(id, tokenCancelacion));

    [HttpPost]
    [Authorize(Roles = NombresRoles.ADMINISTRADOR)]
    [ProducesResponseType<DepartamentoRespuesta>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DepartamentoRespuesta>> CrearAsync(
        GuardarDepartamentoSolicitud solicitud,
        CancellationToken tokenCancelacion)
    {
        var departamento = await _departamentoServicio.CrearAsync(solicitud, tokenCancelacion);
        return CreatedAtRoute(RUTA_OBTENER, new { id = departamento.Id }, departamento);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = NombresRoles.ADMINISTRADOR)]
    [ProducesResponseType<DepartamentoRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DepartamentoRespuesta>> ActualizarAsync(
        Guid id,
        GuardarDepartamentoSolicitud solicitud,
        CancellationToken tokenCancelacion) =>
        Ok(await _departamentoServicio.ActualizarAsync(id, solicitud, tokenCancelacion));

    /// <summary>Desactiva; se rechaza si tiene empleados activos.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = NombresRoles.ADMINISTRADOR)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DesactivarAsync(Guid id, CancellationToken tokenCancelacion)
    {
        await _departamentoServicio.DesactivarAsync(id, tokenCancelacion);
        return NoContent();
    }

    /// <summary>Reactiva un departamento inactivo.</summary>
    [HttpPut("{id:guid}/activar")]
    [Authorize(Roles = NombresRoles.ADMINISTRADOR)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivarAsync(Guid id, CancellationToken tokenCancelacion)
    {
        await _departamentoServicio.ActivarAsync(id, tokenCancelacion);
        return NoContent();
    }
}
