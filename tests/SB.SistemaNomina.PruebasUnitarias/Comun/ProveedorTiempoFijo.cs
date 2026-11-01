namespace SB.SistemaNomina.PruebasUnitarias.Comun;

/// <summary>Reloj fijo; zona local UTC salvo que se indique otra.</summary>
public sealed class ProveedorTiempoFijo(DateTimeOffset ahora, TimeZoneInfo? zonaLocal = null) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => ahora;

    public override TimeZoneInfo LocalTimeZone => zonaLocal ?? TimeZoneInfo.Utc;
}
