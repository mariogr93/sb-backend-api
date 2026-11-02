using SB.SistemaNomina.Dominio.Constantes;

namespace SB.SistemaNomina.Dominio.ObjetosValor;

public sealed class DesglosePago
{
    public DesglosePago(IEnumerable<LineaDesglosePago> lineas)
    {
        Lineas = lineas
            .Select(linea => linea with { Monto = RedondearMoneda(linea.Monto) })
            .ToList()
            .AsReadOnly();

        Total = Lineas.Sum(linea => linea.Monto);
    }

    public IReadOnlyList<LineaDesglosePago> Lineas { get; }

    /// <summary>Suma de líneas redondeadas: el total cuadra con el detalle.</summary>
    public decimal Total { get; }

    public static decimal RedondearMoneda(decimal monto) =>
        Math.Round(monto, ConstantesNomina.DECIMALES_MONEDA, MidpointRounding.AwayFromZero);
}
