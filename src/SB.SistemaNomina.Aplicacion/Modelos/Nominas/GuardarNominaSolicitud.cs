namespace SB.SistemaNomina.Aplicacion.Modelos.Nominas;

public sealed record GuardarNominaSolicitud
{
    /// <summary>Lunes de la semana a guardar.</summary>
    public DateOnly FechaInicioSemana { get; init; }
}
