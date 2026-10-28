using SB.SistemaNomina.Aplicacion.Modelos.Empleados;
using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.Dominio.Enumeraciones;

namespace SB.SistemaNomina.Servicios.Empleados.Creadores;

public sealed class CreadorEmpleadoAsalariadoPorComision : CreadorEmpleadoBase<EmpleadoAsalariadoPorComision>
{
    public override TipoEmpleado Tipo => TipoEmpleado.AsalariadoPorComision;

    protected override EmpleadoAsalariadoPorComision CrearEmpleado(CompensacionEmpleado compensacion) =>
        new()
        {
            VentasBrutas = compensacion.VentasBrutas.GetValueOrDefault(),
            TarifaComision = compensacion.TarifaComision.GetValueOrDefault(),
            SalarioBase = compensacion.SalarioBase.GetValueOrDefault()
        };

    protected override CompensacionEmpleado ExtraerCompensacion(EmpleadoAsalariadoPorComision empleado) =>
        new()
        {
            VentasBrutas = empleado.VentasBrutas,
            TarifaComision = empleado.TarifaComision,
            SalarioBase = empleado.SalarioBase
        };
}
