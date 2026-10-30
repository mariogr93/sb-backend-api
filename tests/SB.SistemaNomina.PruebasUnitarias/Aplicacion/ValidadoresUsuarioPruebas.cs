using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Modelos.Usuarios;
using SB.SistemaNomina.Aplicacion.Validadores.Usuarios;
using SB.SistemaNomina.Dominio.Constantes;

namespace SB.SistemaNomina.PruebasUnitarias.Aplicacion;

public class ValidadoresUsuarioPruebas
{
    private const string CONTRASENA_VALIDA = "Segura123";

    private static readonly CrearUsuarioSolicitudValidador _validadorCrear = new();
    private static readonly ActualizarUsuarioSolicitudValidador _validadorActualizar = new();
    private static readonly CambiarContrasenaSolicitudValidador _validadorContrasena = new();

    private static CrearUsuarioSolicitud CrearSolicitud(string contrasena = CONTRASENA_VALIDA, string rol = NombresRoles.USUARIO) => new()
    {
        NombreUsuario = "mgarcia",
        NombreCompleto = "María García",
        Contrasena = contrasena,
        Rol = rol
    };

    private static ActualizarUsuarioSolicitud ActualizarSolicitud(string nombreCompleto = "María García", string rol = NombresRoles.USUARIO) =>
        new() { NombreCompleto = nombreCompleto, Rol = rol };

    // Mayúscula + minúscula + dígito, rellenada hasta la longitud pedida.
    private static string ContrasenaDeLongitud(int longitud) => "Aa1" + new string('x', longitud - 3);

    [Theory]
    [InlineData(8)]
    [InlineData(128)]
    public async Task ContrasenaFuerteEnLosLimitesEsValida(int longitud)
    {
        var contrasena = ContrasenaDeLongitud(longitud);

        var resultadoCrear = await _validadorCrear.ValidateAsync(CrearSolicitud(contrasena));
        var resultadoCambio = await _validadorContrasena.ValidateAsync(new CambiarContrasenaSolicitud { ContrasenaNueva = contrasena });

        Assert.True(resultadoCrear.IsValid);
        Assert.True(resultadoCambio.IsValid);
    }

    public static TheoryData<string, string> ContrasenasInvalidas() => new()
    {
        { ContrasenaDeLongitud(7), MensajesValidacion.CONTRASENA_LONGITUD },
        { ContrasenaDeLongitud(129), MensajesValidacion.CONTRASENA_LONGITUD },
        { "sinmayuscula1", MensajesValidacion.CONTRASENA_COMPLEJIDAD },
        { "SINMINUSCULA1", MensajesValidacion.CONTRASENA_COMPLEJIDAD },
        { "SinNumeroAqui", MensajesValidacion.CONTRASENA_COMPLEJIDAD }
    };

    [Theory]
    [MemberData(nameof(ContrasenasInvalidas))]
    public async Task ContrasenaDebilEsInvalidaAlCrearYAlCambiar(string contrasena, string mensaje)
    {
        var resultadoCrear = await _validadorCrear.ValidateAsync(CrearSolicitud(contrasena));
        var resultadoCambio = await _validadorContrasena.ValidateAsync(new CambiarContrasenaSolicitud { ContrasenaNueva = contrasena });

        var errorCrear = Assert.Single(resultadoCrear.Errors);
        var errorCambio = Assert.Single(resultadoCambio.Errors);
        Assert.Equal(nameof(CrearUsuarioSolicitud.Contrasena), errorCrear.PropertyName);
        Assert.Equal(nameof(CambiarContrasenaSolicitud.ContrasenaNueva), errorCambio.PropertyName);
        Assert.Equal(mensaje, errorCrear.ErrorMessage);
        Assert.Equal(mensaje, errorCambio.ErrorMessage);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task ContrasenaAusenteUsaReglaRequerida(string? contrasena)
    {
        var resultadoCrear = await _validadorCrear.ValidateAsync(CrearSolicitud(contrasena!));
        var resultadoCambio = await _validadorContrasena.ValidateAsync(new CambiarContrasenaSolicitud { ContrasenaNueva = contrasena! });

        var errorCrear = Assert.Single(resultadoCrear.Errors);
        var errorCambio = Assert.Single(resultadoCambio.Errors);
        Assert.Equal("NotEmptyValidator", errorCrear.ErrorCode);
        Assert.Equal("NotEmptyValidator", errorCambio.ErrorCode);
        Assert.Equal("El campo Contrasena es requerido.", errorCrear.ErrorMessage);
        Assert.Equal("El campo Contrasena Nueva es requerido.", errorCambio.ErrorMessage);
    }

    [Theory]
    [InlineData(NombresRoles.ADMINISTRADOR)]
    [InlineData(NombresRoles.USUARIO)]
    public async Task RolConocidoEsValido(string rol)
    {
        var resultadoCrear = await _validadorCrear.ValidateAsync(CrearSolicitud(rol: rol));
        var resultadoActualizar = await _validadorActualizar.ValidateAsync(ActualizarSolicitud(rol: rol));

        Assert.True(resultadoCrear.IsValid);
        Assert.True(resultadoActualizar.IsValid);
    }

    [Theory]
    [InlineData("SuperAdmin")]
    [InlineData("")]
    [InlineData(null)]
    public async Task RolDesconocidoEsInvalidoAlCrearYActualizar(string? rol)
    {
        var resultadoCrear = await _validadorCrear.ValidateAsync(CrearSolicitud(rol: rol!));
        var resultadoActualizar = await _validadorActualizar.ValidateAsync(ActualizarSolicitud(rol: rol!));

        foreach (var resultado in new[] { resultadoCrear, resultadoActualizar })
        {
            var error = Assert.Single(resultado.Errors);
            Assert.Equal(nameof(CrearUsuarioSolicitud.Rol), error.PropertyName);
            Assert.Equal(MensajesValidacion.ROL_INVALIDO, error.ErrorMessage);
        }
    }

    [Theory]
    [InlineData(3)]
    [InlineData(50)]
    public async Task NombreUsuarioEnLosLimitesEsValido(int longitud)
    {
        var solicitud = CrearSolicitud() with { NombreUsuario = new string('m', longitud) };

        var resultado = await _validadorCrear.ValidateAsync(solicitud);

        Assert.True(resultado.IsValid);
    }

    [Theory]
    [InlineData("Mario_1.test-2")]
    [InlineData("ADMIN99")]
    public async Task NombreUsuarioAceptaMayusculasDigitosPuntoGuionYGuionBajo(string nombreUsuario)
    {
        var resultado = await _validadorCrear.ValidateAsync(CrearSolicitud() with { NombreUsuario = nombreUsuario });

        Assert.True(resultado.IsValid);
    }

    [Theory]
    [InlineData("mg", MensajesValidacion.NOMBRE_USUARIO_INVALIDO)]
    [InlineData("LARGO51", MensajesValidacion.NOMBRE_USUARIO_INVALIDO)]
    [InlineData("mario@", MensajesValidacion.NOMBRE_USUARIO_INVALIDO)]
    [InlineData("", null)]
    [InlineData(null, null)]
    public async Task NombreUsuarioInvalidoSeRechaza(string? nombreUsuario, string? mensaje)
    {
        var texto = nombreUsuario == "LARGO51" ? new string('m', 51) : nombreUsuario;
        var solicitud = CrearSolicitud() with { NombreUsuario = texto! };

        var resultado = await _validadorCrear.ValidateAsync(solicitud);

        var error = Assert.Single(resultado.Errors);
        Assert.Equal(nameof(CrearUsuarioSolicitud.NombreUsuario), error.PropertyName);
        Assert.Equal(mensaje ?? "El campo Nombre Usuario es requerido.", error.ErrorMessage);
    }

    [Theory]
    [InlineData("", "NotEmptyValidator")]
    [InlineData(null, "NotEmptyValidator")]
    [InlineData("Maria3", "RegularExpressionValidator")]
    [InlineData("LARGO101", "MaximumLengthValidator")]
    public async Task NombreCompletoInvalidoSeRechazaAlCrearYActualizar(string? nombreCompleto, string codigoError)
    {
        var texto = nombreCompleto == "LARGO101" ? new string('a', 101) : nombreCompleto;

        var resultadoCrear = await _validadorCrear.ValidateAsync(CrearSolicitud() with { NombreCompleto = texto! });
        var resultadoActualizar = await _validadorActualizar.ValidateAsync(ActualizarSolicitud(nombreCompleto: texto!));

        foreach (var resultado in new[] { resultadoCrear, resultadoActualizar })
        {
            var error = Assert.Single(resultado.Errors);
            Assert.Equal(nameof(CrearUsuarioSolicitud.NombreCompleto), error.PropertyName);
            Assert.Equal(codigoError, error.ErrorCode);
        }
    }

    [Fact]
    public async Task NombreCompletoDeCienCaracteresEsValido()
    {
        var nombre = new string('a', 100);

        var resultadoCrear = await _validadorCrear.ValidateAsync(CrearSolicitud() with { NombreCompleto = nombre });
        var resultadoActualizar = await _validadorActualizar.ValidateAsync(ActualizarSolicitud(nombreCompleto: nombre));

        Assert.True(resultadoCrear.IsValid);
        Assert.True(resultadoActualizar.IsValid);
    }
}
