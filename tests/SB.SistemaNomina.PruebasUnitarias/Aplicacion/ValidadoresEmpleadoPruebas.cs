using System.Globalization;
using FluentValidation;
using FluentValidation.Results;
using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Modelos.Empleados;
using SB.SistemaNomina.Aplicacion.Validadores.Empleados;
using SB.SistemaNomina.Dominio.Enumeraciones;

namespace SB.SistemaNomina.PruebasUnitarias.Aplicacion;

public class ValidadoresEmpleadoPruebas
{
    private static readonly GuardarEmpleadoSolicitudValidador _validador = new(
    [
        new ValidadorCompensacionAsalariado(),
        new ValidadorCompensacionPorHoras(),
        new ValidadorCompensacionPorComision(),
        new ValidadorCompensacionAsalariadoPorComision()
    ]);

    // Todos los campos válidos; cada tipo usa solo los suyos.
    private static readonly CompensacionEmpleado _compensacionValida = new()
    {
        SalarioSemanal = 1000m,
        SueldoPorHora = 500m,
        HorasTrabajadas = 40m,
        VentasBrutas = 1000m,
        TarifaComision = 0.1m,
        SalarioBase = 500m
    };

    private static GuardarEmpleadoSolicitud CrearSolicitud(TipoEmpleado tipo, CompensacionEmpleado? compensacion = null) => new()
    {
        PrimerNombre = "Carlos",
        ApellidoPaterno = "Hernández",
        NumeroSeguroSocial = "402-12-3456",
        DepartamentoId = Guid.NewGuid(),
        Tipo = tipo,
        Compensacion = compensacion ?? _compensacionValida
    };

    private static decimal? LeerDecimal(string? valor) =>
        valor is null ? null : decimal.Parse(valor, CultureInfo.InvariantCulture);

    private static void AfirmarErrorUnico(ValidationResult resultado, string propiedad, string? mensaje = null)
    {
        var error = Assert.Single(resultado.Errors);
        Assert.Equal(propiedad, error.PropertyName);
        if (mensaje is not null)
        {
            Assert.Equal(mensaje, error.ErrorMessage);
        }
    }

    [Theory]
    [InlineData(TipoEmpleado.Asalariado)]
    [InlineData(TipoEmpleado.PorHoras)]
    [InlineData(TipoEmpleado.PorComision)]
    [InlineData(TipoEmpleado.AsalariadoPorComision)]
    public async Task SolicitudValidaDeCadaTipoNoTieneErrores(TipoEmpleado tipo)
    {
        var resultado = await _validador.ValidateAsync(CrearSolicitud(tipo));

        Assert.True(resultado.IsValid);
    }

    // Campos ajenos al tipo fuera de rango: deben ignorarse.
    private static readonly CompensacionEmpleado _compensacionInvalida = new()
    {
        SalarioSemanal = -1m,
        SueldoPorHora = -1m,
        HorasTrabajadas = 999m,
        VentasBrutas = -1m,
        TarifaComision = 5m,
        SalarioBase = -1m
    };

    private static CompensacionEmpleado SoloCamposDelTipo(TipoEmpleado tipo, CompensacionEmpleado otros) => tipo switch
    {
        TipoEmpleado.Asalariado => otros with { SalarioSemanal = 1000m },
        TipoEmpleado.PorHoras => otros with { SueldoPorHora = 500m, HorasTrabajadas = 40m },
        TipoEmpleado.PorComision => otros with { VentasBrutas = 1000m, TarifaComision = 0.1m },
        _ => otros with { VentasBrutas = 1000m, TarifaComision = 0.1m, SalarioBase = 500m }
    };

    [Theory]
    [InlineData(TipoEmpleado.Asalariado, false)]
    [InlineData(TipoEmpleado.PorHoras, false)]
    [InlineData(TipoEmpleado.PorComision, false)]
    [InlineData(TipoEmpleado.AsalariadoPorComision, false)]
    [InlineData(TipoEmpleado.Asalariado, true)]
    [InlineData(TipoEmpleado.PorHoras, true)]
    [InlineData(TipoEmpleado.PorComision, true)]
    [InlineData(TipoEmpleado.AsalariadoPorComision, true)]
    public async Task CadaTipoExigeSoloSusCampos(TipoEmpleado tipo, bool otrosCamposInvalidos)
    {
        var otros = otrosCamposInvalidos ? _compensacionInvalida : new CompensacionEmpleado();

        var resultado = await _validador.ValidateAsync(CrearSolicitud(tipo, SoloCamposDelTipo(tipo, otros)));

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public async Task ValidadorRaizPasaSuTokenAlDeCompensacion()
    {
        using var fuente = new CancellationTokenSource();
        var compensacion = new ValidadorCompensacionQueRegistraToken();
        var validador = new GuardarEmpleadoSolicitudValidador([compensacion]);

        await validador.ValidateAsync(CrearSolicitud(TipoEmpleado.Asalariado), fuente.Token);

        Assert.Equal(fuente.Token, compensacion.TokenRecibido);
    }

    private sealed class ValidadorCompensacionQueRegistraToken : AbstractValidator<GuardarEmpleadoSolicitud>, IValidadorCompensacionEmpleado
    {
        public ValidadorCompensacionQueRegistraToken()
        {
            RuleFor(solicitud => solicitud).CustomAsync((_, _, token) =>
            {
                TokenRecibido = token;
                return Task.CompletedTask;
            });
        }

        public CancellationToken TokenRecibido { get; private set; }

        public TipoEmpleado Tipo => TipoEmpleado.Asalariado;
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("-0.001", false)]
    [InlineData("0", true)]
    [InlineData("168", true)]
    [InlineData("168.001", false)]
    public async Task HorasTrabajadasDebenEstarEntreCeroY168(string? horas, bool esValido)
    {
        var solicitud = CrearSolicitud(TipoEmpleado.PorHoras, _compensacionValida with { HorasTrabajadas = LeerDecimal(horas) });

        var resultado = await _validador.ValidateAsync(solicitud);

        if (esValido)
        {
            Assert.True(resultado.IsValid);
            return;
        }

        var mensaje = horas is null ? null : MensajesValidacion.HORAS_FUERA_RANGO;
        AfirmarErrorUnico(resultado, "Compensacion.HorasTrabajadas", mensaje);
    }

    [Theory]
    [InlineData(TipoEmpleado.PorComision, null, false)]
    [InlineData(TipoEmpleado.PorComision, "-0.0001", false)]
    [InlineData(TipoEmpleado.PorComision, "0", false)]
    [InlineData(TipoEmpleado.PorComision, "0.0001", true)]
    [InlineData(TipoEmpleado.PorComision, "1", true)]
    [InlineData(TipoEmpleado.PorComision, "1.0001", false)]
    [InlineData(TipoEmpleado.AsalariadoPorComision, null, false)]
    [InlineData(TipoEmpleado.AsalariadoPorComision, "-0.0001", false)]
    [InlineData(TipoEmpleado.AsalariadoPorComision, "0", false)]
    [InlineData(TipoEmpleado.AsalariadoPorComision, "1", true)]
    [InlineData(TipoEmpleado.AsalariadoPorComision, "1.0001", false)]
    public async Task TarifaComisionDebeSerMayorQueCeroYHastaUno(TipoEmpleado tipo, string? tarifa, bool esValida)
    {
        var solicitud = CrearSolicitud(tipo, _compensacionValida with { TarifaComision = LeerDecimal(tarifa) });

        var resultado = await _validador.ValidateAsync(solicitud);

        if (esValida)
        {
            Assert.True(resultado.IsValid);
            return;
        }

        var mensaje = tarifa is null ? null : MensajesValidacion.TARIFA_COMISION_FUERA_RANGO;
        AfirmarErrorUnico(resultado, "Compensacion.TarifaComision", mensaje);
    }

    public static TheoryData<TipoEmpleado, string, string?, bool> CasosMonto()
    {
        var campos = new (TipoEmpleado Tipo, string Campo)[]
        {
            (TipoEmpleado.Asalariado, nameof(CompensacionEmpleado.SalarioSemanal)),
            (TipoEmpleado.PorHoras, nameof(CompensacionEmpleado.SueldoPorHora)),
            (TipoEmpleado.PorComision, nameof(CompensacionEmpleado.VentasBrutas)),
            (TipoEmpleado.AsalariadoPorComision, nameof(CompensacionEmpleado.VentasBrutas)),
            (TipoEmpleado.AsalariadoPorComision, nameof(CompensacionEmpleado.SalarioBase))
        };
        var valores = new (string? Valor, bool EsValido)[]
        {
            (null, false),
            ("-0.01", false),
            ("0", true),
            ("99999999.99", true),
            ("100000000.00", false)
        };

        var casos = new TheoryData<TipoEmpleado, string, string?, bool>();
        foreach (var (tipo, campo) in campos)
        {
            foreach (var (valor, esValido) in valores)
            {
                casos.Add(tipo, campo, valor, esValido);
            }
        }

        return casos;
    }

    [Theory]
    [MemberData(nameof(CasosMonto))]
    public async Task MontosDebenEstarEntreCeroYMaximo(TipoEmpleado tipo, string campo, string? valor, bool esValido)
    {
        var monto = LeerDecimal(valor);
        var compensacion = campo switch
        {
            nameof(CompensacionEmpleado.SalarioSemanal) => _compensacionValida with { SalarioSemanal = monto },
            nameof(CompensacionEmpleado.SueldoPorHora) => _compensacionValida with { SueldoPorHora = monto },
            nameof(CompensacionEmpleado.VentasBrutas) => _compensacionValida with { VentasBrutas = monto },
            _ => _compensacionValida with { SalarioBase = monto }
        };

        var resultado = await _validador.ValidateAsync(CrearSolicitud(tipo, compensacion));

        if (esValido)
        {
            Assert.True(resultado.IsValid);
            return;
        }

        AfirmarErrorUnico(resultado, $"Compensacion.{campo}");
    }

    private static GuardarEmpleadoSolicitud ConNombre(string campo, string? valor) =>
        campo == nameof(GuardarEmpleadoSolicitud.PrimerNombre)
            ? CrearSolicitud(TipoEmpleado.Asalariado) with { PrimerNombre = valor! }
            : CrearSolicitud(TipoEmpleado.Asalariado) with { ApellidoPaterno = valor! };

    // Longitudes literales del requisito (§7: máximo 100), independientes de la constante de producción.
    [Theory]
    [InlineData(nameof(GuardarEmpleadoSolicitud.PrimerNombre), 101)]
    [InlineData(nameof(GuardarEmpleadoSolicitud.ApellidoPaterno), 101)]
    public async Task NombreDeCientoUnCaracteresEsInvalido(string campo, int longitud)
    {
        var resultado = await _validador.ValidateAsync(ConNombre(campo, new string('a', longitud)));

        var error = Assert.Single(resultado.Errors);
        Assert.Equal(campo, error.PropertyName);
        Assert.Equal("MaximumLengthValidator", error.ErrorCode);
    }

    [Theory]
    [InlineData(nameof(GuardarEmpleadoSolicitud.PrimerNombre))]
    [InlineData(nameof(GuardarEmpleadoSolicitud.ApellidoPaterno))]
    public async Task NombreDeCienCaracteresEsValido(string campo)
    {
        var resultado = await _validador.ValidateAsync(ConNombre(campo, new string('a', 100)));

        Assert.True(resultado.IsValid);
    }

    [Theory]
    [InlineData(nameof(GuardarEmpleadoSolicitud.PrimerNombre), "Carlos3")]
    [InlineData(nameof(GuardarEmpleadoSolicitud.ApellidoPaterno), "Pérez@")]
    public async Task NombreConCaracteresInvalidosSeRechaza(string campo, string valor)
    {
        var resultado = await _validador.ValidateAsync(ConNombre(campo, valor));

        var error = Assert.Single(resultado.Errors);
        Assert.Equal(campo, error.PropertyName);
        Assert.Equal("RegularExpressionValidator", error.ErrorCode);
    }

    [Theory]
    [InlineData(nameof(GuardarEmpleadoSolicitud.PrimerNombre), "", "El campo Primer Nombre es requerido.")]
    [InlineData(nameof(GuardarEmpleadoSolicitud.PrimerNombre), null, "El campo Primer Nombre es requerido.")]
    [InlineData(nameof(GuardarEmpleadoSolicitud.ApellidoPaterno), "", "El campo Apellido Paterno es requerido.")]
    [InlineData(nameof(GuardarEmpleadoSolicitud.ApellidoPaterno), null, "El campo Apellido Paterno es requerido.")]
    public async Task NombreAusenteUsaReglaRequerida(string campo, string? valor, string mensaje)
    {
        var resultado = await _validador.ValidateAsync(ConNombre(campo, valor));

        var error = Assert.Single(resultado.Errors);
        Assert.Equal(campo, error.PropertyName);
        Assert.Equal("NotEmptyValidator", error.ErrorCode);
        Assert.Equal(mensaje, error.ErrorMessage);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task SeguroSocialAusenteUsaReglaRequerida(string? valor)
    {
        var solicitud = CrearSolicitud(TipoEmpleado.Asalariado) with { NumeroSeguroSocial = valor! };

        var resultado = await _validador.ValidateAsync(solicitud);

        var error = Assert.Single(resultado.Errors);
        Assert.Equal(nameof(GuardarEmpleadoSolicitud.NumeroSeguroSocial), error.PropertyName);
        Assert.Equal("NotEmptyValidator", error.ErrorCode);
        Assert.Equal("El campo Numero Seguro Social es requerido.", error.ErrorMessage);
    }

    [Fact]
    public async Task DepartamentoVacioEsInvalido()
    {
        var solicitud = CrearSolicitud(TipoEmpleado.Asalariado) with { DepartamentoId = Guid.Empty };

        var resultado = await _validador.ValidateAsync(solicitud);

        AfirmarErrorUnico(resultado, nameof(GuardarEmpleadoSolicitud.DepartamentoId));
    }

    [Fact]
    public async Task CompensacionNulaSeReportaSinExcepcion()
    {
        var solicitud = CrearSolicitud(TipoEmpleado.PorHoras) with { Compensacion = null! };

        var resultado = await _validador.ValidateAsync(solicitud);

        AfirmarErrorUnico(resultado, nameof(GuardarEmpleadoSolicitud.Compensacion));
    }

    [Theory]
    [InlineData("402123456", MensajesValidacion.NUMERO_SEGURO_SOCIAL_INVALIDO)]
    [InlineData("40-212-3456", MensajesValidacion.NUMERO_SEGURO_SOCIAL_INVALIDO)]
    [InlineData("402-12-345a", MensajesValidacion.NUMERO_SEGURO_SOCIAL_INVALIDO)]
    public async Task NumeroSeguroSocialConFormatoIncorrectoEsInvalido(string numeroSeguroSocial, string mensaje)
    {
        var solicitud = CrearSolicitud(TipoEmpleado.Asalariado) with { NumeroSeguroSocial = numeroSeguroSocial };

        var resultado = await _validador.ValidateAsync(solicitud);

        AfirmarErrorUnico(resultado, nameof(GuardarEmpleadoSolicitud.NumeroSeguroSocial), mensaje);
    }

    [Fact]
    public async Task TipoEmpleadoNoSoportadoEsInvalido()
    {
        var resultado = await _validador.ValidateAsync(CrearSolicitud((TipoEmpleado)99));

        AfirmarErrorUnico(resultado, nameof(GuardarEmpleadoSolicitud.Tipo), MensajesValidacion.TIPO_EMPLEADO_NO_SOPORTADO);
    }
}
