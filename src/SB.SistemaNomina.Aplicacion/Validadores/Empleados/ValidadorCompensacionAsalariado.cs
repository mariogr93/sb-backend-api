using FluentValidation;
using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Modelos.Empleados;
using SB.SistemaNomina.Dominio.Enumeraciones;

namespace SB.SistemaNomina.Aplicacion.Validadores.Empleados;

public sealed class ValidadorCompensacionAsalariado : ValidadorBase<GuardarEmpleadoSolicitud>, IValidadorCompensacionEmpleado
{
    public ValidadorCompensacionAsalariado()
    {
        RuleFor(solicitud => solicitud.Compensacion.SalarioSemanal)
            .NotNull().WithMessage(MensajesValidacion.CAMPO_REQUERIDO)
            .InclusiveBetween(decimal.Zero, ConstantesValidacion.MONTO_MAXIMO).WithMessage(MensajesValidacion.MONTO_FUERA_RANGO);
    }

    public TipoEmpleado Tipo => TipoEmpleado.Asalariado;
}
