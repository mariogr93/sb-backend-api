using SB.SistemaNomina.Aplicacion.Modelos.Comun;
using SB.SistemaNomina.Aplicacion.Modelos.Departamentos;
using SB.SistemaNomina.Aplicacion.Modelos.EntidadesGubernamentales;
using SB.SistemaNomina.Aplicacion.Modelos.Nominas;
using SB.SistemaNomina.Aplicacion.Modelos.Usuarios;
using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.Dominio.ObjetosValor;

namespace SB.SistemaNomina.Aplicacion.Mapeos;

/// <summary>Entidad → respuesta. Empleado se mapea en su servicio (requiere la fábrica para la compensación).</summary>
public static class ExtensionesMapeo
{
    public static DesglosePagoRespuesta ARespuesta(this DesglosePago desglose) =>
        new(desglose.Lineas.ARespuesta(), desglose.Total);

    public static IReadOnlyList<LineaDesglosePagoRespuesta> ARespuesta(this IEnumerable<LineaDesglosePago> lineas) =>
        lineas.Select(linea => new LineaDesglosePagoRespuesta(linea.Concepto, linea.Formula, linea.Monto)).ToList();

    public static DepartamentoRespuesta ARespuesta(this Departamento departamento) =>
        new(departamento.Id, departamento.Nombre, departamento.EstaActivo);

    public static EntidadGubernamentalRespuesta ARespuesta(this EntidadGubernamental entidad) =>
        new(entidad.Id, entidad.Nombre, entidad.Categoria, entidad.PoderDelEstado, entidad.Sector, entidad.EstaActiva);

    public static UsuarioRespuesta ARespuesta(this Usuario usuario) =>
        new(usuario.Id, usuario.NombreUsuario, usuario.NombreCompleto, usuario.Rol, usuario.EstaActivo, usuario.FechaCreacion);

    public static PagoEmpleadoRespuesta ARespuesta(this DetalleNomina detalle) =>
        new(
            detalle.EmpleadoId,
            detalle.NombreCompletoEmpleado,
            detalle.TipoEmpleado,
            detalle.NombreDepartamento,
            new DesglosePagoRespuesta(detalle.LineasDesglose.ARespuesta(), detalle.PagoBruto));

    public static NominaRespuesta ARespuesta(this Nomina nomina) =>
        new(
            nomina.Id,
            nomina.FechaInicioSemana,
            nomina.FechaCreacion,
            nomina.CreadoPor,
            nomina.MontoTotal,
            nomina.Detalles.Select(detalle => detalle.ARespuesta()).ToList());

    public static NominaResumenRespuesta AResumen(this Nomina nomina) =>
        new(nomina.Id, nomina.FechaInicioSemana, nomina.FechaCreacion, nomina.CreadoPor, nomina.Detalles.Count, nomina.MontoTotal);
}
