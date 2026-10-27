using SB.SistemaNomina.Dominio.Constantes;
using SB.SistemaNomina.Dominio.Enumeraciones;
using SB.SistemaNomina.Dominio.ObjetosValor;

namespace SB.SistemaNomina.Dominio.Entidades;

/// <summary>Pago = sueldo × horas; sobre 40 h: (sueldo × 40) + (sueldo × 1.5 × horas extra).</summary>
public class EmpleadoPorHoras : Empleado
{
    public decimal SueldoPorHora { get; set; }

    public decimal HorasTrabajadas { get; set; }

    public override TipoEmpleado Tipo => TipoEmpleado.PorHoras;

    public override DesglosePago CalcularPagoSemanal()
    {
        var horasRegulares = Math.Min(HorasTrabajadas, ConstantesNomina.HORAS_SEMANALES_ESTANDAR);
        var horasExtra = Math.Max(HorasTrabajadas - ConstantesNomina.HORAS_SEMANALES_ESTANDAR, decimal.Zero);

        var lineas = new List<LineaDesglosePago>
        {
            new(
                "Horas regulares",
                $"{FormatearNumero(SueldoPorHora)} × {FormatearNumero(horasRegulares)}",
                SueldoPorHora * horasRegulares)
        };

        if (horasExtra > decimal.Zero)
        {
            lineas.Add(new(
                "Horas extra",
                $"{FormatearNumero(SueldoPorHora)} × {FormatearNumero(ConstantesNomina.MULTIPLICADOR_HORAS_EXTRA)} × {FormatearNumero(horasExtra)}",
                SueldoPorHora * ConstantesNomina.MULTIPLICADOR_HORAS_EXTRA * horasExtra));
        }

        return new DesglosePago(lineas);
    }
}
