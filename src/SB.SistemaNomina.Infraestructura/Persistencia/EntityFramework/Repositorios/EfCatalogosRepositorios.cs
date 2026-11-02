using Microsoft.EntityFrameworkCore;
using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Interfaces.Repositorios;
using SB.SistemaNomina.Aplicacion.Modelos.EntidadesGubernamentales;
using SB.SistemaNomina.Dominio.Entidades;

namespace SB.SistemaNomina.Infraestructura.Persistencia.EntityFramework.Repositorios;

public sealed class EfDepartamentoRepositorio : IDepartamentoRepositorio
{
    private readonly NominaContexto _contexto;

    public EfDepartamentoRepositorio(NominaContexto contexto)
    {
        _contexto = contexto;
    }

    public async Task<IReadOnlyList<Departamento>> ListarAsync(bool incluirInactivos, CancellationToken tokenCancelacion) =>
        await _contexto.Departamentos.AsNoTracking()
            .Where(departamento => incluirInactivos || departamento.EstaActivo)
            .OrderBy(departamento => departamento.Nombre)
            .ToListAsync(tokenCancelacion);

    public Task<Departamento?> ObtenerPorIdAsync(Guid id, CancellationToken tokenCancelacion) =>
        _contexto.Departamentos.AsNoTracking().FirstOrDefaultAsync(departamento => departamento.Id == id, tokenCancelacion);

    // La colación sin acentos ni mayúsculas hace la comparación.
    public Task<bool> ExisteNombreAsync(string nombre, Guid? idExcluido, CancellationToken tokenCancelacion)
    {
        var nombreLimpio = nombre.Trim();
        return _contexto.Departamentos.AnyAsync(departamento => departamento.Id != idExcluido && departamento.Nombre == nombreLimpio, tokenCancelacion);
    }

    public Task AgregarAsync(Departamento departamento, CancellationToken tokenCancelacion)
    {
        _contexto.Departamentos.Add(departamento);
        return _contexto.GuardarAsync(tokenCancelacion);
    }

    public Task AgregarVariosAsync(IReadOnlyCollection<Departamento> departamentos, CancellationToken tokenCancelacion)
    {
        _contexto.Departamentos.AddRange(departamentos);
        return _contexto.GuardarAsync(tokenCancelacion);
    }

    public Task ActualizarAsync(Departamento departamento, CancellationToken tokenCancelacion)
    {
        _contexto.Departamentos.Update(departamento);
        return _contexto.GuardarAsync(tokenCancelacion);
    }
}

public sealed class EfEntidadGubernamentalRepositorio : IEntidadGubernamentalRepositorio
{
    private readonly NominaContexto _contexto;

    public EfEntidadGubernamentalRepositorio(NominaContexto contexto)
    {
        _contexto = contexto;
    }

    public async Task<ResultadoPaginado<EntidadGubernamental>> ListarAsync(
        FiltroEntidadesGubernamentales filtro,
        CancellationToken tokenCancelacion)
    {
        var consulta = _contexto.EntidadesGubernamentales.AsNoTracking()
            .Where(entidad => filtro.IncluirInactivas || entidad.EstaActiva);
        if (!string.IsNullOrWhiteSpace(filtro.Nombre))
        {
            var nombre = filtro.Nombre.Trim();
            consulta = consulta.Where(entidad => entidad.Nombre.Contains(nombre));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Sector))
        {
            var sector = filtro.Sector.Trim();
            consulta = consulta.Where(entidad => entidad.Sector.Contains(sector));
        }

        if (!string.IsNullOrWhiteSpace(filtro.PoderDelEstado))
        {
            var poder = filtro.PoderDelEstado.Trim();
            consulta = consulta.Where(entidad => entidad.PoderDelEstado.Contains(poder));
        }

        var total = await consulta.CountAsync(tokenCancelacion);
        var elementos = await consulta
            .OrderBy(entidad => entidad.Nombre)
            .Skip(filtro.CantidadOmitir)
            .Take(filtro.TamanoPagina)
            .ToListAsync(tokenCancelacion);

        return new ResultadoPaginado<EntidadGubernamental>(elementos, filtro.Pagina, filtro.TamanoPagina, total);
    }

    public Task<EntidadGubernamental?> ObtenerPorIdAsync(Guid id, CancellationToken tokenCancelacion) =>
        _contexto.EntidadesGubernamentales.AsNoTracking().FirstOrDefaultAsync(entidad => entidad.Id == id, tokenCancelacion);

    public Task<bool> ExisteNombreAsync(string nombre, Guid? idExcluido, CancellationToken tokenCancelacion)
    {
        var nombreLimpio = nombre.Trim();
        return _contexto.EntidadesGubernamentales.AnyAsync(entidad => entidad.Id != idExcluido && entidad.Nombre == nombreLimpio, tokenCancelacion);
    }

    public Task AgregarAsync(EntidadGubernamental entidad, CancellationToken tokenCancelacion)
    {
        _contexto.EntidadesGubernamentales.Add(entidad);
        return _contexto.GuardarAsync(tokenCancelacion);
    }

    public Task AgregarVariosAsync(IReadOnlyCollection<EntidadGubernamental> entidades, CancellationToken tokenCancelacion)
    {
        _contexto.EntidadesGubernamentales.AddRange(entidades);
        return _contexto.GuardarAsync(tokenCancelacion);
    }

    public Task ActualizarAsync(EntidadGubernamental entidad, CancellationToken tokenCancelacion)
    {
        _contexto.EntidadesGubernamentales.Update(entidad);
        return _contexto.GuardarAsync(tokenCancelacion);
    }
}
