using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Interfaces.Servicios;
using SB.SistemaNomina.Aplicacion.Modelos.Nominas;
using SB.SistemaNomina.Dominio.Constantes;

namespace SB.SistemaNomina.API.Controllers;

[ApiController]
[Route("api/reportes-nomina")]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
public sealed class ReportesNominaController : ControllerBase
{
    private const string RUTA_OBTENER_NOMINA = "ObtenerNomina";

    private readonly INominaServicio _nominaServicio;

    public ReportesNominaController(INominaServicio nominaServicio)
    {
        _nominaServicio = nominaServicio;
    }

    /// <summary>Reporte semanal en vivo de empleados activos: desglose, totales por tipo y total general.</summary>
    [HttpGet("semanal")]
    [ProducesResponseType<ReporteNominaSemanalRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReporteNominaSemanalRespuesta>> GenerarReporteSemanalAsync(
        [FromQuery] Guid? departamentoId,
        CancellationToken tokenCancelacion) =>
        Ok(await _nominaServicio.GenerarReporteSemanalAsync(departamentoId, tokenCancelacion));

    /// <summary>Guarda la nómina de la semana (una por semana).</summary>
    [HttpPost("nominas")]
    [Authorize(Roles = NombresRoles.ADMINISTRADOR)]
    [ProducesResponseType<NominaRespuesta>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<NominaRespuesta>> GuardarNominaAsync(
        GuardarNominaSolicitud solicitud,
        CancellationToken tokenCancelacion)
    {
        var nomina = await _nominaServicio.GuardarNominaAsync(solicitud, tokenCancelacion);
        return CreatedAtRoute(RUTA_OBTENER_NOMINA, new { id = nomina.Id }, nomina);
    }

    /// <summary>Historial de nóminas guardadas.</summary>
    [HttpGet("nominas")]
    [ProducesResponseType<ResultadoPaginado<NominaResumenRespuesta>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResultadoPaginado<NominaResumenRespuesta>>> ListarNominasAsync(
        [FromQuery] SolicitudPaginada paginacion,
        CancellationToken tokenCancelacion) =>
        Ok(await _nominaServicio.ListarNominasAsync(paginacion, tokenCancelacion));

    [HttpGet("nominas/{id:guid}", Name = RUTA_OBTENER_NOMINA)]
    [ProducesResponseType<NominaRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NominaRespuesta>> ObtenerNominaPorIdAsync(Guid id, CancellationToken tokenCancelacion) =>
        Ok(await _nominaServicio.ObtenerNominaPorIdAsync(id, tokenCancelacion));
}
