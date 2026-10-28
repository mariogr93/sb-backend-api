namespace SB.SistemaNomina.PruebasUnitarias.Comun;

/// <summary>Reloj fijo en UTC: pruebas iguales en cualquier máquina.</summary>
public sealed class ProveedorTiempoFijo(DateTimeOffset ahora) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => ahora;

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
