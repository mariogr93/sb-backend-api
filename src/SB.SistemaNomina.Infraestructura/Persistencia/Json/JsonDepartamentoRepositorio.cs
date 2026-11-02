using SB.SistemaNomina.Aplicacion.Interfaces.Repositorios;
using SB.SistemaNomina.Dominio.Entidades;

namespace SB.SistemaNomina.Infraestructura.Persistencia.Json;

public sealed class JsonDepartamentoRepositorio : IDepartamentoRepositorio
{
    private readonly AlmacenArchivoJson<Departamento> _almacen;

    public JsonDepartamentoRepositorio(AlmacenArchivoJson<Departamento> almacen)
    {
        _almacen = almacen;
    }

    public Task<IReadOnlyList<Departamento>> ListarAsync(bool incluirInactivos, CancellationToken tokenCancelacion) =>
        _almacen.ListarAsync(
            departamentos => departamentos
                .Where(departamento => incluirInactivos || departamento.EstaActivo)
                .OrderBy(departamento => departamento.Nombre, StringComparer.InvariantCultureIgnoreCase),
            tokenCancelacion);

    public Task<Departamento?> ObtenerPorIdAsync(Guid id, CancellationToken tokenCancelacion) =>
        _almacen.ObtenerAsync(id, tokenCancelacion);

    public Task<bool> ExisteNombreAsync(string nombre, Guid? idExcluido, CancellationToken tokenCancelacion) =>
        _almacen.ExisteAsync(
            departamento => departamento.Id != idExcluido && ComparadorTexto.SonIguales(departamento.Nombre, nombre),
            tokenCancelacion);

    public Task AgregarAsync(Departamento departamento, CancellationToken tokenCancelacion) =>
        _almacen.GuardarAsync(departamento, tokenCancelacion);

    public Task AgregarVariosAsync(IReadOnlyCollection<Departamento> departamentos, CancellationToken tokenCancelacion) =>
        _almacen.GuardarVariosAsync(departamentos, tokenCancelacion);

    public Task ActualizarAsync(Departamento departamento, CancellationToken tokenCancelacion) =>
        _almacen.GuardarAsync(departamento, tokenCancelacion);
}
