using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Excepciones;
using SB.SistemaNomina.Aplicacion.Interfaces.Repositorios;
using SB.SistemaNomina.Aplicacion.Interfaces.Seguridad;
using SB.SistemaNomina.Aplicacion.Interfaces.Servicios;
using SB.SistemaNomina.Aplicacion.Modelos.Comun;
using SB.SistemaNomina.Aplicacion.Modelos.Nominas;
using SB.SistemaNomina.Dominio.Constantes;
using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.Dominio.Enumeraciones;
using SB.SistemaNomina.Dominio.ObjetosValor;
using SB.SistemaNomina.Infraestructura.Persistencia.Json;
using SB.SistemaNomina.PruebasUnitarias.Comun;
using SB.SistemaNomina.Servicios.Comun;

namespace SB.SistemaNomina.PruebasUnitarias.Servicios;

/// <summary>Servicio y repositorios JSON reales en carpeta temporal: reporte en vivo, nóminas guardadas e historial.</summary>
public sealed class NominaServicioPruebas : IDisposable
{
    private const string USUARIO_ACTUAL = "admin";

    private static readonly DateTimeOffset _ahora = new(2026, 11, 2, 15, 30, 0, TimeSpan.Zero);
    private static readonly DateOnly _semana = new(2026, 10, 26);
    private static readonly DateOnly _semanaAnterior = new(2026, 10, 19);
    private static readonly TimeSpan _tiempoMaximoReporte = TimeSpan.FromSeconds(2);

    private readonly CarpetaDatosTemporal _carpeta = new();
    private readonly JsonEmpleadoRepositorio _empleados;
    private readonly JsonDepartamentoRepositorio _departamentos;
    private readonly INominaServicio _servicio;

    private readonly Departamento _finanzas = new() { Nombre = "Finanzas" };
    private readonly Departamento _ventas = new() { Nombre = "Ventas" };
    private readonly Departamento _legal = new() { Nombre = "Legal", EstaActivo = false };

    public NominaServicioPruebas()
    {
        _empleados = _carpeta.Empleados();
        _departamentos = _carpeta.Departamentos();
        var usuarioActual = Substitute.For<IUsuarioActual>();
        usuarioActual.NombreUsuario.Returns(USUARIO_ACTUAL);

        _servicio = ContenedorPruebas
            .Crear(_ahora, servicios => servicios
                .AddSingleton<IEmpleadoRepositorio>(_empleados)
                .AddSingleton<IDepartamentoRepositorio>(_departamentos)
                .AddSingleton<INominaRepositorio>(_carpeta.Nominas())
                .AddSingleton(usuarioActual))
            .GetRequiredService<INominaServicio>();

        _departamentos.AgregarVariosAsync([_finanzas, _ventas, _legal], CancellationToken.None).GetAwaiter().GetResult();
    }

    public void Dispose() => _carpeta.Dispose();

    // Activos: 600 + 1000 + 1200 + 23750 + 1700 = 28250. Inactiva (9999) excluida.
    // Abel (comisión) va primero por nombre: el orden por tipo no coincide con el de aparición.
    private EmpleadoPorComision AbelPorComision { get; } = new() { PrimerNombre = "Abel", ApellidoPaterno = "Soto", VentasBrutas = 10000m, TarifaComision = 0.06m };

    private EmpleadoAsalariado AnaAsalariada { get; } = new() { PrimerNombre = "Ana", ApellidoPaterno = "Pérez", SalarioSemanal = 1000m };

    private EmpleadoAsalariado BrunoAsalariado { get; } = new() { PrimerNombre = "Bruno", ApellidoPaterno = "Díaz", SalarioSemanal = 1200m };

    private EmpleadoPorHoras CarlaPorHoras { get; } = new() { PrimerNombre = "Carla", ApellidoPaterno = "Mejía", SueldoPorHora = 500m, HorasTrabajadas = 45m };

    private EmpleadoAsalariadoPorComision ElenaAsalariadaPorComision { get; } = new()
    {
        PrimerNombre = "Elena",
        ApellidoPaterno = "Vargas",
        VentasBrutas = 10000m,
        TarifaComision = 0.06m,
        SalarioBase = 1000m
    };

    private EmpleadoAsalariado InesInactiva { get; } = new() { PrimerNombre = "Inés", ApellidoPaterno = "Rojas", SalarioSemanal = 9999m, Estado = EstadoEmpleado.Inactivo };

    private async Task SembrarPlantillaAsync()
    {
        AnaAsalariada.DepartamentoId = _finanzas.Id;
        BrunoAsalariado.DepartamentoId = _ventas.Id;
        CarlaPorHoras.DepartamentoId = _finanzas.Id;
        AbelPorComision.DepartamentoId = _ventas.Id;
        ElenaAsalariadaPorComision.DepartamentoId = _legal.Id;
        InesInactiva.DepartamentoId = _finanzas.Id;

        // Insertados fuera de orden: el reporte debe ordenar por nombre.
        await _empleados.AgregarVariosAsync(
            [InesInactiva, ElenaAsalariadaPorComision, CarlaPorHoras, BrunoAsalariado, AbelPorComision, AnaAsalariada],
            CancellationToken.None);
    }

    private static (Guid, string, TipoEmpleado, string, decimal) Resumen(PagoEmpleadoRespuesta pago) =>
        (pago.EmpleadoId, pago.NombreCompleto, pago.TipoEmpleado, pago.NombreDepartamento, pago.Desglose.Total);

    private static IEnumerable<(TipoEmpleado, int, decimal)> Totales(ReporteNominaSemanalRespuesta reporte) =>
        reporte.TotalesPorTipo.Select(total => (total.TipoEmpleado, total.CantidadEmpleados, total.MontoTotal));

    [Fact]
    public async Task ReporteDetallaCadaActivoYExcluyeInactivos()
    {
        await SembrarPlantillaAsync();

        var reporte = await _servicio.GenerarReporteSemanalAsync(null, CancellationToken.None);

        Assert.Equal(_ahora.UtcDateTime, reporte.FechaGeneracion);
        Assert.Equal((5, 28250m), (reporte.CantidadEmpleados, reporte.MontoTotal));
        Assert.Equal(
            [
                (AbelPorComision.Id, "Abel Soto", TipoEmpleado.PorComision, "Ventas", 600m),
                (AnaAsalariada.Id, "Ana Pérez", TipoEmpleado.Asalariado, "Finanzas", 1000m),
                (BrunoAsalariado.Id, "Bruno Díaz", TipoEmpleado.Asalariado, "Ventas", 1200m),
                (CarlaPorHoras.Id, "Carla Mejía", TipoEmpleado.PorHoras, "Finanzas", 23750m),
                (ElenaAsalariadaPorComision.Id, "Elena Vargas", TipoEmpleado.AsalariadoPorComision, "Legal", 1700m)
            ],
            reporte.Pagos.Select(Resumen));
        Assert.Equal(
            [
                new LineaDesglosePagoRespuesta(ConceptosPago.HORAS_REGULARES, "500 × 40", 20000m),
                new LineaDesglosePagoRespuesta(ConceptosPago.HORAS_EXTRA, "500 × 1.5 × 5", 3750m)
            ],
            reporte.Pagos[3].Desglose.Lineas);
        Assert.Equal(
            [
                new LineaDesglosePagoRespuesta(ConceptosPago.COMISION, "10000 × 0.06", 600m),
                new LineaDesglosePagoRespuesta(ConceptosPago.SALARIO_BASE, "1000", 1000m),
                new LineaDesglosePagoRespuesta(ConceptosPago.BONO_SALARIO_BASE, "1000 × 0.1", 100m)
            ],
            reporte.Pagos[4].Desglose.Lineas);
    }

    [Fact]
    public async Task TotalesPorTipoSumanVariosEmpleadosDelMismoTipo()
    {
        await SembrarPlantillaAsync();

        var reporte = await _servicio.GenerarReporteSemanalAsync(null, CancellationToken.None);

        Assert.Equal(
            [
                (TipoEmpleado.Asalariado, 2, 2200m),
                (TipoEmpleado.PorHoras, 1, 23750m),
                (TipoEmpleado.PorComision, 1, 600m),
                (TipoEmpleado.AsalariadoPorComision, 1, 1700m)
            ],
            Totales(reporte));
    }

    [Fact]
    public async Task ReportePorDepartamentoSoloIncluyeEseDepartamento()
    {
        await SembrarPlantillaAsync();

        var reporte = await _servicio.GenerarReporteSemanalAsync(_ventas.Id, CancellationToken.None);

        Assert.Equal([AbelPorComision.Id, BrunoAsalariado.Id], reporte.Pagos.Select(pago => pago.EmpleadoId));
        Assert.Equal((2, 1800m), (reporte.CantidadEmpleados, reporte.MontoTotal));
        Assert.Equal([(TipoEmpleado.Asalariado, 1, 1200m), (TipoEmpleado.PorComision, 1, 600m)], Totales(reporte));
    }

    [Fact]
    public async Task ReporteDeDepartamentoInexistenteLanzaNoEncontrado()
    {
        await Assert.ThrowsAsync<RecursoNoEncontradoException>(() => _servicio.GenerarReporteSemanalAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task ReporteSinActivosEstaVacio()
    {
        await _empleados.AgregarAsync(InesInactiva, CancellationToken.None);

        var reporte = await _servicio.GenerarReporteSemanalAsync(null, CancellationToken.None);

        Assert.Empty(reporte.Pagos);
        Assert.Empty(reporte.TotalesPorTipo);
        Assert.Equal((0, 0m), (reporte.CantidadEmpleados, reporte.MontoTotal));
    }

    [Fact]
    public async Task GuardarNominaCongelaElCalculoConSusDatos()
    {
        await SembrarPlantillaAsync();

        var respuesta = await _servicio.GuardarNominaAsync(new GuardarNominaSolicitud { FechaInicioSemana = _semana }, CancellationToken.None);

        // Almacén nuevo: lee lo escrito en disco, no la caché.
        var guardada = await _carpeta.Nominas().ObtenerPorIdAsync(respuesta.Id, CancellationToken.None);
        Assert.NotNull(guardada);
        Assert.Equal((_semana, _ahora.UtcDateTime, USUARIO_ACTUAL, 28250m), (guardada.FechaInicioSemana, guardada.FechaCreacion, guardada.CreadoPor, guardada.MontoTotal));
        Assert.All(guardada.Detalles, detalle => Assert.Equal(guardada.Id, detalle.NominaId));
        Assert.Equal(
            [AbelPorComision.Id, AnaAsalariada.Id, BrunoAsalariado.Id, CarlaPorHoras.Id, ElenaAsalariadaPorComision.Id],
            guardada.Detalles.Select(detalle => detalle.EmpleadoId));
        Assert.Equal(
            [
                new LineaDesglosePago(ConceptosPago.HORAS_REGULARES, "500 × 40", 20000m),
                new LineaDesglosePago(ConceptosPago.HORAS_EXTRA, "500 × 1.5 × 5", 3750m)
            ],
            guardada.Detalles[3].LineasDesglose);

        Assert.Equal((guardada.Id, _semana, _ahora.UtcDateTime, USUARIO_ACTUAL), (respuesta.Id, respuesta.FechaInicioSemana, respuesta.FechaCreacion, respuesta.CreadoPor));
        Assert.Equal((28250m, 5), (respuesta.MontoTotal, respuesta.Pagos.Count));
        Assert.Equal(guardada.Detalles.Select(detalle => (detalle.EmpleadoId, detalle.PagoBruto)), respuesta.Pagos.Select(pago => (pago.EmpleadoId, pago.Desglose.Total)));
    }

    [Fact]
    public async Task GuardarSemanaInvalidaNoGuardaNomina()
    {
        await SembrarPlantillaAsync();

        var excepcion = await Assert.ThrowsAsync<ValidacionSolicitudException>(() =>
            _servicio.GuardarNominaAsync(new GuardarNominaSolicitud { FechaInicioSemana = _semana.AddDays(1) }, CancellationToken.None));

        Assert.Equal([MensajesValidacion.FECHA_INICIO_SEMANA_NO_LUNES], excepcion.Errores[nameof(GuardarNominaSolicitud.FechaInicioSemana)]);
        Assert.Equal(0, (await _servicio.ListarNominasAsync(new SolicitudPaginada(), CancellationToken.None)).TotalElementos);
    }

    [Fact]
    public async Task HistorialConPaginaInvalidaSeRechaza()
    {
        var excepcion = await Assert.ThrowsAsync<ValidacionSolicitudException>(() =>
            _servicio.ListarNominasAsync(new SolicitudPaginada { Pagina = 0 }, CancellationToken.None));

        Assert.Equal([nameof(SolicitudPaginada.Pagina)], excepcion.Errores.Keys);
    }

    [Fact]
    public async Task NominaGuardadaNoCambiaSiCambiaElPago()
    {
        await SembrarPlantillaAsync();
        var respuesta = await _servicio.GuardarNominaAsync(new GuardarNominaSolicitud { FechaInicioSemana = _semana }, CancellationToken.None);

        AnaAsalariada.SalarioSemanal = 5000m;
        await _empleados.ActualizarAsync(AnaAsalariada, CancellationToken.None);
        var historica = await _servicio.ObtenerNominaPorIdAsync(respuesta.Id, CancellationToken.None);

        Assert.Equal((respuesta.Id, _semana, _ahora.UtcDateTime, USUARIO_ACTUAL), (historica.Id, historica.FechaInicioSemana, historica.FechaCreacion, historica.CreadoPor));
        Assert.Equal(28250m, historica.MontoTotal);
        Assert.Equal(1000m, historica.Pagos.Single(pago => pago.EmpleadoId == AnaAsalariada.Id).Desglose.Total);
    }

    [Fact]
    public async Task SemanaYaGuardadaLanzaConflictoYNoAgregaOtra()
    {
        await SembrarPlantillaAsync();
        var solicitud = new GuardarNominaSolicitud { FechaInicioSemana = _semana };
        await _servicio.GuardarNominaAsync(solicitud, CancellationToken.None);

        var excepcion = await Assert.ThrowsAsync<ConflictoException>(() => _servicio.GuardarNominaAsync(solicitud, CancellationToken.None));

        Assert.Equal(MensajesServicio.NOMINA_SEMANA_DUPLICADA, excepcion.Message);
        var historial = await _servicio.ListarNominasAsync(new SolicitudPaginada(), CancellationToken.None);
        Assert.Equal(1, historial.TotalElementos);
    }

    [Fact]
    public async Task HistorialPaginaDeLaSemanaMasReciente()
    {
        await SembrarPlantillaAsync();
        await _servicio.GuardarNominaAsync(new GuardarNominaSolicitud { FechaInicioSemana = _semanaAnterior }, CancellationToken.None);
        await _servicio.GuardarNominaAsync(new GuardarNominaSolicitud { FechaInicioSemana = _semana }, CancellationToken.None);

        var primera = await _servicio.ListarNominasAsync(new SolicitudPaginada { Pagina = 1, TamanoPagina = 1 }, CancellationToken.None);
        var segunda = await _servicio.ListarNominasAsync(new SolicitudPaginada { Pagina = 2, TamanoPagina = 1 }, CancellationToken.None);

        var reciente = Assert.Single(primera.Elementos);
        Assert.Equal((_semana, 5, 28250m, USUARIO_ACTUAL), (reciente.FechaInicioSemana, reciente.CantidadEmpleados, reciente.MontoTotal, reciente.CreadoPor));
        Assert.Equal((2, 2), (primera.TotalElementos, primera.TotalPaginas));
        Assert.Equal(_semanaAnterior, Assert.Single(segunda.Elementos).FechaInicioSemana);
    }

    [Fact]
    public async Task NominaInexistenteLanzaNoEncontrado()
    {
        await Assert.ThrowsAsync<RecursoNoEncontradoException>(() => _servicio.ObtenerNominaPorIdAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task ReporteDeMilEmpleadosTardaMenosDeDosSegundos()
    {
        var empleados = Enumerable.Range(1, 1000)
            .Select(numero => new EmpleadoAsalariado
            {
                PrimerNombre = "Empleado",
                ApellidoPaterno = $"Número{numero}",
                DepartamentoId = _finanzas.Id,
                SalarioSemanal = 1500m
            })
            .ToArray();
        await _empleados.AgregarVariosAsync(empleados, CancellationToken.None);
        await _servicio.GenerarReporteSemanalAsync(null, CancellationToken.None);

        var cronometro = Stopwatch.StartNew();
        var reporte = await _servicio.GenerarReporteSemanalAsync(null, CancellationToken.None);
        cronometro.Stop();

        Assert.Equal((1000, 1_500_000m), (reporte.CantidadEmpleados, reporte.MontoTotal));
        Assert.Equal([(TipoEmpleado.Asalariado, 1000, 1_500_000m)], Totales(reporte));
        Assert.True(cronometro.Elapsed < _tiempoMaximoReporte, $"El reporte tardó {cronometro.Elapsed.TotalMilliseconds} ms.");
    }
}
