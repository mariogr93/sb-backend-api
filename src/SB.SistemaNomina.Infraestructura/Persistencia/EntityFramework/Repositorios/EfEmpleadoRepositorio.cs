using Microsoft.EntityFrameworkCore;
using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Interfaces.Repositorios;
using SB.SistemaNomina.Aplicacion.Modelos.Empleados;
using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.Dominio.Enumeraciones;
using SB.SistemaNomina.Infraestructura.Persistencia.EntityFramework.Configuraciones;

namespace SB.SistemaNomina.Infraestructura.Persistencia.EntityFramework.Repositorios;

public sealed class EfEmpleadoRepositorio : IEmpleadoRepositorio
{
    private readonly NominaContexto _contexto;

    public EfEmpleadoRepositorio(NominaContexto contexto)
    {
        _contexto = contexto;
    }

    public async Task<ResultadoPaginado<Empleado>> ListarAsync(FiltroEmpleados filtro, CancellationToken tokenCancelacion)
    {
        var consulta = _contexto.Empleados.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(filtro.Nombre))
        {
            var nombre = filtro.Nombre.Trim();
            consulta = consulta.Where(empleado => (empleado.PrimerNombre + " " + empleado.ApellidoPaterno).Contains(nombre));
        }

        if (filtro.DepartamentoId is { } departamentoId)
        {
            consulta = consulta.Where(empleado => empleado.DepartamentoId == departamentoId);
        }

        if (filtro.Estado is { } estado)
        {
            consulta = consulta.Where(empleado => empleado.Estado == estado);
        }

        if (filtro.Tipo is { } tipo)
        {
            // Valor exacto del discriminador: PorComision no incluye a su subclase.
            var discriminador = tipo.ToString();
            consulta = consulta.Where(empleado => EF.Property<string>(empleado, ConstantesColumnas.DISCRIMINADOR_EMPLEADO) == discriminador);
        }

        var total = await consulta.CountAsync(tokenCancelacion);
        var elementos = await consulta
            .OrderBy(empleado => empleado.ApellidoPaterno)
            .ThenBy(empleado => empleado.PrimerNombre)
            .ThenBy(empleado => empleado.Id)
            .Skip(filtro.CantidadOmitir)
            .Take(filtro.TamanoPagina)
            .ToListAsync(tokenCancelacion);

        return new ResultadoPaginado<Empleado>(elementos, filtro.Pagina, filtro.TamanoPagina, total);
    }

    public async Task<IReadOnlyList<Empleado>> ListarActivosAsync(Guid? departamentoId, CancellationToken tokenCancelacion) =>
        await _contexto.Empleados.AsNoTracking()
            .Where(empleado => empleado.Estado == EstadoEmpleado.Activo && (departamentoId == null || empleado.DepartamentoId == departamentoId))
            .ToListAsync(tokenCancelacion);

    public Task<Empleado?> ObtenerPorIdAsync(Guid id, CancellationToken tokenCancelacion) =>
        _contexto.Empleados.AsNoTracking().FirstOrDefaultAsync(empleado => empleado.Id == id, tokenCancelacion);

    public Task<bool> ExisteCedulaAsync(string cedula, Guid? idExcluido, CancellationToken tokenCancelacion) =>
        _contexto.Empleados.AnyAsync(empleado => empleado.Id != idExcluido && empleado.Cedula == cedula, tokenCancelacion);

    public Task AgregarAsync(Empleado empleado, CancellationToken tokenCancelacion)
    {
        _contexto.Empleados.Add(empleado);
        return _contexto.GuardarAsync(tokenCancelacion);
    }

    public Task AgregarVariosAsync(IReadOnlyCollection<Empleado> empleados, CancellationToken tokenCancelacion)
    {
        _contexto.Empleados.AddRange(empleados);
        return _contexto.GuardarAsync(tokenCancelacion);
    }

    /// <summary>Cambio de tipo = otra subclase con el mismo Id: se borra y reinserta en una transacción.</summary>
    public async Task ActualizarAsync(Empleado empleado, CancellationToken tokenCancelacion)
    {
        var discriminadorActual = await _contexto.Empleados.AsNoTracking()
            .Where(existente => existente.Id == empleado.Id)
            .Select(existente => EF.Property<string>(existente, ConstantesColumnas.DISCRIMINADOR_EMPLEADO))
            .FirstOrDefaultAsync(tokenCancelacion);

        if (discriminadorActual == empleado.Tipo.ToString())
        {
            _contexto.Empleados.Update(empleado);
            await _contexto.GuardarAsync(tokenCancelacion);
            return;
        }

        await using var transaccion = await _contexto.Database.BeginTransactionAsync(tokenCancelacion);
        await _contexto.Empleados.Where(existente => existente.Id == empleado.Id).ExecuteDeleteAsync(tokenCancelacion);
        _contexto.Empleados.Add(empleado);
        await _contexto.GuardarAsync(tokenCancelacion);
        await transaccion.CommitAsync(tokenCancelacion);
    }
}
