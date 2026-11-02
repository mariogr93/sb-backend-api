using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Modelos.Nominas;
using SB.SistemaNomina.Dominio.Entidades;

namespace SB.SistemaNomina.Aplicacion.Interfaces.Repositorios;

public interface INominaRepositorio
{
    /// <summary>Resúmenes sin detalles, más recientes primero.</summary>
    Task<ResultadoPaginado<NominaResumenRespuesta>> ListarResumenesAsync(SolicitudPaginada paginacion, CancellationToken tokenCancelacion);

    Task<Nomina?> ObtenerPorIdAsync(Guid id, CancellationToken tokenCancelacion);

    Task<bool> ExisteSemanaAsync(DateOnly fechaInicioSemana, CancellationToken tokenCancelacion);

    Task AgregarAsync(Nomina nomina, CancellationToken tokenCancelacion);
}
