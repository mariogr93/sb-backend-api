using System.Globalization;
using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.Dominio.ObjetosValor;

namespace SB.SistemaNomina.PruebasUnitarias.Dominio;

public class DesglosePagoPruebas
{
    [Fact]
    public void MedioCentavoRedondeaHaciaArriba()
    {
        var desglose = new DesglosePago(
        [
            new LineaDesglosePago("A", "a", 1.005m),
            new LineaDesglosePago("B", "b", 2.675m)
        ]);

        // Valores calculados a mano: AwayFromZero, no ToEven (1.00 / 2.68).
        Assert.Equal([1.01m, 2.68m], desglose.Lineas.Select(linea => linea.Monto));
        Assert.Equal(3.69m, desglose.Total);
    }

    [Theory]
    [InlineData("1.004", "1.00")]
    [InlineData("1.006", "1.01")]
    [InlineData("-1.005", "-1.01")]
    public void MontoSeRedondeaADosDecimales(string monto, string esperado)
    {
        var resultado = DesglosePago.RedondearMoneda(decimal.Parse(monto, CultureInfo.InvariantCulture));

        Assert.Equal(decimal.Parse(esperado, CultureInfo.InvariantCulture), resultado);
    }

    [Fact]
    public void TotalSumaLineasYaRedondeadas()
    {
        // Sin redondear: 0.333 × 3 = 0.999 → 1.00; redondeando cada línea: 0.33 × 3 = 0.99.
        var desglose = new DesglosePago(
        [
            new LineaDesglosePago("A", "a", 0.333m),
            new LineaDesglosePago("B", "b", 0.333m),
            new LineaDesglosePago("C", "c", 0.333m)
        ]);

        Assert.Equal(0.99m, desglose.Total);
    }

    [Fact]
    public void FormulaUsaPuntoDecimalYPrecisionCompletaConCulturaDeComa()
    {
        var culturaOriginal = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("es-ES");
            var empleado = new EmpleadoPorComision { VentasBrutas = 1234.5m, TarifaComision = 0.0123456789m };

            var linea = Assert.Single(empleado.CalcularPagoSemanal().Lineas);

            Assert.Equal("1234.5 × 0.0123456789", linea.Formula);
            // 1234.5 × 0.0123456789 = 15.24074060... → 15.24
            Assert.Equal(15.24m, linea.Monto);

            // Límite de decimal: 28 decimales visibles en la fórmula.
            var empleadoPrecision = new EmpleadoPorComision { VentasBrutas = 1m, TarifaComision = 0.1234567890123456789012345678m };
            var lineaPrecision = Assert.Single(empleadoPrecision.CalcularPagoSemanal().Lineas);
            Assert.Equal("1 × 0.1234567890123456789012345678", lineaPrecision.Formula);
            Assert.Equal(0.12m, lineaPrecision.Monto);
        }
        finally
        {
            CultureInfo.CurrentCulture = culturaOriginal;
        }
    }
}
