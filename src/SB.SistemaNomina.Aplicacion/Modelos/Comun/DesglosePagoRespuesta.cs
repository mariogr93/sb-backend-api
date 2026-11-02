namespace SB.SistemaNomina.Aplicacion.Modelos.Comun;

public sealed record DesglosePagoRespuesta(IReadOnlyList<LineaDesglosePagoRespuesta> Lineas, decimal Total);
