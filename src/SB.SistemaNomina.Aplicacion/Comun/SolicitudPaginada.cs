namespace SB.SistemaNomina.Aplicacion.Comun;

public class SolicitudPaginada
{
    public int Pagina { get; init; } = ConstantesPaginacion.PAGINA_INICIAL;

    public int TamanoPagina { get; init; } = ConstantesPaginacion.TAMANO_PAGINA_PREDETERMINADO;

    public int CantidadOmitir => (Pagina - ConstantesPaginacion.PAGINA_INICIAL) * TamanoPagina;
}
