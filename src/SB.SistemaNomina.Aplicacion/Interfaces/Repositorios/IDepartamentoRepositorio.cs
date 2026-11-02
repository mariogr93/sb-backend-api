using SB.SistemaNomina.Dominio.Entidades;

namespace SB.SistemaNomina.Aplicacion.Interfaces.Repositorios;

public interface IDepartamentoRepositorio
{
    Task<IReadOnlyList<Departamento>> ListarAsync(bool incluirInactivos, CancellationToken tokenCancelacion);

    Task<Departamento?> ObtenerPorIdAsync(Guid id, CancellationToken tokenCancelacion);

    Task<bool> ExisteNombreAsync(string nombre, Guid? idExcluido, CancellationToken tokenCancelacion);

    Task AgregarAsync(Departamento departamento, CancellationToken tokenCancelacion);

    /// <summary>Todos o ninguno (datos iniciales).</summary>
    Task AgregarVariosAsync(IReadOnlyCollection<Departamento> departamentos, CancellationToken tokenCancelacion);

    Task ActualizarAsync(Departamento departamento, CancellationToken tokenCancelacion);
}
