using System.Globalization;
using SB.SistemaNomina.Dominio.Constantes;
using SB.SistemaNomina.Dominio.Enumeraciones;
using SB.SistemaNomina.Dominio.ObjetosValor;

namespace SB.SistemaNomina.Dominio.Entidades;

/// <summary>Base abstracta. Cada tipo = subclase con su cálculo; tipo nuevo no modifica código existente.</summary>
public abstract class Empleado
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string PrimerNombre { get; set; } = string.Empty;

    public string ApellidoPaterno { get; set; } = string.Empty;

    public string NumeroSeguroSocial { get; set; } = string.Empty;

    public Guid DepartamentoId { get; set; }

    public EstadoEmpleado Estado { get; set; } = EstadoEmpleado.Activo;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }

    public abstract TipoEmpleado Tipo { get; }

    public string NombreCompleto => $"{PrimerNombre} {ApellidoPaterno}".Trim();

    public bool EstaActivo => Estado == EstadoEmpleado.Activo;

    public abstract DesglosePago CalcularPagoSemanal();

    public void Desactivar()
    {
        Estado = EstadoEmpleado.Inactivo;
        FechaActualizacion = DateTime.UtcNow;
    }

    public void Activar()
    {
        Estado = EstadoEmpleado.Activo;
        FechaActualizacion = DateTime.UtcNow;
    }

    /// <summary>Cultura invariante: misma fórmula en cualquier servidor.</summary>
    protected static string FormatearNumero(decimal valor) =>
        valor.ToString(ConstantesNomina.FORMATO_NUMERO_FORMULA, CultureInfo.InvariantCulture);
}
