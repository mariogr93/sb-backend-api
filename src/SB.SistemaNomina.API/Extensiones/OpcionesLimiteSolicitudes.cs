namespace SB.SistemaNomina.API.Extensiones;

/// <summary>Sección "LimiteSolicitudes": ventana fija por usuario o IP.</summary>
public sealed class OpcionesLimiteSolicitudes
{
    public const string SECCION = "LimiteSolicitudes";

    public VentanaLimite General { get; init; } = new();

    /// <summary>Más estricta: frena intentos de fuerza bruta.</summary>
    public VentanaLimite InicioSesion { get; init; } = new();
}

public sealed class VentanaLimite
{
    public int PermisosPorVentana { get; init; }

    public int VentanaSegundos { get; init; }
}
