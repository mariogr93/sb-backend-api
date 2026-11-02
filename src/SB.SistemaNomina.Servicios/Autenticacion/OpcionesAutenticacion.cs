namespace SB.SistemaNomina.Servicios.Autenticacion;

/// <summary>Sección "Autenticacion" de appsettings; ClaveFirma en producción por variable de entorno.</summary>
public sealed class OpcionesAutenticacion
{
    public const string SECCION = "Autenticacion";

    /// <summary>HMAC-SHA256 requiere al menos 32 bytes.</summary>
    public const int LONGITUD_MINIMA_CLAVE_BYTES = 32;

    public string Emisor { get; init; } = string.Empty;

    public string Audiencia { get; init; } = string.Empty;

    public string ClaveFirma { get; init; } = string.Empty;

    public int DuracionTokenMinutos { get; init; }
}
