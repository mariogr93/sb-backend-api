using FluentValidation;
using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Dominio.Constantes;

namespace SB.SistemaNomina.Aplicacion.Validadores;

/// <summary>Reglas reutilizadas por varios validadores.</summary>
public static class ExtensionesReglas
{
    public static IRuleBuilderOptions<T, string> ReglasTextoRequerido<T>(this IRuleBuilder<T, string> regla, int longitudMaxima) =>
        regla
            .NotEmpty().WithMessage(MensajesValidacion.CAMPO_REQUERIDO)
            .MaximumLength(longitudMaxima).WithMessage(MensajesValidacion.LONGITUD_MAXIMA);

    public static IRuleBuilderOptions<T, string> ReglasNombrePersona<T>(this IRuleBuilder<T, string> regla) =>
        regla
            .ReglasTextoRequerido(ConstantesValidacion.LONGITUD_MAXIMA_NOMBRE_PERSONA)
            .Matches(ConstantesValidacion.PATRON_NOMBRE_PERSONA).WithMessage(MensajesValidacion.NOMBRE_INVALIDO);

    public static IRuleBuilderOptions<T, string> ReglasContrasena<T>(this IRuleBuilder<T, string> regla) =>
        regla
            .NotEmpty().WithMessage(MensajesValidacion.CAMPO_REQUERIDO)
            .Length(ConstantesValidacion.LONGITUD_MINIMA_CONTRASENA, ConstantesValidacion.LONGITUD_MAXIMA_CONTRASENA)
                .WithMessage(MensajesValidacion.CONTRASENA_LONGITUD)
            .Matches(ConstantesValidacion.PATRON_MAYUSCULA).WithMessage(MensajesValidacion.CONTRASENA_COMPLEJIDAD)
            .Matches(ConstantesValidacion.PATRON_MINUSCULA).WithMessage(MensajesValidacion.CONTRASENA_COMPLEJIDAD)
            .Matches(ConstantesValidacion.PATRON_DIGITO).WithMessage(MensajesValidacion.CONTRASENA_COMPLEJIDAD);

    public static IRuleBuilderOptions<T, string> ReglasRol<T>(this IRuleBuilder<T, string> regla) =>
        regla.Must(NombresRoles.TODOS.Contains).WithMessage(MensajesValidacion.ROL_INVALIDO);

    public static IRuleBuilderOptions<T, string?> ReglasFiltroTexto<T>(this IRuleBuilder<T, string?> regla) =>
        regla.MaximumLength(ConstantesValidacion.LONGITUD_MAXIMA_FILTRO_TEXTO).WithMessage(MensajesValidacion.LONGITUD_MAXIMA);
}
