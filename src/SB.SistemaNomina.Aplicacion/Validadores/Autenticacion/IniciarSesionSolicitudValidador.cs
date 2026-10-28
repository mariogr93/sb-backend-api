using FluentValidation;
using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Modelos.Autenticacion;

namespace SB.SistemaNomina.Aplicacion.Validadores.Autenticacion;

/// <summary>Solo presencia y largo: no revela la política de contraseñas al iniciar sesión.</summary>
public sealed class IniciarSesionSolicitudValidador : ValidadorBase<IniciarSesionSolicitud>
{
    public IniciarSesionSolicitudValidador()
    {
        RuleFor(solicitud => solicitud.NombreUsuario)
            .NotEmpty().WithMessage(MensajesValidacion.CAMPO_REQUERIDO)
            .MaximumLength(ConstantesValidacion.LONGITUD_MAXIMA_FILTRO_TEXTO).WithMessage(MensajesValidacion.LONGITUD_MAXIMA);

        RuleFor(solicitud => solicitud.Contrasena)
            .NotEmpty().WithMessage(MensajesValidacion.CAMPO_REQUERIDO)
            .MaximumLength(ConstantesValidacion.LONGITUD_MAXIMA_CONTRASENA).WithMessage(MensajesValidacion.LONGITUD_MAXIMA);
    }
}
