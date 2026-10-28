namespace SB.SistemaNomina.Aplicacion.Modelos.EntidadesGubernamentales;

public sealed record EntidadGubernamentalRespuesta(
    Guid Id,
    string Nombre,
    string Categoria,
    string PoderDelEstado,
    string Sector,
    bool EstaActiva);
