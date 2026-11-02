using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Modelos.EntidadesGubernamentales;

namespace SB.SistemaNomina.Aplicacion.Interfaces.Servicios;

public interface IEntidadGubernamentalServicio
{
    Task<ResultadoPaginado<EntidadGubernamentalRespuesta>> ListarAsync(FiltroEntidadesGubernamentales filtro, CancellationToken tokenCancelacion);

    Task<EntidadGubernamentalRespuesta> ObtenerPorIdAsync(Guid id, CancellationToken tokenCancelacion);

    Task<EntidadGubernamentalRespuesta> CrearAsync(GuardarEntidadGubernamentalSolicitud solicitud, CancellationToken tokenCancelacion);

    Task<EntidadGubernamentalRespuesta> ActualizarAsync(Guid id, GuardarEntidadGubernamentalSolicitud solicitud, CancellationToken tokenCancelacion);

    Task DesactivarAsync(Guid id, CancellationToken tokenCancelacion);
}
