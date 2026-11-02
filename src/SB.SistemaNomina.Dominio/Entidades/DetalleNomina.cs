using SB.SistemaNomina.Dominio.Enumeraciones;
using SB.SistemaNomina.Dominio.ObjetosValor;

namespace SB.SistemaNomina.Dominio.Entidades;

/// <summary>Pago de un empleado en la nómina; copia sus datos al guardar.</summary>
public class DetalleNomina
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid NominaId { get; set; }

    public Guid EmpleadoId { get; set; }

    public string NombreCompletoEmpleado { get; set; } = string.Empty;

    public TipoEmpleado TipoEmpleado { get; set; }

    public string NombreDepartamento { get; set; } = string.Empty;

    public decimal PagoBruto { get; set; }

    public List<LineaDesglosePago> LineasDesglose { get; set; } = [];
}
