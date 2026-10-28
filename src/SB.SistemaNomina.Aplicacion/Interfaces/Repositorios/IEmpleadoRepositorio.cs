using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Modelos.Empleados;
using SB.SistemaNomina.Dominio.Entidades;

namespace SB.SistemaNomina.Aplicacion.Interfaces.Repositorios;

public interface IEmpleadoRepositorio
{
    Task<ResultadoPaginado<Empleado>> ListarAsync(FiltroEmpleados filtro, CancellationToken tokenCancelacion);

    /// <summary>Empleados activos para la nómina; departamentoId null = todos.</summary>
    Task<IReadOnlyList<Empleado>> ListarActivosAsync(Guid? departamentoId, CancellationToken tokenCancelacion);

    Task<Empleado?> ObtenerPorIdAsync(Guid id, CancellationToken tokenCancelacion);

    Task<bool> ExisteNumeroSeguroSocialAsync(string numeroSeguroSocial, Guid? idExcluido, CancellationToken tokenCancelacion);

    Task AgregarAsync(Empleado empleado, CancellationToken tokenCancelacion);

    /// <summary>Reemplaza el empleado con el mismo Id (admite cambio de tipo).</summary>
    Task ActualizarAsync(Empleado empleado, CancellationToken tokenCancelacion);
}
