using SB.SistemaNomina.Aplicacion.Modelos.Empleados;
using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.Dominio.Enumeraciones;

namespace SB.SistemaNomina.Aplicacion.Interfaces.Fabricas;

/// <summary>Uno por tipo de empleado. Tipo nuevo = clase nueva; se registra sola por escaneo.</summary>
public interface ICreadorEmpleado
{
    TipoEmpleado Tipo { get; }

    /// <summary>Instancia la subclase con sus datos de compensación (sin datos comunes).</summary>
    Empleado Crear(CompensacionEmpleado compensacion);

    CompensacionEmpleado ObtenerCompensacion(Empleado empleado);
}
