using SB.SistemaNomina.Aplicacion.Modelos.Usuarios;

namespace SB.SistemaNomina.Aplicacion.Validadores.Usuarios;

public sealed class ActualizarUsuarioSolicitudValidador : ValidadorBase<ActualizarUsuarioSolicitud>
{
    public ActualizarUsuarioSolicitudValidador()
    {
        RuleFor(solicitud => solicitud.NombreCompleto).ReglasNombrePersona();

        RuleFor(solicitud => solicitud.Rol).ReglasRol();
    }
}
