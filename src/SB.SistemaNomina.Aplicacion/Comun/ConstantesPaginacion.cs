namespace SB.SistemaNomina.Aplicacion.Comun;

public static class ConstantesPaginacion
{
    public const int PAGINA_INICIAL = 1;

    public const int TAMANO_PAGINA_PREDETERMINADO = 20;

    public const int TAMANO_PAGINA_MAXIMO = 100;

    /// <summary>Tope de página; evita desbordar int en CantidadOmitir.</summary>
    public const int PAGINA_MAXIMA = 1_000_000;
}
