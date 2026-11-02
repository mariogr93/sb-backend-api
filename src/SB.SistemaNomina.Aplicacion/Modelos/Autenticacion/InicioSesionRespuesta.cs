namespace SB.SistemaNomina.Aplicacion.Modelos.Autenticacion;

public sealed record InicioSesionRespuesta(
    string Token,
    DateTime FechaExpiracion,
    string NombreUsuario,
    string NombreCompleto,
    string Rol);
