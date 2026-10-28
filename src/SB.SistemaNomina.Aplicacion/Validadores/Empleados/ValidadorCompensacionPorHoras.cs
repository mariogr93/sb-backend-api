using FluentValidation;
using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Modelos.Empleados;
using SB.SistemaNomina.Dominio.Enumeraciones;

namespace SB.SistemaNomina.Aplicacion.Validadores.Empleados;

public sealed class ValidadorCompensacionPorHoras : ValidadorBase<GuardarEmpleadoSolicitud>, IValidadorCompensacionEmpleado
{
    public ValidadorCompensacionPorHoras()
    {
        RuleFor(solicitud => solicitud.Compensacion.SueldoPorHora)
            .NotNull().WithMessage(MensajesValidacion.CAMPO_REQUERIDO)
            .InclusiveBetween(decimal.Zero, ConstantesValidacion.MONTO_MAXIMO).WithMessage(MensajesValidacion.MONTO_FUERA_RANGO);

        RuleFor(solicitud => solicitud.Compensacion.HorasTrabajadas)
            .NotNull().WithMessage(MensajesValidacion.CAMPO_REQUERIDO)
            .InclusiveBetween(decimal.Zero, ConstantesValidacion.HORAS_MAXIMAS_SEMANA).WithMessage(MensajesValidacion.HORAS_FUERA_RANGO);
    }

    public TipoEmpleado Tipo => TipoEmpleado.PorHoras;
}
