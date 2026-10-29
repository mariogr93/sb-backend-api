using SB.SistemaNomina.Aplicacion.Interfaces.Repositorios;
using SB.SistemaNomina.Dominio.Entidades;

namespace SB.SistemaNomina.Infraestructura.Persistencia.Json;

public sealed class JsonUsuarioRepositorio : IUsuarioRepositorio
{
    private readonly AlmacenArchivoJson<Usuario> _almacen;

    public JsonUsuarioRepositorio(AlmacenArchivoJson<Usuario> almacen)
    {
        _almacen = almacen;
    }

    public Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken tokenCancelacion) =>
        _almacen.ListarAsync(
            usuarios => usuarios.OrderBy(usuario => usuario.NombreUsuario, StringComparer.OrdinalIgnoreCase),
            tokenCancelacion);

    public Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken tokenCancelacion) =>
        _almacen.ObtenerAsync(id, tokenCancelacion);

    public Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, CancellationToken tokenCancelacion) =>
        _almacen.BuscarAsync(usuario => string.Equals(usuario.NombreUsuario, nombreUsuario, StringComparison.OrdinalIgnoreCase), tokenCancelacion);

    public Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, CancellationToken tokenCancelacion) =>
        _almacen.ExisteAsync(usuario => string.Equals(usuario.NombreUsuario, nombreUsuario, StringComparison.OrdinalIgnoreCase), tokenCancelacion);

    public Task<int> ContarAdministradoresActivosAsync(CancellationToken tokenCancelacion) =>
        _almacen.ContarAsync(usuario => usuario is { EsAdministrador: true, EstaActivo: true }, tokenCancelacion);

    public Task AgregarAsync(Usuario usuario, CancellationToken tokenCancelacion) =>
        _almacen.GuardarAsync(usuario, tokenCancelacion);

    public Task ActualizarAsync(Usuario usuario, CancellationToken tokenCancelacion) =>
        _almacen.GuardarAsync(usuario, tokenCancelacion);
}
