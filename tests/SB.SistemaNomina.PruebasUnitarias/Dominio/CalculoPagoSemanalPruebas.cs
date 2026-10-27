using SB.SistemaNomina.Dominio.Constantes;
using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.Dominio.Enumeraciones;

namespace SB.SistemaNomina.PruebasUnitarias.Dominio;

public class CalculoPagoSemanalPruebas
{
    [Fact]
    public void AsalariadoPagaSalarioSemanal()
    {
        var empleado = new EmpleadoAsalariado { SalarioSemanal = 1500m };

        var desglose = empleado.CalcularPagoSemanal();

        Assert.Equal(1500m, desglose.Total);
        Assert.Equal(ConceptosPago.SALARIO_SEMANAL, Assert.Single(desglose.Lineas).Concepto);
    }

    [Theory]
    [InlineData(500, 40, 20000)]
    [InlineData(500, 0, 0)]
    [InlineData(500, 39.5, 19750)]
    public void PorHorasHastaCuarentaHorasSinHorasExtra(decimal sueldoPorHora, decimal horasTrabajadas, decimal pagoEsperado)
    {
        var empleado = new EmpleadoPorHoras { SueldoPorHora = sueldoPorHora, HorasTrabajadas = horasTrabajadas };

        var desglose = empleado.CalcularPagoSemanal();

        Assert.Equal(pagoEsperado, desglose.Total);
        Assert.Equal(ConceptosPago.HORAS_REGULARES, Assert.Single(desglose.Lineas).Concepto);
    }

    [Fact]
    public void PorHorasSobreCuarentaHorasPagaHorasExtraAUnoPuntoCinco()
    {
        var empleado = new EmpleadoPorHoras { SueldoPorHora = 500m, HorasTrabajadas = 45m };

        var desglose = empleado.CalcularPagoSemanal();

        // (500 × 40) + (500 × 1.5 × 5) = 20,000 + 3,750
        Assert.Equal(23750m, desglose.Total);
        Assert.Equal(2, desglose.Lineas.Count);
        Assert.Equal(3750m, desglose.Lineas.Single(linea => linea.Concepto == ConceptosPago.HORAS_EXTRA).Monto);
    }

    [Fact]
    public void PorComisionPagaVentasPorTarifa()
    {
        var empleado = new EmpleadoPorComision { VentasBrutas = 10000m, TarifaComision = 0.06m };

        var desglose = empleado.CalcularPagoSemanal();

        Assert.Equal(600m, desglose.Total);
    }

    [Fact]
    public void AsalariadoPorComisionIncluyeBonoDeDiezPorCiento()
    {
        var empleado = new EmpleadoAsalariadoPorComision
        {
            VentasBrutas = 10000m,
            TarifaComision = 0.06m,
            SalarioBase = 1000m
        };

        var desglose = empleado.CalcularPagoSemanal();

        // 600 + 1,000 + 100
        Assert.Equal(1700m, desglose.Total);
        Assert.Equal(100m, desglose.Lineas.Single(linea => linea.Concepto == ConceptosPago.BONO_SALARIO_BASE).Monto);
    }

    [Fact]
    public void FormulaMuestraOperandosSinPerderPrecision()
    {
        var empleado = new EmpleadoPorComision { VentasBrutas = 1000m, TarifaComision = 0.075m };

        var linea = Assert.Single(empleado.CalcularPagoSemanal().Lineas);

        Assert.Equal("1000 × 0.075", linea.Formula);
        Assert.Equal(75m, linea.Monto);
    }

    [Fact]
    public void TotalEsIgualALaSumaDeLineasRedondeadas()
    {
        var empleado = new EmpleadoPorHoras { SueldoPorHora = 12.345m, HorasTrabajadas = 41.333m };

        var desglose = empleado.CalcularPagoSemanal();

        Assert.Equal(desglose.Lineas.Sum(linea => linea.Monto), desglose.Total);
        Assert.All(desglose.Lineas, linea => Assert.Equal(Math.Round(linea.Monto, ConstantesNomina.DECIMALES_MONEDA), linea.Monto));
    }

    [Fact]
    public void DesactivarCambiaEstadoYUsaFechaRecibida()
    {
        var empleado = new EmpleadoAsalariado();
        var fechaActualizacion = new DateTime(2026, 10, 27, 12, 0, 0, DateTimeKind.Utc);

        empleado.Desactivar(fechaActualizacion);

        Assert.Equal(EstadoEmpleado.Inactivo, empleado.Estado);
        Assert.Equal(fechaActualizacion, empleado.FechaActualizacion);
    }
}
