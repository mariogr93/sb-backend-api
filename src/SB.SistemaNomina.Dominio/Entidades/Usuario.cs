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

    /// <summary>Asignada por el servicio (TimeProvider).</summary>
    public DateTime FechaCreacion { get; set; }

    public bool EsAdministrador => Rol == NombresRoles.ADMINISTRADOR;
}
