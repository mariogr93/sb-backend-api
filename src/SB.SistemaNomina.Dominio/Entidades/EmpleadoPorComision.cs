using SB.SistemaNomina.Dominio.Constantes;
using SB.SistemaNomina.Dominio.Enumeraciones;
using SB.SistemaNomina.Dominio.ObjetosValor;

namespace SB.SistemaNomina.Dominio.Entidades;

/// <summary>Pago = ventasBrutas × tarifaComision.</summary>
public class EmpleadoPorComision : Empleado
{
    public decimal VentasBrutas { get; set; }

    /// <summary>Fracción: 0.06 = 6%.</summary>
    public decimal TarifaComision { get; set; }

    public override TipoEmpleado Tipo => TipoEmpleado.PorComision;

    public override DesglosePago CalcularPagoSemanal() => new([CrearLineaComision()]);

    protected LineaDesglosePago CrearLineaComision() =>
        new(
            ConceptosPago.COMISION,
            $"{FormatearNumero(VentasBrutas)} × {FormatearNumero(TarifaComision)}",
            VentasBrutas * TarifaComision);
}
