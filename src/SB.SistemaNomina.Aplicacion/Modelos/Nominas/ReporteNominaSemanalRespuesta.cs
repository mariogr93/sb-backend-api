namespace SB.SistemaNomina.Aplicacion.Modelos.Nominas;

/// <summary>Cálculo en vivo con los datos actuales; solo empleados activos.</summary>
public sealed record ReporteNominaSemanalRespuesta(
    DateTime FechaGeneracion,
    IReadOnlyList<PagoEmpleadoRespuesta> Pagos,
    IReadOnlyList<TotalTipoEmpleadoRespuesta> TotalesPorTipo,
    int CantidadEmpleados,
    decimal MontoTotal);
