using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Interfaces.Servicios;
using SB.SistemaNomina.Aplicacion.Modelos.Empleados;
using SB.SistemaNomina.Dominio.Constantes;

namespace SB.SistemaNomina.API.Controllers;

[ApiController]
[Route("api/empleados")]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
public sealed class EmpleadosController : ControllerBase
{
    private const string RUTA_OBTENER = "ObtenerEmpleado";

    private readonly IEmpleadoServicio _empleadoServicio;

    public EmpleadosController(IEmpleadoServicio empleadoServicio)
    {
        _empleadoServicio = empleadoServicio;
    }

    /// <summary>Lista paginada con filtros; incluye el pago semanal actual.</summary>
    [HttpGet]
    [ProducesResponseType<ResultadoPaginado<EmpleadoRespuesta>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResultadoPaginado<EmpleadoRespuesta>>> ListarAsync(
        [FromQuery] FiltroEmpleados filtro,
        CancellationToken tokenCancelacion) =>
        Ok(await _empleadoServicio.ListarAsync(filtro, tokenCancelacion));

    /// <summary>Empleado con el desglose de su pago semanal.</summary>
    [HttpGet("{id:guid}", Name = RUTA_OBTENER)]
    [ProducesResponseType<EmpleadoRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmpleadoRespuesta>> ObtenerPorIdAsync(Guid id, CancellationToken tokenCancelacion) =>
        Ok(await _empleadoServicio.ObtenerPorIdAsync(id, tokenCancelacion));

    /// <summary>Crea un empleado; los campos de compensación dependen del tipo.</summary>
    [HttpPost]
    [Authorize(Roles = NombresRoles.ADMINISTRADOR)]
    [ProducesResponseType<EmpleadoRespuesta>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<EmpleadoRespuesta>> CrearAsync(
        GuardarEmpleadoSolicitud solicitud,
        CancellationToken tokenCancelacion)
    {
        var empleado = await _empleadoServicio.CrearAsync(solicitud, tokenCancelacion);
        return CreatedAtRoute(RUTA_OBTENER, new { id = empleado.Id }, empleado);
    }

    /// <summary>Actualiza un empleado; admite cambio de tipo (conserva el Id).</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = NombresRoles.ADMINISTRADOR)]
    [ProducesResponseType<EmpleadoRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<EmpleadoRespuesta>> ActualizarAsync(
        Guid id,
        GuardarEmpleadoSolicitud solicitud,
        CancellationToken tokenCancelacion) =>
        Ok(await _empleadoServicio.ActualizarAsync(id, solicitud, tokenCancelacion));

    /// <summary>Borrado lógico: el empleado pasa a Inactivo.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = NombresRoles.ADMINISTRADOR)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DesactivarAsync(Guid id, CancellationToken tokenCancelacion)
    {
        await _empleadoServicio.DesactivarAsync(id, tokenCancelacion);
        return NoContent();
    }

    /// <summary>Reactiva un empleado inactivo; se rechaza si su departamento está inactivo.</summary>
    [HttpPut("{id:guid}/activar")]
    [Authorize(Roles = NombresRoles.ADMINISTRADOR)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ActivarAsync(Guid id, CancellationToken tokenCancelacion)
    {
        await _empleadoServicio.ActivarAsync(id, tokenCancelacion);
        return NoContent();
    }
}
