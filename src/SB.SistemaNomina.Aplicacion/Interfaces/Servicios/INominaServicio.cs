using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Modelos.Nominas;

namespace SB.SistemaNomina.Aplicacion.Interfaces.Servicios;

public interface INominaServicio
{
    Task<ReporteNominaSemanalRespuesta> GenerarReporteSemanalAsync(Guid? departamentoId, CancellationToken tokenCancelacion);

    /// <summary>Congela el reporte de la semana; una sola nómina por semana.</summary>
    Task<NominaRespuesta> GuardarNominaAsync(GuardarNominaSolicitud solicitud, CancellationToken tokenCancelacion);

    Task<ResultadoPaginado<NominaResumenRespuesta>> ListarNominasAsync(SolicitudPaginada paginacion, CancellationToken tokenCancelacion);

    Task<NominaRespuesta> ObtenerNominaPorIdAsync(Guid id, CancellationToken tokenCancelacion);
}
