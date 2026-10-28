using FluentValidation;
using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Modelos.Nominas;

namespace SB.SistemaNomina.Aplicacion.Validadores.Nominas;

public sealed class GuardarNominaSolicitudValidador : ValidadorBase<GuardarNominaSolicitud>
{
    private readonly TimeProvider _proveedorTiempo;

    public GuardarNominaSolicitudValidador(TimeProvider proveedorTiempo)
    {
        _proveedorTiempo = proveedorTiempo;

        RuleFor(solicitud => solicitud.FechaInicioSemana)
            .NotEmpty().WithMessage(MensajesValidacion.CAMPO_REQUERIDO)
            .Must(fecha => fecha.DayOfWeek == DayOfWeek.Monday).WithMessage(MensajesValidacion.FECHA_INICIO_SEMANA_NO_LUNES)
            .Must(NoEsFutura).WithMessage(MensajesValidacion.FECHA_INICIO_SEMANA_FUTURA);
    }

    private bool NoEsFutura(DateOnly fecha) => fecha <= DateOnly.FromDateTime(_proveedorTiempo.GetLocalNow().DateTime);
}
