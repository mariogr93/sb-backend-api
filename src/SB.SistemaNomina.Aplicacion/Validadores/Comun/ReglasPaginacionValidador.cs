using FluentValidation;
using SB.SistemaNomina.Aplicacion.Comun;

namespace SB.SistemaNomina.Aplicacion.Validadores.Comun;

public abstract class ReglasPaginacionValidador<T> : ValidadorBase<T> where T : SolicitudPaginada
{
    protected ReglasPaginacionValidador()
    {
        RuleFor(solicitud => solicitud.Pagina)
            .InclusiveBetween(ConstantesPaginacion.PAGINA_INICIAL, ConstantesPaginacion.PAGINA_MAXIMA).WithMessage(MensajesValidacion.PAGINA_INVALIDA);

        RuleFor(solicitud => solicitud.TamanoPagina)
            .InclusiveBetween(ConstantesPaginacion.PAGINA_INICIAL, ConstantesPaginacion.TAMANO_PAGINA_MAXIMO)
            .WithMessage(MensajesValidacion.TAMANO_PAGINA_INVALIDO);
    }
}
