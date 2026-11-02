namespace SB.SistemaNomina.Dominio.Entidades;

/// <summary>Fuente: ListaEntidadesGubernamentales.xlsx.</summary>
public class EntidadGubernamental
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Nombre { get; set; } = string.Empty;

    public string Categoria { get; set; } = string.Empty;

    public string PoderDelEstado { get; set; } = string.Empty;

    public string Sector { get; set; } = string.Empty;

    public bool EstaActiva { get; set; } = true;
}
