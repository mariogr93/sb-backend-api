using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SB.SistemaNomina.Aplicacion.Validadores.Empleados;

namespace SB.SistemaNomina.Aplicacion;

public static class InyeccionDependencias
{
    /// <summary>Registra validadores. Requiere TimeProvider registrado por el host.</summary>
    public static IServiceCollection AddAplicacion(this IServiceCollection servicios)
    {
        var ensamblado = typeof(InyeccionDependencias).Assembly;

        // Los de compensación se registran aparte para no competir como IValidator<GuardarEmpleadoSolicitud>.
        servicios.AddValidatorsFromAssembly(ensamblado, filter: resultado => !EsValidadorCompensacion(resultado.ValidatorType));

        foreach (var tipoValidador in ensamblado.GetTypes().Where(EsValidadorCompensacion))
        {
            servicios.AddSingleton(typeof(IValidadorCompensacionEmpleado), tipoValidador);
        }

        return servicios;
    }

    private static bool EsValidadorCompensacion(Type tipo) =>
        tipo is { IsClass: true, IsAbstract: false } && typeof(IValidadorCompensacionEmpleado).IsAssignableFrom(tipo);
}
