namespace SB.SistemaNomina.Aplicacion.Modelos.Usuarios;

/// <summary>NombreUsuario no cambia; contraseña va por CambiarContrasenaSolicitud.</summary>
public sealed record ActualizarUsuarioSolicitud
{
    public string NombreCompleto { get; init; } = string.Empty;

    public string Rol { get; init; } = string.Empty;

    public bool EstaActivo { get; init; } = true;
}
