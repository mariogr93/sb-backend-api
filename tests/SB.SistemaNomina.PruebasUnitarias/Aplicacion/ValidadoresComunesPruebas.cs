using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SB.SistemaNomina.Aplicacion;
using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Excepciones;
using SB.SistemaNomina.Aplicacion.Modelos.Autenticacion;
using SB.SistemaNomina.Aplicacion.Modelos.Departamentos;
using SB.SistemaNomina.Aplicacion.Modelos.Empleados;
using SB.SistemaNomina.Aplicacion.Modelos.EntidadesGubernamentales;
using SB.SistemaNomina.Aplicacion.Modelos.Nominas;
using SB.SistemaNomina.Aplicacion.Modelos.Usuarios;
using SB.SistemaNomina.Aplicacion.Validadores;
using SB.SistemaNomina.Aplicacion.Validadores.Autenticacion;
using SB.SistemaNomina.Aplicacion.Validadores.Comun;
using SB.SistemaNomina.Aplicacion.Validadores.Departamentos;
using SB.SistemaNomina.Aplicacion.Validadores.Empleados;
using SB.SistemaNomina.Aplicacion.Validadores.EntidadesGubernamentales;
using SB.SistemaNomina.Aplicacion.Validadores.Nominas;
using SB.SistemaNomina.Aplicacion.Validadores.Usuarios;
using SB.SistemaNomina.Dominio.Enumeraciones;
using SB.SistemaNomina.PruebasUnitarias.Comun;

namespace SB.SistemaNomina.PruebasUnitarias.Aplicacion;

public class ValidadoresComunesPruebas
{
    private static readonly DateOnly _lunes = new(2026, 10, 26);

    [Theory]
    [InlineData(0, 20, nameof(SolicitudPaginada.Pagina))]
    [InlineData(1_000_001, 20, nameof(SolicitudPaginada.Pagina))]
    [InlineData(1, 0, nameof(SolicitudPaginada.TamanoPagina))]
    [InlineData(1, 101, nameof(SolicitudPaginada.TamanoPagina))]
    public async Task PaginacionFueraDeRangoEsInvalida(int pagina, int tamanoPagina, string campo)
    {
        var resultado = await new SolicitudPaginadaValidador()
            .ValidateAsync(new SolicitudPaginada { Pagina = pagina, TamanoPagina = tamanoPagina });

        Assert.Equal(campo, Assert.Single(resultado.Errors).PropertyName);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(1_000_000, 100)]
    public async Task PaginacionEnLosLimitesEsValida(int pagina, int tamanoPagina)
    {
        var resultado = await new SolicitudPaginadaValidador()
            .ValidateAsync(new SolicitudPaginada { Pagina = pagina, TamanoPagina = tamanoPagina });

        Assert.True(resultado.IsValid);
    }

    [Theory]
    [InlineData(1, 20, 0)]
    [InlineData(3, 20, 40)]
    [InlineData(1_000_000, 100, 99_999_900)]
    public void CantidadOmitirSeCalculaPorPagina(int pagina, int tamanoPagina, int esperado)
    {
        var solicitud = new SolicitudPaginada { Pagina = pagina, TamanoPagina = tamanoPagina };

        Assert.Equal(esperado, solicitud.CantidadOmitir);
    }

    [Fact]
    public void PaginacionPorDefectoEsPrimeraPaginaDeVeinte()
    {
        var solicitud = new SolicitudPaginada();

        Assert.Equal(1, solicitud.Pagina);
        Assert.Equal(20, solicitud.TamanoPagina);
    }

    [Theory]
    [InlineData(2026, 10, 25, false)]
    [InlineData(2026, 10, 26, true)]
    public async Task LunesActualEsValidoSoloDesdeEseDia(int anio, int mes, int dia, bool esValido)
    {
        var reloj = new ProveedorTiempoFijo(new DateTimeOffset(anio, mes, dia, 0, 0, 0, TimeSpan.Zero));

        var resultado = await new GuardarNominaSolicitudValidador(reloj)
            .ValidateAsync(new GuardarNominaSolicitud { FechaInicioSemana = _lunes });

        Assert.Equal(esValido, resultado.IsValid);
        if (!esValido)
        {
            Assert.Equal(MensajesValidacion.FECHA_INICIO_SEMANA_FUTURA, Assert.Single(resultado.Errors).ErrorMessage);
        }
    }

    [Theory]
    [InlineData(2026, 10, 26)]
    [InlineData(2026, 10, 19)]
    [InlineData(2025, 1, 6)]
    public async Task LunesPasadoEsValido(int anio, int mes, int dia)
    {
        var reloj = new ProveedorTiempoFijo(new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero));

        var resultado = await new GuardarNominaSolicitudValidador(reloj)
            .ValidateAsync(new GuardarNominaSolicitud { FechaInicioSemana = new DateOnly(anio, mes, dia) });

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public async Task FechaNoLunesSeRechazaConSuMensaje()
    {
        var reloj = new ProveedorTiempoFijo(new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero));

        var resultado = await new GuardarNominaSolicitudValidador(reloj)
            .ValidateAsync(new GuardarNominaSolicitud { FechaInicioSemana = _lunes.AddDays(1) });

        Assert.Equal(MensajesValidacion.FECHA_INICIO_SEMANA_NO_LUNES, Assert.Single(resultado.Errors).ErrorMessage);
    }

    [Fact]
    public async Task FechaOmitidaEsRequerida()
    {
        var reloj = new ProveedorTiempoFijo(new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero));

        var resultado = await new GuardarNominaSolicitudValidador(reloj).ValidateAsync(new GuardarNominaSolicitud());

        var error = Assert.Single(resultado.Errors);
        Assert.Equal(nameof(GuardarNominaSolicitud.FechaInicioSemana), error.PropertyName);
        Assert.Equal("NotEmptyValidator", error.ErrorCode);
    }

    [Fact]
    public async Task ValidarSolicitudValidaNoLanza()
    {
        var excepcion = await Record.ExceptionAsync(() =>
            new SolicitudPaginadaValidador().ValidarSolicitudAsync(new SolicitudPaginada(), CancellationToken.None));

        Assert.Null(excepcion);
    }

    [Fact]
    public async Task ValidarSolicitudInvalidaLanzaErroresAgrupadosPorCampo()
    {
        var solicitud = new SolicitudPaginada { Pagina = 0, TamanoPagina = 0 };

        var excepcion = await Assert.ThrowsAsync<ValidacionSolicitudException>(() =>
            new SolicitudPaginadaValidador().ValidarSolicitudAsync(solicitud, CancellationToken.None));

        Assert.Equal(MensajesValidacion.SOLICITUD_INVALIDA, excepcion.Message);
        Assert.Equal(2, excepcion.Errores.Count);
        Assert.Equal([MensajesValidacion.PAGINA_INVALIDA], excepcion.Errores[nameof(SolicitudPaginada.Pagina)]);
        Assert.Equal([MensajesValidacion.TAMANO_PAGINA_INVALIDO], excepcion.Errores[nameof(SolicitudPaginada.TamanoPagina)]);
    }

    [Fact]
    public async Task ValidarSolicitudAgrupaYEliminaMensajesDuplicados()
    {
        var validador = new InlineValidator<SolicitudPaginada>();
        validador.RuleFor(solicitud => solicitud).Custom((_, contexto) =>
        {
            contexto.AddFailure("Pagina", "Pagina invalida.");
            contexto.AddFailure("Pagina", "Pagina invalida.");
            contexto.AddFailure("Pagina", "Pagina fuera de rango.");
            contexto.AddFailure("TamanoPagina", "Tamano invalido.");
        });

        var excepcion = await Assert.ThrowsAsync<ValidacionSolicitudException>(() =>
            validador.ValidarSolicitudAsync(new SolicitudPaginada(), CancellationToken.None));

        Assert.Equal(2, excepcion.Errores.Count);
        Assert.Equal(["Pagina invalida.", "Pagina fuera de rango."], excepcion.Errores["Pagina"]);
        Assert.Equal(["Tamano invalido."], excepcion.Errores["TamanoPagina"]);
    }

    [Fact]
    public async Task ValidarSolicitudPropagaTokenRecibido()
    {
        using var fuente = new CancellationTokenSource();
        var tokenRecibido = CancellationToken.None;
        var validador = new InlineValidator<SolicitudPaginada>();
        validador.RuleFor(solicitud => solicitud).CustomAsync((_, _, token) =>
        {
            tokenRecibido = token;
            return Task.CompletedTask;
        });

        await validador.ValidarSolicitudAsync(new SolicitudPaginada(), fuente.Token);

        Assert.Equal(fuente.Token, tokenRecibido);
    }

    [Fact]
    public async Task ValidarSolicitudConTokenCanceladoLanzaCancelacion()
    {
        using var fuente = new CancellationTokenSource();
        await fuente.CancelAsync();
        var validador = new InlineValidator<SolicitudPaginada>();
        validador.RuleFor(solicitud => solicitud).CustomAsync((_, _, token) =>
        {
            token.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            validador.ValidarSolicitudAsync(new SolicitudPaginada(), fuente.Token));
    }

    private static ServiceProvider CrearContenedor() =>
        new ServiceCollection()
            .AddSingleton<TimeProvider>(new ProveedorTiempoFijo(new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero)))
            .AddAplicacion()
            .BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

    [Theory]
    [InlineData(typeof(IniciarSesionSolicitud), typeof(IniciarSesionSolicitudValidador))]
    [InlineData(typeof(SolicitudPaginada), typeof(SolicitudPaginadaValidador))]
    [InlineData(typeof(FiltroEmpleados), typeof(FiltroEmpleadosValidador))]
    [InlineData(typeof(GuardarEmpleadoSolicitud), typeof(GuardarEmpleadoSolicitudValidador))]
    [InlineData(typeof(GuardarDepartamentoSolicitud), typeof(GuardarDepartamentoSolicitudValidador))]
    [InlineData(typeof(FiltroEntidadesGubernamentales), typeof(FiltroEntidadesGubernamentalesValidador))]
    [InlineData(typeof(GuardarEntidadGubernamentalSolicitud), typeof(GuardarEntidadGubernamentalSolicitudValidador))]
    [InlineData(typeof(GuardarNominaSolicitud), typeof(GuardarNominaSolicitudValidador))]
    [InlineData(typeof(CrearUsuarioSolicitud), typeof(CrearUsuarioSolicitudValidador))]
    [InlineData(typeof(ActualizarUsuarioSolicitud), typeof(ActualizarUsuarioSolicitudValidador))]
    [InlineData(typeof(CambiarContrasenaSolicitud), typeof(CambiarContrasenaSolicitudValidador))]
    public void AddAplicacionResuelveCadaValidadorDeSolicitud(Type solicitud, Type validadorEsperado)
    {
        using var contenedor = CrearContenedor();
        using var alcance = contenedor.CreateScope();

        var validador = alcance.ServiceProvider.GetRequiredService(typeof(IValidator<>).MakeGenericType(solicitud));

        Assert.IsType(validadorEsperado, validador);
    }

    [Fact]
    public async Task ValidadoresResueltosAplicanSusReglas()
    {
        using var contenedor = CrearContenedor();
        using var alcance = contenedor.CreateScope();
        var proveedor = alcance.ServiceProvider;

        var loginVacio = await proveedor.GetRequiredService<IValidator<IniciarSesionSolicitud>>()
            .ValidateAsync(new IniciarSesionSolicitud());
        var nominaPasada = await proveedor.GetRequiredService<IValidator<GuardarNominaSolicitud>>()
            .ValidateAsync(new GuardarNominaSolicitud { FechaInicioSemana = _lunes });
        var nominaFutura = await proveedor.GetRequiredService<IValidator<GuardarNominaSolicitud>>()
            .ValidateAsync(new GuardarNominaSolicitud { FechaInicioSemana = _lunes.AddDays(7) });

        Assert.Equal(
            [nameof(IniciarSesionSolicitud.NombreUsuario), nameof(IniciarSesionSolicitud.Contrasena)],
            loginVacio.Errors.Select(error => error.PropertyName));
        Assert.True(nominaPasada.IsValid);
        Assert.Equal(MensajesValidacion.FECHA_INICIO_SEMANA_FUTURA, Assert.Single(nominaFutura.Errors).ErrorMessage);
    }

    [Fact]
    public void AddAplicacionRegistraValidadorRaizYCuatroDeCompensacion()
    {
        var servicios = new ServiceCollection().AddAplicacion();

        var validadoresEmpleado = servicios.Where(registro => registro.ServiceType == typeof(IValidator<GuardarEmpleadoSolicitud>)).ToList();
        var validadoresCompensacion = servicios
            .Where(registro => registro.ServiceType == typeof(IValidadorCompensacionEmpleado))
            .Select(registro => registro.ImplementationType)
            .ToList();

        Assert.Equal(typeof(GuardarEmpleadoSolicitudValidador), Assert.Single(validadoresEmpleado).ImplementationType);
        Assert.Equal(
            [
                typeof(ValidadorCompensacionAsalariado),
                typeof(ValidadorCompensacionAsalariadoPorComision),
                typeof(ValidadorCompensacionPorComision),
                typeof(ValidadorCompensacionPorHoras)
            ],
            validadoresCompensacion.OrderBy(tipo => tipo!.Name));
        Assert.Equal(
            Enum.GetValues<TipoEmpleado>().Length,
            validadoresCompensacion.Count);
    }
}
