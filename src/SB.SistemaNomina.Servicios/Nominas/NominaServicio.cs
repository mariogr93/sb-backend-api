using FluentValidation;
using Microsoft.Extensions.Logging;
using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Excepciones;
using SB.SistemaNomina.Aplicacion.Interfaces.Repositorios;
using SB.SistemaNomina.Aplicacion.Interfaces.Seguridad;
using SB.SistemaNomina.Aplicacion.Interfaces.Servicios;
using SB.SistemaNomina.Aplicacion.Mapeos;
using SB.SistemaNomina.Aplicacion.Modelos.Nominas;
using SB.SistemaNomina.Aplicacion.Validadores;
using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.Servicios.Comun;

namespace SB.SistemaNomina.Servicios.Nominas;

public sealed class NominaServicio : INominaServicio
{
    private readonly IEmpleadoRepositorio _empleadoRepositorio;
    private readonly IDepartamentoRepositorio _departamentoRepositorio;
    private readonly INominaRepositorio _nominaRepositorio;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IValidator<GuardarNominaSolicitud> _validadorGuardar;
    private readonly IValidator<SolicitudPaginada> _validadorPaginacion;
    private readonly TimeProvider _proveedorTiempo;
    private readonly ILogger<NominaServicio> _registro;

    public NominaServicio(
        IEmpleadoRepositorio empleadoRepositorio,
        IDepartamentoRepositorio departamentoRepositorio,
        INominaRepositorio nominaRepositorio,
        IUsuarioActual usuarioActual,
        IValidator<GuardarNominaSolicitud> validadorGuardar,
        IValidator<SolicitudPaginada> validadorPaginacion,
        TimeProvider proveedorTiempo,
        ILogger<NominaServicio> registro)
    {
        _empleadoRepositorio = empleadoRepositorio;
        _departamentoRepositorio = departamentoRepositorio;
        _nominaRepositorio = nominaRepositorio;
        _usuarioActual = usuarioActual;
        _validadorGuardar = validadorGuardar;
        _validadorPaginacion = validadorPaginacion;
        _proveedorTiempo = proveedorTiempo;
        _registro = registro;
    }

    public async Task<ReporteNominaSemanalRespuesta> GenerarReporteSemanalAsync(Guid? departamentoId, CancellationToken tokenCancelacion)
    {
        if (departamentoId is { } id && await _departamentoRepositorio.ObtenerPorIdAsync(id, tokenCancelacion) is null)
        {
            throw new RecursoNoEncontradoException(NombresRecursos.DEPARTAMENTO, id);
        }

        var detalles = await CalcularDetallesAsync(departamentoId, tokenCancelacion);
        var totalesPorTipo = detalles
            .GroupBy(detalle => detalle.TipoEmpleado)
            .OrderBy(grupo => grupo.Key)
            .Select(grupo => new TotalTipoEmpleadoRespuesta(grupo.Key, grupo.Count(), grupo.Sum(detalle => detalle.PagoBruto)))
            .ToList();

        return new ReporteNominaSemanalRespuesta(
            _proveedorTiempo.GetUtcNow().UtcDateTime,
            detalles.Select(detalle => detalle.ARespuesta()).ToList(),
            totalesPorTipo,
            detalles.Count,
            detalles.Sum(detalle => detalle.PagoBruto));
    }

    public async Task<NominaRespuesta> GuardarNominaAsync(GuardarNominaSolicitud solicitud, CancellationToken tokenCancelacion)
    {
        await _validadorGuardar.ValidarSolicitudAsync(solicitud, tokenCancelacion);
        if (await _nominaRepositorio.ExisteSemanaAsync(solicitud.FechaInicioSemana, tokenCancelacion))
        {
            throw new ConflictoException(MensajesServicio.NOMINA_SEMANA_DUPLICADA);
        }

        var nomina = new Nomina
        {
            FechaInicioSemana = solicitud.FechaInicioSemana,
            FechaCreacion = _proveedorTiempo.GetUtcNow().UtcDateTime,
            CreadoPor = _usuarioActual.NombreUsuario,
            Detalles = await CalcularDetallesAsync(null, tokenCancelacion)
        };
        nomina.Detalles.ForEach(detalle => detalle.NominaId = nomina.Id);
        nomina.MontoTotal = nomina.Detalles.Sum(detalle => detalle.PagoBruto);
        await _nominaRepositorio.AgregarAsync(nomina, tokenCancelacion);

        _registro.LogInformation(
            "Nómina {NominaId} guardada para la semana {FechaInicioSemana} con {CantidadEmpleados} empleados.",
            nomina.Id,
            nomina.FechaInicioSemana,
            nomina.Detalles.Count);
        return nomina.ARespuesta();
    }

    public async Task<ResultadoPaginado<NominaResumenRespuesta>> ListarNominasAsync(SolicitudPaginada paginacion, CancellationToken tokenCancelacion)
    {
        await _validadorPaginacion.ValidarSolicitudAsync(paginacion, tokenCancelacion);
        var resultado = await _nominaRepositorio.ListarAsync(paginacion, tokenCancelacion);
        return resultado.Mapear(nomina => nomina.AResumen());
    }

    public async Task<NominaRespuesta> ObtenerNominaPorIdAsync(Guid id, CancellationToken tokenCancelacion)
    {
        var nomina = await _nominaRepositorio.ObtenerPorIdAsync(id, tokenCancelacion)
            ?? throw new RecursoNoEncontradoException(NombresRecursos.NOMINA, id);
        return nomina.ARespuesta();
    }

    /// <summary>Pago de cada empleado activo; cada subclase calcula el suyo (sin switch por tipo).</summary>
    private async Task<List<DetalleNomina>> CalcularDetallesAsync(Guid? departamentoId, CancellationToken tokenCancelacion)
    {
        var empleados = await _empleadoRepositorio.ListarActivosAsync(departamentoId, tokenCancelacion);
        var departamentos = await _departamentoRepositorio.ListarAsync(incluirInactivos: true, tokenCancelacion);
        var nombresDepartamentos = departamentos.ToDictionary(departamento => departamento.Id, departamento => departamento.Nombre);

        return empleados
            .OrderBy(empleado => empleado.NombreCompleto, StringComparer.InvariantCultureIgnoreCase)
            .Select(empleado =>
            {
                var desglose = empleado.CalcularPagoSemanal();
                return new DetalleNomina
                {
                    EmpleadoId = empleado.Id,
                    NombreCompletoEmpleado = empleado.NombreCompleto,
                    TipoEmpleado = empleado.Tipo,
                    NombreDepartamento = nombresDepartamentos.GetValueOrDefault(empleado.DepartamentoId, string.Empty),
                    PagoBruto = desglose.Total,
                    LineasDesglose = desglose.Lineas.ToList()
                };
            })
            .ToList();
    }
}
