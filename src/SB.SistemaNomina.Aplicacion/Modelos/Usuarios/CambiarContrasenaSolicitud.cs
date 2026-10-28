namespace SB.SistemaNomina.Aplicacion.Modelos.Usuarios;

public sealed record CambiarContrasenaSolicitud
{
    public string ContrasenaNueva { get; init; } = string.Empty;
}
