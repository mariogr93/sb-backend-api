using System.Text.RegularExpressions;
using SB.SistemaNomina.Aplicacion.Comun;

namespace SB.SistemaNomina.Servicios.Empleados;

/// <summary>Oculta la cédula para usuarios sin rol Admin: 123-1234567-1 → ***-****567-1.</summary>
public static class EnmascaradorCedula
{
    // "567-1": últimos 3 dígitos del bloque central y el verificador.
    private const int CARACTERES_VISIBLES = 5;
    private const char CARACTER_OCULTO = '*';

    public static string Enmascarar(string cedula)
    {
        // Formato inesperado: se ocultan todos los dígitos.
        var visiblesDesde = Regex.IsMatch(cedula, ConstantesValidacion.PATRON_CEDULA)
            ? cedula.Length - CARACTERES_VISIBLES
            : cedula.Length;

        return string.Concat(cedula.Select((caracter, posicion) =>
            posicion < visiblesDesde && char.IsDigit(caracter) ? CARACTER_OCULTO : caracter));
    }
}
