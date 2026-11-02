using SB.SistemaNomina.Aplicacion.Modelos.Empleados;
using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.Dominio.Enumeraciones;

namespace SB.SistemaNomina.Servicios.Empleados.Creadores;

public sealed class CreadorEmpleadoPorHoras : CreadorEmpleadoBase<EmpleadoPorHoras>
{
    public override TipoEmpleado Tipo => TipoEmpleado.PorHoras;

    protected override EmpleadoPorHoras CrearEmpleado(CompensacionEmpleado compensacion) =>
        new()
        {
            SueldoPorHora = compensacion.SueldoPorHora.GetValueOrDefault(),
            HorasTrabajadas = compensacion.HorasTrabajadas.GetValueOrDefault()
        };

    protected override CompensacionEmpleado ExtraerCompensacion(EmpleadoPorHoras empleado) =>
        new() { SueldoPorHora = empleado.SueldoPorHora, HorasTrabajadas = empleado.HorasTrabajadas };
}
