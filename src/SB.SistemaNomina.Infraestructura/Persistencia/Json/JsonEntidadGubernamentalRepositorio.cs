using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Interfaces.Repositorios;
using SB.SistemaNomina.Aplicacion.Modelos.EntidadesGubernamentales;
using SB.SistemaNomina.Dominio.Entidades;

namespace SB.SistemaNomina.Infraestructura.Persistencia.Json;

public sealed class JsonEntidadGubernamentalRepositorio : IEntidadGubernamentalRepositorio
{
    private readonly AlmacenArchivoJson<EntidadGubernamental> _almacen;

    public JsonEntidadGubernamentalRepositorio(AlmacenArchivoJson<EntidadGubernamental> almacen)
    {
        _almacen = almacen;
    }

    public async Task<ResultadoPaginado<EntidadGubernamental>> ListarAsync(
        FiltroEntidadesGubernamentales filtro,
        CancellationToken tokenCancelacion)
    {
        var (elementos, total) = await _almacen.PaginarAsync(
            entidades => entidades
                .Where(entidad => filtro.IncluirInactivas || entidad.EstaActiva)
                .Where(entidad => ComparadorTexto.Contiene(entidad.Nombre, filtro.Nombre))
                .Where(entidad => ComparadorTexto.Contiene(entidad.Sector, filtro.Sector))
                .Where(entidad => ComparadorTexto.Contiene(entidad.PoderDelEstado, filtro.PoderDelEstado))
                .OrderBy(entidad => entidad.Nombre, StringComparer.InvariantCultureIgnoreCase),
            filtro.CantidadOmitir,
            filtro.TamanoPagina,
            tokenCancelacion);

        return new ResultadoPaginado<EntidadGubernamental>(elementos, filtro.Pagina, filtro.TamanoPagina, total);
    }

    public Task<EntidadGubernamental?> ObtenerPorIdAsync(Guid id, CancellationToken tokenCancelacion) =>
        _almacen.ObtenerAsync(id, tokenCancelacion);

    public Task<bool> ExisteNombreAsync(string nombre, Guid? idExcluido, CancellationToken tokenCancelacion) =>
        _almacen.ExisteAsync(
            entidad => entidad.Id != idExcluido && ComparadorTexto.SonIguales(entidad.Nombre, nombre),
            tokenCancelacion);

    public Task AgregarAsync(EntidadGubernamental entidad, CancellationToken tokenCancelacion) =>
        _almacen.GuardarAsync(entidad, tokenCancelacion);

    public Task AgregarVariosAsync(IReadOnlyCollection<EntidadGubernamental> entidades, CancellationToken tokenCancelacion) =>
        _almacen.GuardarVariosAsync(entidades, tokenCancelacion);

    public Task ActualizarAsync(EntidadGubernamental entidad, CancellationToken tokenCancelacion) =>
        _almacen.GuardarAsync(entidad, tokenCancelacion);
}
