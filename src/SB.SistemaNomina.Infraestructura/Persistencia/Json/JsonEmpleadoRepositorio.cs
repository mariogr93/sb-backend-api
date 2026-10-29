using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Interfaces.Repositorios;
using SB.SistemaNomina.Aplicacion.Modelos.Empleados;
using SB.SistemaNomina.Dominio.Entidades;

namespace SB.SistemaNomina.Infraestructura.Persistencia.Json;

public sealed class JsonEmpleadoRepositorio : IEmpleadoRepositorio
{
    private readonly AlmacenArchivoJson<Empleado> _almacen;

    public JsonEmpleadoRepositorio(AlmacenArchivoJson<Empleado> almacen)
    {
        _almacen = almacen;
    }

    public async Task<ResultadoPaginado<Empleado>> ListarAsync(FiltroEmpleados filtro, CancellationToken tokenCancelacion)
    {
        var (elementos, total) = await _almacen.PaginarAsync(
            empleados => empleados
                .Where(empleado => ComparadorTexto.Contiene(empleado.NombreCompleto, filtro.Nombre))
                .Where(empleado => filtro.DepartamentoId is null || empleado.DepartamentoId == filtro.DepartamentoId)
                .Where(empleado => filtro.Estado is null || empleado.Estado == filtro.Estado)
                .Where(empleado => filtro.Tipo is null || empleado.Tipo == filtro.Tipo)
                .OrderBy(empleado => empleado.ApellidoPaterno, StringComparer.InvariantCultureIgnoreCase)
                .ThenBy(empleado => empleado.PrimerNombre, StringComparer.InvariantCultureIgnoreCase),
            filtro.CantidadOmitir,
            filtro.TamanoPagina,
            tokenCancelacion);

        return new ResultadoPaginado<Empleado>(elementos, filtro.Pagina, filtro.TamanoPagina, total);
    }

    public Task<IReadOnlyList<Empleado>> ListarActivosAsync(Guid? departamentoId, CancellationToken tokenCancelacion) =>
        _almacen.ListarAsync(
            empleados => empleados.Where(empleado =>
                empleado.EstaActivo && (departamentoId is null || empleado.DepartamentoId == departamentoId)),
            tokenCancelacion);

    public Task<Empleado?> ObtenerPorIdAsync(Guid id, CancellationToken tokenCancelacion) =>
        _almacen.ObtenerAsync(id, tokenCancelacion);

    public Task<bool> ExisteNumeroSeguroSocialAsync(string numeroSeguroSocial, Guid? idExcluido, CancellationToken tokenCancelacion) =>
        _almacen.ExisteAsync(
            empleado => empleado.Id != idExcluido && empleado.NumeroSeguroSocial == numeroSeguroSocial,
            tokenCancelacion);

    public Task AgregarAsync(Empleado empleado, CancellationToken tokenCancelacion) =>
        _almacen.GuardarAsync(empleado, tokenCancelacion);

    public Task AgregarVariosAsync(IReadOnlyCollection<Empleado> empleados, CancellationToken tokenCancelacion) =>
        _almacen.GuardarVariosAsync(empleados, tokenCancelacion);

    public Task ActualizarAsync(Empleado empleado, CancellationToken tokenCancelacion) =>
        _almacen.GuardarAsync(empleado, tokenCancelacion);
}
