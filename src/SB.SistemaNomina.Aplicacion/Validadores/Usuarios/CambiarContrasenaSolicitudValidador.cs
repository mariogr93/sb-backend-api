using SB.SistemaNomina.Aplicacion.Modelos.Usuarios;

namespace SB.SistemaNomina.Aplicacion.Validadores.Usuarios;

public sealed class CambiarContrasenaSolicitudValidador : ValidadorBase<CambiarContrasenaSolicitud>
{
    public CambiarContrasenaSolicitudValidador()
    {
        RuleFor(solicitud => solicitud.ContrasenaNueva).ReglasContrasena();
    }
}
