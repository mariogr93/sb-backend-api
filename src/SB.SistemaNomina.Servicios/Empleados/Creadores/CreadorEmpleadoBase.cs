using SB.SistemaNomina.Aplicacion.Interfaces.Fabricas;
using SB.SistemaNomina.Aplicacion.Modelos.Empleados;
using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.Dominio.Enumeraciones;
using SB.SistemaNomina.Servicios.Comun;

namespace SB.SistemaNomina.Servicios.Empleados.Creadores;

/// <summary>Base tipada: cada creador solo trabaja con su subclase.</summary>
public abstract class CreadorEmpleadoBase<TEmpleado> : ICreadorEmpleado where TEmpleado : Empleado
{
    public abstract TipoEmpleado Tipo { get; }

    public Empleado Crear(CompensacionEmpleado compensacion) => CrearEmpleado(compensacion);

    public CompensacionEmpleado ObtenerCompensacion(Empleado empleado) =>
        empleado is TEmpleado empleadoTipado
            ? ExtraerCompensacion(empleadoTipado)
            : throw new ArgumentException(MensajesServicio.TIPO_EMPLEADO_NO_COINCIDE, nameof(empleado));

    protected abstract TEmpleado CrearEmpleado(CompensacionEmpleado compensacion);

    protected abstract CompensacionEmpleado ExtraerCompensacion(TEmpleado empleado);
}
