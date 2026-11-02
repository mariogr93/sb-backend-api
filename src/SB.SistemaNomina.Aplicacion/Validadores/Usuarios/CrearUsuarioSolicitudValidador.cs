using FluentValidation;
using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Modelos.Usuarios;

namespace SB.SistemaNomina.Aplicacion.Validadores.Usuarios;

public sealed class CrearUsuarioSolicitudValidador : ValidadorBase<CrearUsuarioSolicitud>
{
    public CrearUsuarioSolicitudValidador()
    {
        RuleFor(solicitud => solicitud.NombreUsuario)
            .NotEmpty().WithMessage(MensajesValidacion.CAMPO_REQUERIDO)
            .Matches(ConstantesValidacion.PATRON_NOMBRE_USUARIO).WithMessage(MensajesValidacion.NOMBRE_USUARIO_INVALIDO);

        RuleFor(solicitud => solicitud.NombreCompleto).ReglasNombrePersona();

        RuleFor(solicitud => solicitud.Contrasena).ReglasContrasena();

        RuleFor(solicitud => solicitud.Rol).ReglasRol();
    }
}
