using SB.SistemaNomina.Aplicacion.Interfaces.Fabricas;
using SB.SistemaNomina.Aplicacion.Modelos.Empleados;
using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.Dominio.Enumeraciones;
using SB.SistemaNomina.Dominio.Excepciones;
using SB.SistemaNomina.Servicios.Comun;

namespace SB.SistemaNomina.Servicios.Empleados;

public sealed class FabricaEmpleado : IFabricaEmpleado
{
    private readonly IReadOnlyDictionary<TipoEmpleado, ICreadorEmpleado> _creadores;

    public FabricaEmpleado(IEnumerable<ICreadorEmpleado> creadores)
    {
        _creadores = creadores.ToDictionary(creador => creador.Tipo);
    }

    public Empleado Crear(GuardarEmpleadoSolicitud solicitud)
    {
        var empleado = ObtenerCreador(solicitud.Tipo).Crear(solicitud.Compensacion);
        empleado.PrimerNombre = solicitud.PrimerNombre.Trim();
        empleado.ApellidoPaterno = solicitud.ApellidoPaterno.Trim();
        empleado.NumeroSeguroSocial = solicitud.NumeroSeguroSocial.Trim();
        empleado.DepartamentoId = solicitud.DepartamentoId;
        return empleado;
    }

    public CompensacionEmpleado ObtenerCompensacion(Empleado empleado) =>
        ObtenerCreador(empleado.Tipo).ObtenerCompensacion(empleado);

    private ICreadorEmpleado ObtenerCreador(TipoEmpleado tipo) =>
        _creadores.TryGetValue(tipo, out var creador)
            ? creador
            : throw new DominioException(MensajesServicio.TIPO_EMPLEADO_NO_SOPORTADO);
}
