using SB.SistemaNomina.Dominio.Constantes;

namespace SB.SistemaNomina.Dominio.Entidades;

public class Usuario
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string NombreUsuario { get; set; } = string.Empty;

    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>Solo hash; nunca texto plano.</summary>
    public string HashContrasena { get; set; } = string.Empty;

    public string Rol { get; set; } = NombresRoles.USUARIO;

    public bool EstaActivo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public bool EsAdministrador => Rol == NombresRoles.ADMINISTRADOR;
}
