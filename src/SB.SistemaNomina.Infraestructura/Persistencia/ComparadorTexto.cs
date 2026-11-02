using System.Globalization;

namespace SB.SistemaNomina.Infraestructura.Persistencia;

/// <summary>Comparación sin mayúsculas ni acentos: "perez" encuentra "Pérez".</summary>
public static class ComparadorTexto
{
    private const CompareOptions OPCIONES_COMPARACION = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

    private static readonly CompareInfo _comparador = CultureInfo.InvariantCulture.CompareInfo;

    /// <summary>Búsqueda vacía coincide con todo.</summary>
    public static bool Contiene(string texto, string? busqueda) =>
        string.IsNullOrWhiteSpace(busqueda) || _comparador.IndexOf(texto, busqueda.Trim(), OPCIONES_COMPARACION) >= 0;

    public static bool SonIguales(string texto, string otro) =>
        _comparador.Compare(texto.Trim(), otro.Trim(), OPCIONES_COMPARACION) == 0;
}
