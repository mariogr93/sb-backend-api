using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Modelos.Departamentos;

namespace SB.SistemaNomina.Aplicacion.Validadores.Departamentos;

public sealed class GuardarDepartamentoSolicitudValidador : ValidadorBase<GuardarDepartamentoSolicitud>
{
    public GuardarDepartamentoSolicitudValidador()
    {
        RuleFor(solicitud => solicitud.Nombre).ReglasTextoRequerido(ConstantesValidacion.LONGITUD_MAXIMA_NOMBRE_DEPARTAMENTO);
    }
}
