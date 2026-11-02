namespace SB.SistemaNomina.Dominio.Entidades;

/// <summary>Nómina semanal guardada; congela el cálculo.</summary>
public class Nomina
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Lunes de la semana.</summary>
    public DateOnly FechaInicioSemana { get; set; }

    /// <summary>Asignada por el servicio (TimeProvider).</summary>
    public DateTime FechaCreacion { get; set; }

    public string CreadoPor { get; set; } = string.Empty;

    public decimal MontoTotal { get; set; }

    public List<DetalleNomina> Detalles { get; set; } = [];
}
