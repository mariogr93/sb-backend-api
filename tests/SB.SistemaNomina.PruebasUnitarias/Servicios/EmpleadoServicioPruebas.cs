using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Excepciones;
using SB.SistemaNomina.Aplicacion.Interfaces.Repositorios;
using SB.SistemaNomina.Aplicacion.Interfaces.Seguridad;
using SB.SistemaNomina.Aplicacion.Interfaces.Servicios;
using SB.SistemaNomina.Aplicacion.Modelos.Comun;
using SB.SistemaNomina.Aplicacion.Modelos.Empleados;
using SB.SistemaNomina.Dominio.Constantes;
using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.Dominio.Enumeraciones;
using SB.SistemaNomina.Dominio.Excepciones;
using SB.SistemaNomina.PruebasUnitarias.Comun;
using SB.SistemaNomina.Servicios.Comun;
using SB.SistemaNomina.Servicios.Empleados;
using SB.SistemaNomina.Servicios.Empleados.Creadores;

namespace SB.SistemaNomina.PruebasUnitarias.Servicios;

/// <summary>Servicio real (validadores, fábrica y creadores reales); repositorios y usuario son dobles.</summary>
public sealed class EmpleadoServicioPruebas
{
    private const string CEDULA = "402-1234567-8";

    private static readonly DateTimeOffset _ahora = new(2026, 11, 2, 15, 30, 0, TimeSpan.Zero);
    private static readonly DateTime _fechaAnterior = new(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);

    // Todos los campos llenos: cada tipo debe tomar solo los suyos.
    private static readonly CompensacionEmpleado _compensacionCompleta = new()
    {
        SalarioSemanal = 1500m,
        SueldoPorHora = 500m,
        HorasTrabajadas = 45m,
        VentasBrutas = 10000m,
        TarifaComision = 0.06m,
        SalarioBase = 1000m
    };

    private readonly Departamento _departamento = new() { Nombre = "Finanzas" };
    private readonly Departamento _departamentoInactivo = new() { Nombre = "Legal", EstaActivo = false };
    private readonly IEmpleadoRepositorio _empleadoRepositorio = Substitute.For<IEmpleadoRepositorio>();
    private readonly IDepartamentoRepositorio _departamentoRepositorio = Substitute.For<IDepartamentoRepositorio>();
    private readonly IUsuarioActual _usuarioActual = Substitute.For<IUsuarioActual>();
    private readonly IEmpleadoServicio _servicio;
    private Empleado? _agregado;
    private Empleado? _actualizado;

    public EmpleadoServicioPruebas()
    {
        _departamentoRepositorio.ObtenerPorIdAsync(_departamento.Id, Arg.Any<CancellationToken>()).Returns(_departamento);
        _departamentoRepositorio.ObtenerPorIdAsync(_departamentoInactivo.Id, Arg.Any<CancellationToken>()).Returns(_departamentoInactivo);
        _empleadoRepositorio
            .When(repositorio => repositorio.AgregarAsync(Arg.Any<Empleado>(), Arg.Any<CancellationToken>()))
            .Do(llamada => _agregado = llamada.Arg<Empleado>());
        _empleadoRepositorio
            .When(repositorio => repositorio.ActualizarAsync(Arg.Any<Empleado>(), Arg.Any<CancellationToken>()))
            .Do(llamada => _actualizado = llamada.Arg<Empleado>());

        _servicio = ContenedorPruebas
            .Crear(_ahora, servicios => servicios
                .AddSingleton(_empleadoRepositorio)
                .AddSingleton(_departamentoRepositorio)
                .AddSingleton(_usuarioActual))
            .GetRequiredService<IEmpleadoServicio>();
    }

    private GuardarEmpleadoSolicitud Solicitud(
        TipoEmpleado tipo = TipoEmpleado.Asalariado,
        CompensacionEmpleado? compensacion = null) => new()
    {
        PrimerNombre = "Ana",
        ApellidoPaterno = "Pérez",
        Cedula = CEDULA,
        DepartamentoId = _departamento.Id,
        Tipo = tipo,
        Compensacion = compensacion ?? new CompensacionEmpleado { SalarioSemanal = 1500m }
    };

    private EmpleadoAsalariado Existente(EstadoEmpleado estado = EstadoEmpleado.Activo)
    {
        var existente = new EmpleadoAsalariado
        {
            PrimerNombre = "Ana",
            ApellidoPaterno = "Pérez",
            Cedula = CEDULA,
            DepartamentoId = _departamento.Id,
            SalarioSemanal = 1500m,
            Estado = estado,
            FechaCreacion = _fechaAnterior,
            FechaActualizacion = _fechaAnterior
        };
        _empleadoRepositorio.ObtenerPorIdAsync(existente.Id, Arg.Any<CancellationToken>()).Returns(existente);
        return existente;
    }

    private async Task NoHuboEscriturasAsync()
    {
        await _empleadoRepositorio.DidNotReceiveWithAnyArgs().AgregarAsync(default!, default);
        await _empleadoRepositorio.DidNotReceiveWithAnyArgs().ActualizarAsync(default!, default);
    }

    private Task EjecutarAsync(bool esActualizacion, GuardarEmpleadoSolicitud solicitud) =>
        esActualizacion
            ? _servicio.ActualizarAsync(Existente().Id, solicitud, CancellationToken.None)
            : _servicio.CrearAsync(solicitud, CancellationToken.None);

    private static CompensacionEmpleado CompensacionPropia(TipoEmpleado tipo) => tipo switch
    {
        TipoEmpleado.Asalariado => new() { SalarioSemanal = 1500m },
        TipoEmpleado.PorHoras => new() { SueldoPorHora = 500m, HorasTrabajadas = 45m },
        TipoEmpleado.PorComision => new() { VentasBrutas = 10000m, TarifaComision = 0.06m },
        _ => new() { VentasBrutas = 10000m, TarifaComision = 0.06m, SalarioBase = 1000m }
    };

    [Fact]
    public async Task CrearGuardaTodosLosDatosYDevuelveLoGuardado()
    {
        var respuesta = await _servicio.CrearAsync(Solicitud(), CancellationToken.None);

        await _empleadoRepositorio.Received(1).AgregarAsync(Arg.Any<Empleado>(), Arg.Any<CancellationToken>());
        var guardado = Assert.IsType<EmpleadoAsalariado>(_agregado);
        Assert.Equal(("Ana", "Pérez", CEDULA, _departamento.Id, 1500m), (guardado.PrimerNombre, guardado.ApellidoPaterno, guardado.Cedula, guardado.DepartamentoId, guardado.SalarioSemanal));
        Assert.Equal(EstadoEmpleado.Activo, guardado.Estado);
        Assert.Equal(_ahora.UtcDateTime, guardado.FechaCreacion);
        Assert.Null(guardado.FechaActualizacion);

        Assert.Equal(guardado.Id, respuesta.Id);
        Assert.Equal(("Ana", "Pérez", "Ana Pérez"), (respuesta.PrimerNombre, respuesta.ApellidoPaterno, respuesta.NombreCompleto));
        Assert.Equal((_departamento.Id, "Finanzas"), (respuesta.DepartamentoId, respuesta.NombreDepartamento));
        Assert.Equal((TipoEmpleado.Asalariado, EstadoEmpleado.Activo), (respuesta.Tipo, respuesta.Estado));
        Assert.Equal(new CompensacionEmpleado { SalarioSemanal = 1500m }, respuesta.Compensacion);
        Assert.Equal(1500m, respuesta.PagoSemanal.Total);
        Assert.Equal([new LineaDesglosePagoRespuesta(ConceptosPago.SALARIO_SEMANAL, "1500", 1500m)], respuesta.PagoSemanal.Lineas);
        Assert.Equal(_ahora.UtcDateTime, respuesta.FechaCreacion);
        Assert.Null(respuesta.FechaActualizacion);
    }

    [Theory]
    [InlineData(TipoEmpleado.Asalariado, typeof(EmpleadoAsalariado), 1500)]
    [InlineData(TipoEmpleado.PorHoras, typeof(EmpleadoPorHoras), 23750)]
    [InlineData(TipoEmpleado.PorComision, typeof(EmpleadoPorComision), 600)]
    [InlineData(TipoEmpleado.AsalariadoPorComision, typeof(EmpleadoAsalariadoPorComision), 1700)]
    public async Task CrearUsaElCreadorDelTipoEIgnoraCamposAjenos(TipoEmpleado tipo, Type subtipo, decimal pagoEsperado)
    {
        var respuesta = await _servicio.CrearAsync(Solicitud(tipo, _compensacionCompleta), CancellationToken.None);

        Assert.IsType(subtipo, _agregado);
        Assert.Equal(tipo, respuesta.Tipo);
        Assert.Equal(CompensacionPropia(tipo), respuesta.Compensacion);
        Assert.Equal(pagoEsperado, respuesta.PagoSemanal.Total);
    }

    [Fact]
    public async Task CedulaDuplicadaLanzaConflictoYNoGuarda()
    {
        _empleadoRepositorio.ExisteCedulaAsync(CEDULA, null, Arg.Any<CancellationToken>()).Returns(true);

        var excepcion = await Assert.ThrowsAsync<ConflictoException>(() => _servicio.CrearAsync(Solicitud(), CancellationToken.None));

        Assert.Equal(MensajesServicio.CEDULA_DUPLICADA, excepcion.Message);
        await NoHuboEscriturasAsync();
    }

    [Fact]
    public async Task CedulaDeOtroEmpleadoAlActualizarLanzaConflictoYNoGuarda()
    {
        var existente = Existente();
        _empleadoRepositorio.ExisteCedulaAsync(CEDULA, existente.Id, Arg.Any<CancellationToken>()).Returns(true);

        await Assert.ThrowsAsync<ConflictoException>(() => _servicio.ActualizarAsync(existente.Id, Solicitud(), CancellationToken.None));

        await NoHuboEscriturasAsync();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SolicitudInvalidaSeRechazaSinGuardar(bool esActualizacion)
    {
        var solicitud = Solicitud() with { Cedula = "402-12-3456" };

        var excepcion = await Assert.ThrowsAsync<ValidacionSolicitudException>(() => EjecutarAsync(esActualizacion, solicitud));

        Assert.Equal([MensajesValidacion.CEDULA_INVALIDA], excepcion.Errores[nameof(GuardarEmpleadoSolicitud.Cedula)]);
        await NoHuboEscriturasAsync();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task DepartamentoInexistenteOInactivoSeRechazaSinGuardar(bool esActualizacion, bool departamentoExiste)
    {
        var solicitud = Solicitud() with { DepartamentoId = departamentoExiste ? _departamentoInactivo.Id : Guid.NewGuid() };

        var excepcion = await Assert.ThrowsAsync<ValidacionSolicitudException>(() => EjecutarAsync(esActualizacion, solicitud));

        Assert.Equal([MensajesServicio.DEPARTAMENTO_NO_DISPONIBLE], excepcion.Errores[nameof(GuardarEmpleadoSolicitud.DepartamentoId)]);
        await NoHuboEscriturasAsync();
    }

    [Fact]
    public async Task ActualizarMismoTipoRecalculaYFijaFechaActualizacion()
    {
        var existente = Existente();
        var solicitud = Solicitud(compensacion: new CompensacionEmpleado { SalarioSemanal = 2000m });

        var respuesta = await _servicio.ActualizarAsync(existente.Id, solicitud, CancellationToken.None);

        await _empleadoRepositorio.Received(1).ActualizarAsync(Arg.Any<Empleado>(), Arg.Any<CancellationToken>());
        var guardado = Assert.IsType<EmpleadoAsalariado>(_actualizado);
        Assert.Equal(2000m, guardado.SalarioSemanal);
        Assert.Equal(_ahora.UtcDateTime, guardado.FechaActualizacion);
        Assert.Equal(2000m, respuesta.PagoSemanal.Total);
    }

    [Fact]
    public async Task ActualizarCambiaTipoYConservaIdEstadoYFechaCreacion()
    {
        var existente = Existente(EstadoEmpleado.Inactivo);
        var solicitud = Solicitud(TipoEmpleado.PorHoras, new CompensacionEmpleado { SueldoPorHora = 500m, HorasTrabajadas = 45m })
            with { PrimerNombre = "Ana María" };

        var respuesta = await _servicio.ActualizarAsync(existente.Id, solicitud, CancellationToken.None);

        await _empleadoRepositorio.Received(1).ActualizarAsync(Arg.Any<Empleado>(), Arg.Any<CancellationToken>());
        var guardado = Assert.IsType<EmpleadoPorHoras>(_actualizado);
        Assert.Equal(existente.Id, guardado.Id);
        Assert.Equal(EstadoEmpleado.Inactivo, guardado.Estado);
        Assert.Equal(_fechaAnterior, guardado.FechaCreacion);
        Assert.Equal(_ahora.UtcDateTime, guardado.FechaActualizacion);
        Assert.Equal((500m, 45m, "Ana María"), (guardado.SueldoPorHora, guardado.HorasTrabajadas, guardado.PrimerNombre));

        Assert.Equal((existente.Id, TipoEmpleado.PorHoras, EstadoEmpleado.Inactivo), (respuesta.Id, respuesta.Tipo, respuesta.Estado));
        Assert.Equal(23750m, respuesta.PagoSemanal.Total);
        Assert.Equal(_fechaAnterior, respuesta.FechaCreacion);
    }

    [Fact]
    public async Task DesactivarGuardaEmpleadoInactivoConFechaDelReloj()
    {
        var existente = Existente();

        await _servicio.DesactivarAsync(existente.Id, CancellationToken.None);

        await _empleadoRepositorio.Received(1).ActualizarAsync(Arg.Any<Empleado>(), Arg.Any<CancellationToken>());
        Assert.NotNull(_actualizado);
        Assert.Equal(existente.Id, _actualizado.Id);
        Assert.Equal(EstadoEmpleado.Inactivo, _actualizado.Estado);
        Assert.Equal(_ahora.UtcDateTime, _actualizado.FechaActualizacion);
    }

    [Fact]
    public async Task DesactivarEmpleadoInexistenteLanzaNoEncontradoYNoGuarda()
    {
        await Assert.ThrowsAsync<RecursoNoEncontradoException>(() => _servicio.DesactivarAsync(Guid.NewGuid(), CancellationToken.None));

        await NoHuboEscriturasAsync();
    }

    [Theory]
    [InlineData(true, CEDULA)]
    [InlineData(false, "***-****567-8")]
    public async Task CedulaCompletaSoloParaAdministrador(bool esAdministrador, string esperada)
    {
        var existente = Existente();
        _usuarioActual.EsAdministrador.Returns(esAdministrador);

        var respuesta = await _servicio.ObtenerPorIdAsync(existente.Id, CancellationToken.None);

        Assert.Equal(esperada, respuesta.Cedula);
    }

    [Fact]
    public void FabricaRechazaTipoSinCreador()
    {
        var fabrica = new FabricaEmpleado([new CreadorEmpleadoAsalariado()]);

        var excepcion = Assert.Throws<DominioException>(() => fabrica.Crear(Solicitud(TipoEmpleado.PorHoras)));

        Assert.Equal(MensajesServicio.TIPO_EMPLEADO_NO_SOPORTADO, excepcion.Message);
    }

    [Fact]
    public void CreadorRechazaEmpleadoDeOtroTipo()
    {
        var excepcion = Assert.Throws<ArgumentException>(() => new CreadorEmpleadoAsalariado().ObtenerCompensacion(new EmpleadoPorHoras()));

        Assert.Equal("empleado", excepcion.ParamName);
        Assert.StartsWith(MensajesServicio.TIPO_EMPLEADO_NO_COINCIDE, excepcion.Message);
    }
}
