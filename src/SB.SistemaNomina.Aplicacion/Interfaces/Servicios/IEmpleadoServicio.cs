using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Modelos.Empleados;

namespace SB.SistemaNomina.Aplicacion.Interfaces.Servicios;

public interface IEmpleadoServicio
{
    Task<ResultadoPaginado<EmpleadoRespuesta>> ListarAsync(FiltroEmpleados filtro, CancellationToken tokenCancelacion);

    Task<EmpleadoRespuesta> ObtenerPorIdAsync(Guid id, CancellationToken tokenCancelacion);

    Task<EmpleadoRespuesta> CrearAsync(GuardarEmpleadoSolicitud solicitud, CancellationToken tokenCancelacion);

    Task<EmpleadoRespuesta> ActualizarAsync(Guid id, GuardarEmpleadoSolicitud solicitud, CancellationToken tokenCancelacion);

    /// <summary>Borrado lógico: estado Inactivo.</summary>
    Task DesactivarAsync(Guid id, CancellationToken tokenCancelacion);
}
