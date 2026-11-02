using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Modelos.EntidadesGubernamentales;

namespace SB.SistemaNomina.Aplicacion.Validadores.EntidadesGubernamentales;

public sealed class GuardarEntidadGubernamentalSolicitudValidador : ValidadorBase<GuardarEntidadGubernamentalSolicitud>
{
    public GuardarEntidadGubernamentalSolicitudValidador()
    {
        RuleFor(solicitud => solicitud.Nombre).ReglasTextoRequerido(ConstantesValidacion.LONGITUD_MAXIMA_NOMBRE_ENTIDAD);

        RuleFor(solicitud => solicitud.Categoria).ReglasTextoRequerido(ConstantesValidacion.LONGITUD_MAXIMA_CLASIFICACION_ENTIDAD);

        RuleFor(solicitud => solicitud.PoderDelEstado).ReglasTextoRequerido(ConstantesValidacion.LONGITUD_MAXIMA_CLASIFICACION_ENTIDAD);

        RuleFor(solicitud => solicitud.Sector).ReglasTextoRequerido(ConstantesValidacion.LONGITUD_MAXIMA_CLASIFICACION_ENTIDAD);
    }
}
