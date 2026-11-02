namespace SB.SistemaNomina.Aplicacion.Modelos.Nominas;

public sealed record NominaResumenRespuesta(
    Guid Id,
    DateOnly FechaInicioSemana,
    DateTime FechaCreacion,
    string CreadoPor,
    int CantidadEmpleados,
    decimal MontoTotal);
