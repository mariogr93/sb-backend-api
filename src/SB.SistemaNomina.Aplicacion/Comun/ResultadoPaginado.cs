namespace SB.SistemaNomina.Aplicacion.Comun;

public sealed record ResultadoPaginado<T>(IReadOnlyList<T> Elementos, int Pagina, int TamanoPagina, int TotalElementos)
{
    public int TotalPaginas => TamanoPagina > 0 ? (int)Math.Ceiling(TotalElementos / (double)TamanoPagina) : 0;

    public ResultadoPaginado<TDestino> Mapear<TDestino>(Func<T, TDestino> convertir) =>
        new(Elementos.Select(convertir).ToList(), Pagina, TamanoPagina, TotalElementos);
}
