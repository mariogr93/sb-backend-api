using SB.SistemaNomina.Aplicacion.Modelos.EntidadesGubernamentales;
using SB.SistemaNomina.Aplicacion.Validadores.Comun;

namespace SB.SistemaNomina.Aplicacion.Validadores.EntidadesGubernamentales;

public sealed class FiltroEntidadesGubernamentalesValidador : ReglasPaginacionValidador<FiltroEntidadesGubernamentales>
{
    public FiltroEntidadesGubernamentalesValidador()
    {
        RuleFor(filtro => filtro.Nombre).ReglasFiltroTexto();

        RuleFor(filtro => filtro.Sector).ReglasFiltroTexto();

        RuleFor(filtro => filtro.PoderDelEstado).ReglasFiltroTexto();
    }
}
