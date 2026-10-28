namespace SB.SistemaNomina.Aplicacion.Modelos.Empleados;

/// <summary>Datos de pago; cada tipo de empleado usa solo los suyos y el resto se ignora.</summary>
public sealed record CompensacionEmpleado
{
    public decimal? SalarioSemanal { get; init; }

    public decimal? SueldoPorHora { get; init; }

    public decimal? HorasTrabajadas { get; init; }

    public decimal? VentasBrutas { get; init; }

    /// <summary>Fracción: 0.06 = 6%.</summary>
    public decimal? TarifaComision { get; init; }

    public decimal? SalarioBase { get; init; }
}
