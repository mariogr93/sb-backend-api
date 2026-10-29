using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Interfaces.Repositorios;
using SB.SistemaNomina.Dominio.Entidades;

namespace SB.SistemaNomina.Infraestructura.Persistencia.Json;

public sealed class JsonNominaRepositorio : INominaRepositorio
{
    private readonly AlmacenArchivoJson<Nomina> _almacen;

    public JsonNominaRepositorio(AlmacenArchivoJson<Nomina> almacen)
    {
        _almacen = almacen;
    }

    public async Task<ResultadoPaginado<Nomina>> ListarAsync(SolicitudPaginada paginacion, CancellationToken tokenCancelacion)
    {
        var (elementos, total) = await _almacen.PaginarAsync(
            nominas => nominas.OrderByDescending(nomina => nomina.FechaInicioSemana),
            paginacion.CantidadOmitir,
            paginacion.TamanoPagina,
            tokenCancelacion);

        return new ResultadoPaginado<Nomina>(elementos, paginacion.Pagina, paginacion.TamanoPagina, total);
    }

    public Task<Nomina?> ObtenerPorIdAsync(Guid id, CancellationToken tokenCancelacion) =>
        _almacen.ObtenerAsync(id, tokenCancelacion);

    public Task<bool> ExisteSemanaAsync(DateOnly fechaInicioSemana, CancellationToken tokenCancelacion) =>
        _almacen.ExisteAsync(nomina => nomina.FechaInicioSemana == fechaInicioSemana, tokenCancelacion);

    public Task AgregarAsync(Nomina nomina, CancellationToken tokenCancelacion) =>
        _almacen.GuardarAsync(nomina, tokenCancelacion);
}
