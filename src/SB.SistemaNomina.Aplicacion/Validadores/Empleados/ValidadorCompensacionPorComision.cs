using FluentValidation;
using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Modelos.Empleados;
using SB.SistemaNomina.Dominio.Enumeraciones;

namespace SB.SistemaNomina.Aplicacion.Validadores.Empleados;

public class ValidadorCompensacionPorComision : ValidadorBase<GuardarEmpleadoSolicitud>, IValidadorCompensacionEmpleado
{
    public ValidadorCompensacionPorComision()
    {
        RuleFor(solicitud => solicitud.Compensacion.VentasBrutas)
            .NotNull().WithMessage(MensajesValidacion.CAMPO_REQUERIDO)
            .InclusiveBetween(decimal.Zero, ConstantesValidacion.MONTO_MAXIMO).WithMessage(MensajesValidacion.MONTO_FUERA_RANGO);

        RuleFor(solicitud => solicitud.Compensacion.TarifaComision)
            .NotNull().WithMessage(MensajesValidacion.CAMPO_REQUERIDO)
            .GreaterThan(decimal.Zero).WithMessage(MensajesValidacion.TARIFA_COMISION_FUERA_RANGO)
            .LessThanOrEqualTo(ConstantesValidacion.TARIFA_COMISION_MAXIMA).WithMessage(MensajesValidacion.TARIFA_COMISION_FUERA_RANGO);
    }

    public virtual TipoEmpleado Tipo => TipoEmpleado.PorComision;
}
