using FluentValidation;
using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Dominio.Enumeraciones;

namespace SB.SistemaNomina.Aplicacion.Validadores.Empleados;

/// <summary>Reglas de comisión (heredadas) + salario base.</summary>
public sealed class ValidadorCompensacionAsalariadoPorComision : ValidadorCompensacionPorComision
{
    public ValidadorCompensacionAsalariadoPorComision()
    {
        RuleFor(solicitud => solicitud.Compensacion.SalarioBase)
            .NotNull().WithMessage(MensajesValidacion.CAMPO_REQUERIDO)
            .InclusiveBetween(decimal.Zero, ConstantesValidacion.MONTO_MAXIMO).WithMessage(MensajesValidacion.MONTO_FUERA_RANGO);
    }

    public override TipoEmpleado Tipo => TipoEmpleado.AsalariadoPorComision;
}
