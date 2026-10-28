using SB.SistemaNomina.Aplicacion.Modelos.Empleados;
using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.Dominio.Enumeraciones;

namespace SB.SistemaNomina.Servicios.Empleados.Creadores;

public sealed class CreadorEmpleadoPorComision : CreadorEmpleadoBase<EmpleadoPorComision>
{
    public override TipoEmpleado Tipo => TipoEmpleado.PorComision;

    protected override EmpleadoPorComision CrearEmpleado(CompensacionEmpleado compensacion) =>
        new()
        {
            VentasBrutas = compensacion.VentasBrutas.GetValueOrDefault(),
            TarifaComision = compensacion.TarifaComision.GetValueOrDefault()
        };

    protected override CompensacionEmpleado ExtraerCompensacion(EmpleadoPorComision empleado) =>
        new() { VentasBrutas = empleado.VentasBrutas, TarifaComision = empleado.TarifaComision };
}
