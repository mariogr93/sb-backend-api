using SB.SistemaNomina.Dominio.Constantes;
using SB.SistemaNomina.Dominio.Enumeraciones;
using SB.SistemaNomina.Dominio.ObjetosValor;

namespace SB.SistemaNomina.Dominio.Entidades;

/// <summary>Pago = comisión + salarioBase + (salarioBase × 0.10).</summary>
public class EmpleadoAsalariadoPorComision : EmpleadoPorComision
{
    public decimal SalarioBase { get; set; }

    public override TipoEmpleado Tipo => TipoEmpleado.AsalariadoPorComision;

    public override DesglosePago CalcularPagoSemanal() =>
        new(
        [
            CrearLineaComision(),
            new LineaDesglosePago("Salario base", FormatearNumero(SalarioBase), SalarioBase),
            new LineaDesglosePago(
                "Bono sobre salario base",
                $"{FormatearNumero(SalarioBase)} × {FormatearNumero(ConstantesNomina.TASA_BONO_SALARIO)}",
                SalarioBase * ConstantesNomina.TASA_BONO_SALARIO)
        ]);
}
