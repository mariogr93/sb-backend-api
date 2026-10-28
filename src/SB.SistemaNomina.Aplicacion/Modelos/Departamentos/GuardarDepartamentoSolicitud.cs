namespace SB.SistemaNomina.Aplicacion.Modelos.Departamentos;

public sealed record GuardarDepartamentoSolicitud
{
    public string Nombre { get; init; } = string.Empty;
}
