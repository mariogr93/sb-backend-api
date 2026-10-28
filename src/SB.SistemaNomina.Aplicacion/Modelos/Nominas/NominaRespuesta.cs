namespace SB.SistemaNomina.Aplicacion.Modelos.Nominas;

public sealed record NominaRespuesta(
    Guid Id,
    DateOnly FechaInicioSemana,
    DateTime FechaCreacion,
    string CreadoPor,
    decimal MontoTotal,
    IReadOnlyList<PagoEmpleadoRespuesta> Pagos);
