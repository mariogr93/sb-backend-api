using SB.SistemaNomina.Aplicacion.Comun;

namespace SB.SistemaNomina.Aplicacion.Modelos.EntidadesGubernamentales;

public sealed class FiltroEntidadesGubernamentales : SolicitudPaginada
{
    /// <summary>Contiene, sin distinguir mayúsculas.</summary>
    public string? Nombre { get; init; }

    public string? Sector { get; init; }

    public string? PoderDelEstado { get; init; }

    public bool IncluirInactivas { get; init; }
}
