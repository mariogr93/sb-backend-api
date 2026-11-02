using SB.SistemaNomina.Dominio.Constantes;
using SB.SistemaNomina.Dominio.Enumeraciones;
using SB.SistemaNomina.Dominio.ObjetosValor;

namespace SB.SistemaNomina.Dominio.Entidades;

/// <summary>Pago = salarioSemanal.</summary>
public class EmpleadoAsalariado : Empleado
{
    public decimal SalarioSemanal { get; set; }

    public override TipoEmpleado Tipo => TipoEmpleado.Asalariado;

    public override DesglosePago CalcularPagoSemanal() =>
        new(
        [
            new LineaDesglosePago(ConceptosPago.SALARIO_SEMANAL, FormatearNumero(SalarioSemanal), SalarioSemanal)
        ]);
}
