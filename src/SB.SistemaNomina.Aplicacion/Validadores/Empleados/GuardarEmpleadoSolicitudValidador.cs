using FluentValidation;
using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Modelos.Empleados;
using SB.SistemaNomina.Dominio.Enumeraciones;

namespace SB.SistemaNomina.Aplicacion.Validadores.Empleados;

/// <summary>Datos comunes + delega la compensación al validador del tipo.</summary>
public sealed class GuardarEmpleadoSolicitudValidador : ValidadorBase<GuardarEmpleadoSolicitud>
{
    private readonly IReadOnlyDictionary<TipoEmpleado, IValidadorCompensacionEmpleado> _validadoresCompensacion;

    public GuardarEmpleadoSolicitudValidador(IEnumerable<IValidadorCompensacionEmpleado> validadoresCompensacion)
    {
        _validadoresCompensacion = validadoresCompensacion.ToDictionary(validador => validador.Tipo);

        RuleFor(solicitud => solicitud.PrimerNombre).ReglasNombrePersona();

        RuleFor(solicitud => solicitud.ApellidoPaterno).ReglasNombrePersona();

        RuleFor(solicitud => solicitud.NumeroSeguroSocial)
            .NotEmpty().WithMessage(MensajesValidacion.CAMPO_REQUERIDO)
            .Matches(ConstantesValidacion.PATRON_NUMERO_SEGURO_SOCIAL).WithMessage(MensajesValidacion.NUMERO_SEGURO_SOCIAL_INVALIDO);

        RuleFor(solicitud => solicitud.DepartamentoId)
            .NotEmpty().WithMessage(MensajesValidacion.CAMPO_REQUERIDO);

        RuleFor(solicitud => solicitud.Tipo)
            .Must(_validadoresCompensacion.ContainsKey).WithMessage(MensajesValidacion.TIPO_EMPLEADO_NO_SOPORTADO);

        RuleFor(solicitud => solicitud.Compensacion)
            .NotNull().WithMessage(MensajesValidacion.CAMPO_REQUERIDO)
            .CustomAsync(ValidarCompensacionAsync);
    }

    private async Task ValidarCompensacionAsync(
        CompensacionEmpleado? compensacion,
        ValidationContext<GuardarEmpleadoSolicitud> contexto,
        CancellationToken tokenCancelacion)
    {
        var solicitud = contexto.InstanceToValidate;

        if (compensacion is null || !_validadoresCompensacion.TryGetValue(solicitud.Tipo, out var validador))
        {
            return;
        }

        var resultado = await validador.ValidateAsync(solicitud, tokenCancelacion);

        foreach (var error in resultado.Errors)
        {
            contexto.AddFailure(error);
        }
    }
}
