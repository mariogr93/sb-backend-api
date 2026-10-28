using SB.SistemaNomina.Aplicacion.Comun;

namespace SB.SistemaNomina.PruebasUnitarias.Aplicacion;

public class ResultadoPaginadoPruebas
{
    [Theory]
    [InlineData(0, 20, 0)]
    [InlineData(20, 20, 1)]
    [InlineData(21, 20, 2)]
    [InlineData(40, 20, 2)]
    [InlineData(1, 0, 0)]
    public void TotalPaginasRedondeaHaciaArriba(int totalElementos, int tamanoPagina, int esperado)
    {
        var resultado = new ResultadoPaginado<int>([], 1, tamanoPagina, totalElementos);

        Assert.Equal(esperado, resultado.TotalPaginas);
    }

    [Fact]
    public void MapearTransformaElementosYConservaMetadatos()
    {
        var resultado = new ResultadoPaginado<int>([1, 2], 3, 20, 41);

        var mapeado = resultado.Mapear(numero => $"Empleado {numero}");

        Assert.Equal(["Empleado 1", "Empleado 2"], mapeado.Elementos);
        Assert.Equal(3, mapeado.Pagina);
        Assert.Equal(20, mapeado.TamanoPagina);
        Assert.Equal(41, mapeado.TotalElementos);
        Assert.Equal(3, mapeado.TotalPaginas);
    }

    [Fact]
    public void MapearListaVaciaConservaMetadatos()
    {
        var mapeado = new ResultadoPaginado<int>([], 2, 10, 15).Mapear(numero => numero.ToString());

        Assert.Empty(mapeado.Elementos);
        Assert.Equal((2, 10, 15, 2), (mapeado.Pagina, mapeado.TamanoPagina, mapeado.TotalElementos, mapeado.TotalPaginas));
    }
}
