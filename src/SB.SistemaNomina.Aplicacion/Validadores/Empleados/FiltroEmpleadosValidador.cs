using FluentValidation;
using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Modelos.Empleados;
using SB.SistemaNomina.Aplicacion.Validadores.Comun;

namespace SB.SistemaNomina.Aplicacion.Validadores.Empleados;

public sealed class FiltroEmpleadosValidador : ReglasPaginacionValidador<FiltroEmpleados>
{
    public FiltroEmpleadosValidador()
    {
        RuleFor(filtro => filtro.Nombre).ReglasFiltroTexto();

        RuleFor(filtro => filtro.Estado).IsInEnum().WithMessage(MensajesValidacion.VALOR_NO_VALIDO);

        RuleFor(filtro => filtro.Tipo).IsInEnum().WithMessage(MensajesValidacion.VALOR_NO_VALIDO);
    }
}
