namespace SB.SistemaNomina.Dominio.ObjetosValor;

/// <summary>Concepto del pago: nombre, fórmula aplicada y monto.</summary>
public sealed record LineaDesglosePago(string Concepto, string Formula, decimal Monto);
