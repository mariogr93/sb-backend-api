using SB.SistemaNomina.Aplicacion.Modelos.Empleados;
using SB.SistemaNomina.Dominio.Entidades;

namespace SB.SistemaNomina.Aplicacion.Interfaces.Fabricas;

/// <summary>Elige el ICreadorEmpleado según el tipo.</summary>
public interface IFabricaEmpleado
{
    /// <summary>Empleado nuevo con datos comunes y de compensación (sin Id ni fechas).</summary>
    Empleado Crear(GuardarEmpleadoSolicitud solicitud);

    CompensacionEmpleado ObtenerCompensacion(Empleado empleado);
}
