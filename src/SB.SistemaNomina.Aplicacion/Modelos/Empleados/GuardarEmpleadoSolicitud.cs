using SB.SistemaNomina.Dominio.Enumeraciones;

namespace SB.SistemaNomina.Aplicacion.Modelos.Empleados;

/// <summary>Crear o actualizar empleado. Cambiar Tipo en una actualización conserva el Id.</summary>
public sealed record GuardarEmpleadoSolicitud
{
    public string PrimerNombre { get; init; } = string.Empty;

    public string ApellidoPaterno { get; init; } = string.Empty;

    public string NumeroSeguroSocial { get; init; } = string.Empty;

    public Guid DepartamentoId { get; init; }

    public TipoEmpleado Tipo { get; init; }

    public CompensacionEmpleado Compensacion { get; init; } = new();
}
