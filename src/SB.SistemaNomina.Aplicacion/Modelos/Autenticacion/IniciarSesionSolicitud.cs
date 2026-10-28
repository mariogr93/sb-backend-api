namespace SB.SistemaNomina.Aplicacion.Modelos.Autenticacion;

public sealed record IniciarSesionSolicitud
{
    public string NombreUsuario { get; init; } = string.Empty;

    public string Contrasena { get; init; } = string.Empty;
}
