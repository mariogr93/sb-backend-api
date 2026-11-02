using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Modelos.EntidadesGubernamentales;
using SB.SistemaNomina.Dominio.Entidades;

namespace SB.SistemaNomina.Aplicacion.Interfaces.Repositorios;

public interface IEntidadGubernamentalRepositorio
{
    Task<ResultadoPaginado<EntidadGubernamental>> ListarAsync(FiltroEntidadesGubernamentales filtro, CancellationToken tokenCancelacion);

    Task<EntidadGubernamental?> ObtenerPorIdAsync(Guid id, CancellationToken tokenCancelacion);

    Task<bool> ExisteNombreAsync(string nombre, Guid? idExcluido, CancellationToken tokenCancelacion);

    Task AgregarAsync(EntidadGubernamental entidad, CancellationToken tokenCancelacion);

    /// <summary>Todos o ninguno (datos iniciales).</summary>
    Task AgregarVariosAsync(IReadOnlyCollection<EntidadGubernamental> entidades, CancellationToken tokenCancelacion);

    Task ActualizarAsync(EntidadGubernamental entidad, CancellationToken tokenCancelacion);
}
