using Microsoft.EntityFrameworkCore;
using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Interfaces.Repositorios;
using SB.SistemaNomina.Aplicacion.Modelos.Nominas;
using SB.SistemaNomina.Dominio.Constantes;
using SB.SistemaNomina.Dominio.Entidades;

namespace SB.SistemaNomina.Infraestructura.Persistencia.EntityFramework.Repositorios;

public sealed class EfUsuarioRepositorio : IUsuarioRepositorio
{
    private readonly NominaContexto _contexto;

    public EfUsuarioRepositorio(NominaContexto contexto)
    {
        _contexto = contexto;
    }

    public async Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken tokenCancelacion) =>
        await _contexto.Usuarios.AsNoTracking().OrderBy(usuario => usuario.NombreUsuario).ToListAsync(tokenCancelacion);

    public Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken tokenCancelacion) =>
        _contexto.Usuarios.AsNoTracking().FirstOrDefaultAsync(usuario => usuario.Id == id, tokenCancelacion);

    // Colación sin mayúsculas: "ADMIN" encuentra "admin".
    public Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, CancellationToken tokenCancelacion) =>
        _contexto.Usuarios.AsNoTracking().FirstOrDefaultAsync(usuario => usuario.NombreUsuario == nombreUsuario, tokenCancelacion);

    public Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, CancellationToken tokenCancelacion) =>
        _contexto.Usuarios.AnyAsync(usuario => usuario.NombreUsuario == nombreUsuario, tokenCancelacion);

    public Task<int> ContarAdministradoresActivosAsync(CancellationToken tokenCancelacion) =>
        _contexto.Usuarios.CountAsync(usuario => usuario.Rol == NombresRoles.ADMINISTRADOR && usuario.EstaActivo, tokenCancelacion);

    public Task AgregarAsync(Usuario usuario, CancellationToken tokenCancelacion)
    {
        _contexto.Usuarios.Add(usuario);
        return _contexto.GuardarAsync(tokenCancelacion);
    }

    public Task ActualizarAsync(Usuario usuario, CancellationToken tokenCancelacion)
    {
        _contexto.Usuarios.Update(usuario);
        return _contexto.GuardarAsync(tokenCancelacion);
    }
}

public sealed class EfNominaRepositorio : INominaRepositorio
{
    private readonly NominaContexto _contexto;

    public EfNominaRepositorio(NominaContexto contexto)
    {
        _contexto = contexto;
    }

    public async Task<ResultadoPaginado<NominaResumenRespuesta>> ListarResumenesAsync(SolicitudPaginada paginacion, CancellationToken tokenCancelacion)
    {
        var total = await _contexto.Nominas.CountAsync(tokenCancelacion);
        // Proyección en SQL: cuenta los detalles sin cargarlos.
        var elementos = await _contexto.Nominas
            .OrderByDescending(nomina => nomina.FechaInicioSemana)
            .Skip(paginacion.CantidadOmitir)
            .Take(paginacion.TamanoPagina)
            .Select(nomina => new NominaResumenRespuesta(
                nomina.Id, nomina.FechaInicioSemana, nomina.FechaCreacion, nomina.CreadoPor, nomina.Detalles.Count, nomina.MontoTotal))
            .ToListAsync(tokenCancelacion);

        return new ResultadoPaginado<NominaResumenRespuesta>(elementos, paginacion.Pagina, paginacion.TamanoPagina, total);
    }

    public async Task<Nomina?> ObtenerPorIdAsync(Guid id, CancellationToken tokenCancelacion)
    {
        var nomina = await ConDetalles().FirstOrDefaultAsync(nomina => nomina.Id == id, tokenCancelacion);
        return nomina is null ? null : OrdenarDetalles(nomina);
    }

    public Task<bool> ExisteSemanaAsync(DateOnly fechaInicioSemana, CancellationToken tokenCancelacion) =>
        _contexto.Nominas.AnyAsync(nomina => nomina.FechaInicioSemana == fechaInicioSemana, tokenCancelacion);

    public Task AgregarAsync(Nomina nomina, CancellationToken tokenCancelacion)
    {
        _contexto.Nominas.Add(nomina);
        return _contexto.GuardarAsync(tokenCancelacion);
    }

    private IQueryable<Nomina> ConDetalles() =>
        _contexto.Nominas.AsNoTracking().AsSplitQuery().Include(nomina => nomina.Detalles);

    // Mismo orden que el reporte (por nombre); SQL no garantiza el orden de las filas hijas.
    private static Nomina OrdenarDetalles(Nomina nomina)
    {
        nomina.Detalles = nomina.Detalles
            .OrderBy(detalle => detalle.NombreCompletoEmpleado, StringComparer.InvariantCultureIgnoreCase)
            .ToList();
        return nomina;
    }
}
