namespace SB.SistemaNomina.Aplicacion.Modelos.Usuarios;

public sealed record CrearUsuarioSolicitud
{
    public string NombreUsuario { get; init; } = string.Empty;

    public string NombreCompleto { get; init; } = string.Empty;

    public string Contrasena { get; init; } = string.Empty;

    public string Rol { get; init; } = string.Empty;
}
