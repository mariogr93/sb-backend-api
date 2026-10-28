using FluentValidation;
using SB.SistemaNomina.Aplicacion.Excepciones;

namespace SB.SistemaNomina.Aplicacion.Validadores;

public static class ExtensionesValidacion
{
    /// <summary>Valida y lanza ValidacionSolicitudException con los errores agrupados por campo.</summary>
    public static async Task ValidarSolicitudAsync<T>(this IValidator<T> validador, T solicitud, CancellationToken tokenCancelacion)
    {
        var resultado = await validador.ValidateAsync(solicitud, tokenCancelacion);

        if (resultado.IsValid)
        {
            return;
        }

        var errores = resultado.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => grupo.Select(error => error.ErrorMessage).Distinct().ToArray());

        throw new ValidacionSolicitudException(errores);
    }
}
