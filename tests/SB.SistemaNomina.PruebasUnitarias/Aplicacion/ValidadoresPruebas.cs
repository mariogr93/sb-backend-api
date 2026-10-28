using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Modelos.Empleados;
using SB.SistemaNomina.Aplicacion.Modelos.Nominas;
using SB.SistemaNomina.Aplicacion.Modelos.Usuarios;
using SB.SistemaNomina.Aplicacion.Validadores.Comun;
using SB.SistemaNomina.Aplicacion.Validadores.Empleados;
using SB.SistemaNomina.Aplicacion.Validadores.Nominas;
using SB.SistemaNomina.Aplicacion.Validadores.Usuarios;
using SB.SistemaNomina.Dominio.Enumeraciones;

namespace SB.SistemaNomina.PruebasUnitarias.Aplicacion;

public class ValidadoresPruebas
{
    private static readonly GuardarEmpleadoSolicitudValidador _validadorEmpleado = new(
    [
        new ValidadorCompensacionAsalariado(),
        new ValidadorCompensacionPorHoras(),
        new ValidadorCompensacionPorComision(),
        new ValidadorCompensacionAsalariadoPorComision()
    ]);

    private static GuardarEmpleadoSolicitud CrearSolicitudPorHoras(decimal? horasTrabajadas) => new()
    {
        PrimerNombre = "Carlos",
        ApellidoPaterno = "Hernández",
        NumeroSeguroSocial = "402-12-3456",
        DepartamentoId = Guid.NewGuid(),
        Tipo = TipoEmpleado.PorHoras,
        Compensacion = new CompensacionEmpleado { SueldoPorHora = 500m, HorasTrabajadas = horasTrabajadas }
    };

    [Fact]
    public async Task EmpleadoPorHorasValidoNoTieneErrores()
    {
        var resultado = await _validadorEmpleado.ValidateAsync(CrearSolicitudPorHoras(45m));

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public async Task EmpleadoPorHorasSinHorasTrabajadasEsInvalido()
    {
        var resultado = await _validadorEmpleado.ValidateAsync(CrearSolicitudPorHoras(null));

        Assert.Contains(resultado.Errors, error => error.PropertyName == "Compensacion.HorasTrabajadas");
    }

    [Fact]
    public async Task HorasSobreLimiteSemanalSonInvalidas()
    {
        var resultado = await _validadorEmpleado.ValidateAsync(CrearSolicitudPorHoras(169m));

        Assert.Contains(resultado.Errors, error => error.PropertyName == "Compensacion.HorasTrabajadas");
    }

    [Fact]
    public async Task TarifaComisionMayorQueUnoEsInvalida()
    {
        var solicitud = CrearSolicitudPorHoras(null) with
        {
            Tipo = TipoEmpleado.AsalariadoPorComision,
            Compensacion = new CompensacionEmpleado { VentasBrutas = 1000m, TarifaComision = 1.5m, SalarioBase = 500m }
        };

        var resultado = await _validadorEmpleado.ValidateAsync(solicitud);

        Assert.Equal("Compensacion.TarifaComision", Assert.Single(resultado.Errors).PropertyName);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(99_999_999.99, true)]
    [InlineData(-1, false)]
    [InlineData(100_000_000, false)]
    public async Task SueldoPorHoraDebeEstarEnRango(decimal sueldoPorHora, bool esValido)
    {
        var solicitud = CrearSolicitudPorHoras(168m) with
        {
            Compensacion = new CompensacionEmpleado { SueldoPorHora = sueldoPorHora, HorasTrabajadas = 168m }
        };

        var resultado = await _validadorEmpleado.ValidateAsync(solicitud);

        Assert.Equal(esValido, resultado.IsValid);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(1_000_000, true)]
    [InlineData(0, false)]
    [InlineData(int.MaxValue, false)]
    public async Task PaginaDebeEstarEnRango(int pagina, bool esValida)
    {
        var resultado = await new SolicitudPaginadaValidador().ValidateAsync(new SolicitudPaginada { Pagina = pagina, TamanoPagina = 100 });

        Assert.Equal(esValida, resultado.IsValid);
    }

    [Theory]
    [InlineData("402123456")]
    [InlineData("40-212-3456")]
    [InlineData("")]
    public async Task NumeroSeguroSocialConFormatoIncorrectoEsInvalido(string numeroSeguroSocial)
    {
        var solicitud = CrearSolicitudPorHoras(40m) with { NumeroSeguroSocial = numeroSeguroSocial };

        var resultado = await _validadorEmpleado.ValidateAsync(solicitud);

        Assert.Equal(nameof(GuardarEmpleadoSolicitud.NumeroSeguroSocial), Assert.Single(resultado.Errors).PropertyName);
    }

    [Fact]
    public async Task TipoEmpleadoNoSoportadoEsInvalido()
    {
        var solicitud = CrearSolicitudPorHoras(40m) with { Tipo = (TipoEmpleado)99 };

        var resultado = await _validadorEmpleado.ValidateAsync(solicitud);

        Assert.Equal(nameof(GuardarEmpleadoSolicitud.Tipo), Assert.Single(resultado.Errors).PropertyName);
    }

    [Theory]
    [InlineData("corta1A")]
    [InlineData("sinmayuscula1")]
    [InlineData("SINMINUSCULA1")]
    [InlineData("SinNumeroAqui")]
    public async Task ContrasenaDebilEsInvalida(string contrasena)
    {
        var solicitud = new CrearUsuarioSolicitud
        {
            NombreUsuario = "mgarcia",
            NombreCompleto = "María García",
            Contrasena = contrasena,
            Rol = "Usuario"
        };

        var resultado = await new CrearUsuarioSolicitudValidador().ValidateAsync(solicitud);

        Assert.Equal(nameof(CrearUsuarioSolicitud.Contrasena), Assert.Single(resultado.Errors).PropertyName);
    }

    [Fact]
    public async Task RolDesconocidoEsInvalido()
    {
        var solicitud = new ActualizarUsuarioSolicitud { NombreCompleto = "María García", Rol = "SuperAdmin" };

        var resultado = await new ActualizarUsuarioSolicitudValidador().ValidateAsync(solicitud);

        Assert.Equal(nameof(ActualizarUsuarioSolicitud.Rol), Assert.Single(resultado.Errors).PropertyName);
    }

    [Theory]
    [InlineData("2026-10-19", true)]
    [InlineData("2026-10-20", false)]
    [InlineData("2026-11-02", false)]
    [InlineData("0001-01-01", false)]
    public async Task FechaInicioSemanaDebeSerLunesNoFuturo(string fecha, bool esValida)
    {
        var validador = new GuardarNominaSolicitudValidador(new ProveedorTiempoFijo(new DateTimeOffset(2026, 10, 27, 12, 0, 0, TimeSpan.Zero)));

        var resultado = await validador.ValidateAsync(new GuardarNominaSolicitud { FechaInicioSemana = DateOnly.Parse(fecha) });

        Assert.Equal(esValida, resultado.IsValid);
    }

    private sealed class ProveedorTiempoFijo(DateTimeOffset ahora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => ahora;
    }
}
