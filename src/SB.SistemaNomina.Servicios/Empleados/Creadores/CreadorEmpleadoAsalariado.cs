using SB.SistemaNomina.Aplicacion.Modelos.Empleados;
using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.Dominio.Enumeraciones;

namespace SB.SistemaNomina.Servicios.Empleados.Creadores;

public sealed class CreadorEmpleadoAsalariado : CreadorEmpleadoBase<EmpleadoAsalariado>
{
    public override TipoEmpleado Tipo => TipoEmpleado.Asalariado;

    protected override EmpleadoAsalariado CrearEmpleado(CompensacionEmpleado compensacion) =>
        new() { SalarioSemanal = compensacion.SalarioSemanal.GetValueOrDefault() };

    protected override CompensacionEmpleado ExtraerCompensacion(EmpleadoAsalariado empleado) =>
        new() { SalarioSemanal = empleado.SalarioSemanal };
}
