namespace SB.SistemaNomina.Aplicacion.Modelos.Usuarios;

/// <summary>Nunca expone el hash de la contraseña.</summary>
public sealed record UsuarioRespuesta(
    Guid Id,
    string NombreUsuario,
    string NombreCompleto,
    string Rol,
    bool EstaActivo,
    DateTime FechaCreacion);
