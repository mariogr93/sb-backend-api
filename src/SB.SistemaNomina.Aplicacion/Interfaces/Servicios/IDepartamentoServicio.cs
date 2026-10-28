using SB.SistemaNomina.Aplicacion.Modelos.Departamentos;

namespace SB.SistemaNomina.Aplicacion.Interfaces.Servicios;

public interface IDepartamentoServicio
{
    Task<IReadOnlyList<DepartamentoRespuesta>> ListarAsync(bool incluirInactivos, CancellationToken tokenCancelacion);

    Task<DepartamentoRespuesta> ObtenerPorIdAsync(Guid id, CancellationToken tokenCancelacion);

    Task<DepartamentoRespuesta> CrearAsync(GuardarDepartamentoSolicitud solicitud, CancellationToken tokenCancelacion);

    Task<DepartamentoRespuesta> ActualizarAsync(Guid id, GuardarDepartamentoSolicitud solicitud, CancellationToken tokenCancelacion);

    Task DesactivarAsync(Guid id, CancellationToken tokenCancelacion);
}
