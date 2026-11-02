using SB.SistemaNomina.Dominio.Entidades;

namespace SB.SistemaNomina.Aplicacion.Interfaces.Repositorios;

public interface IUsuarioRepositorio
{
    Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken tokenCancelacion);

    Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken tokenCancelacion);

    /// <summary>Búsqueda sin distinguir mayúsculas.</summary>
    Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, CancellationToken tokenCancelacion);

    Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, CancellationToken tokenCancelacion);

    Task<int> ContarAdministradoresActivosAsync(CancellationToken tokenCancelacion);

    Task AgregarAsync(Usuario usuario, CancellationToken tokenCancelacion);

    Task ActualizarAsync(Usuario usuario, CancellationToken tokenCancelacion);
}
