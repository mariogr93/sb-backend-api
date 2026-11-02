using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Dominio.Enumeraciones;

namespace SB.SistemaNomina.Aplicacion.Modelos.Empleados;

public sealed class FiltroEmpleados : SolicitudPaginada
{
    /// <summary>Busca en primer nombre + apellido, sin distinguir mayúsculas.</summary>
    public string? Nombre { get; init; }

    public Guid? DepartamentoId { get; init; }

    public EstadoEmpleado? Estado { get; init; }

    public TipoEmpleado? Tipo { get; init; }
}
