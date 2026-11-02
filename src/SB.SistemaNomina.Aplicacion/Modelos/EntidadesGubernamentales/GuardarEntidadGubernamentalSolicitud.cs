namespace SB.SistemaNomina.Aplicacion.Modelos.EntidadesGubernamentales;

public sealed record GuardarEntidadGubernamentalSolicitud
{
    public string Nombre { get; init; } = string.Empty;

    public string Categoria { get; init; } = string.Empty;

    public string PoderDelEstado { get; init; } = string.Empty;

    public string Sector { get; init; } = string.Empty;
}
