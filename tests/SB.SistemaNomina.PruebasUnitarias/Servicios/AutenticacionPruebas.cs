using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using SB.SistemaNomina.Aplicacion.Excepciones;
using SB.SistemaNomina.Aplicacion.Interfaces.Repositorios;
using SB.SistemaNomina.Aplicacion.Interfaces.Seguridad;
using SB.SistemaNomina.Aplicacion.Interfaces.Servicios;
using SB.SistemaNomina.Aplicacion.Modelos.Autenticacion;
using SB.SistemaNomina.Dominio.Constantes;
using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.Infraestructura.Seguridad;
using SB.SistemaNomina.PruebasUnitarias.Comun;
using SB.SistemaNomina.Servicios.Autenticacion;

namespace SB.SistemaNomina.PruebasUnitarias.Servicios;

/// <summary>Inicio de sesión con hash real (PBKDF2) y generador JWT real.</summary>
public sealed class AutenticacionPruebas
{
    private const string CONTRASENA = "Segura123";
    private const string EMISOR = "SB.SistemaNomina";
    private const string AUDIENCIA = "SB.SistemaNomina.Frontend";
    private const string MENSAJE_DURACION_INVALIDA = "Autenticacion:DuracionTokenMinutos debe ser mayor que cero.";
    private const int DURACION_MINUTOS = 60;

    private static readonly DateTimeOffset _ahora = new(2026, 11, 2, 15, 30, 0, TimeSpan.Zero);
    private static readonly string _clave = new('k', OpcionesAutenticacion.LONGITUD_MINIMA_CLAVE_BYTES);

    private readonly IUsuarioRepositorio _usuarioRepositorio = Substitute.For<IUsuarioRepositorio>();
    private readonly ServicioHashContrasena _servicioHash = new();
    private readonly Dictionary<string, Usuario> _usuarios = [];

    public AutenticacionPruebas()
    {
        Registrar("admin", "Administrador del sistema", NombresRoles.ADMINISTRADOR);
        Registrar("consulta", "Usuario de consulta", NombresRoles.USUARIO);
    }

    private void Registrar(string nombreUsuario, string nombreCompleto, string rol)
    {
        var usuario = new Usuario { NombreUsuario = nombreUsuario, NombreCompleto = nombreCompleto, Rol = rol };
        usuario.HashContrasena = _servicioHash.GenerarHash(usuario, CONTRASENA);
        _usuarioRepositorio.ObtenerPorNombreUsuarioAsync(nombreUsuario, Arg.Any<CancellationToken>()).Returns(usuario);
        _usuarios[nombreUsuario] = usuario;
    }

    private static OpcionesAutenticacion Opciones(string? clave = null, int duracionMinutos = DURACION_MINUTOS) => new()
    {
        Emisor = EMISOR,
        Audiencia = AUDIENCIA,
        ClaveFirma = clave ?? _clave,
        DuracionTokenMinutos = duracionMinutos
    };

    private static GeneradorTokenJwt Generador(OpcionesAutenticacion opciones) =>
        new(Options.Create(opciones), new ProveedorTiempoFijo(_ahora));

    private IAutenticacionServicio Servicio(IGeneradorToken? generador = null) =>
        ContenedorPruebas
            .Crear(_ahora, servicios =>
            {
                servicios.AddSingleton(_usuarioRepositorio);
                servicios.AddSingleton<IServicioHashContrasena>(_servicioHash);
                servicios.AddSingleton(generador ?? Generador(Opciones()));
            })
            .GetRequiredService<IAutenticacionServicio>();

    private static IniciarSesionSolicitud Solicitud(string nombreUsuario, string contrasena) =>
        new() { NombreUsuario = nombreUsuario, Contrasena = contrasena };

    private static TokenValidationParameters Parametros(string clave) => new()
    {
        ValidIssuer = EMISOR,
        ValidAudience = AUDIENCIA,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave)),
        ValidateLifetime = false
    };

    [Theory]
    [InlineData("admin", "Administrador del sistema", NombresRoles.ADMINISTRADOR)]
    [InlineData("consulta", "Usuario de consulta", NombresRoles.USUARIO)]
    public async Task CredencialesCorrectasDevuelvenTokenFirmadoConSuIdentidadYRol(string nombreUsuario, string nombreCompleto, string rol)
    {
        var respuesta = await Servicio().IniciarSesionAsync(Solicitud($" {nombreUsuario} ", CONTRASENA), CancellationToken.None);

        Assert.Equal((nombreUsuario, nombreCompleto, rol), (respuesta.NombreUsuario, respuesta.NombreCompleto, respuesta.Rol));
        Assert.Equal(_ahora.UtcDateTime.AddMinutes(DURACION_MINUTOS), respuesta.FechaExpiracion);

        // Vigencia desactivada en la validación: se compara contra el reloj fijo.
        var principal = new JwtSecurityTokenHandler { MapInboundClaims = false }
            .ValidateToken(respuesta.Token, Parametros(_clave), out var tokenValidado);
        Assert.Equal(_usuarios[nombreUsuario].Id.ToString(), principal.FindFirstValue(JwtRegisteredClaimNames.Sub));
        Assert.Equal(nombreUsuario, principal.FindFirstValue(ClaimTypes.Name));
        Assert.Equal(rol, principal.FindFirstValue(ClaimTypes.Role));
        Assert.Equal((_ahora.UtcDateTime, respuesta.FechaExpiracion), (tokenValidado.ValidFrom, tokenValidado.ValidTo));
    }

    [Fact]
    public async Task TokenConOtraClaveNoValida()
    {
        var respuesta = await Servicio().IniciarSesionAsync(Solicitud("admin", CONTRASENA), CancellationToken.None);

        var excepcion = Record.Exception(() =>
            new JwtSecurityTokenHandler().ValidateToken(respuesta.Token, Parametros(new string('x', 32)), out _));

        Assert.IsAssignableFrom<SecurityTokenException>(excepcion);
    }

    [Theory]
    [InlineData("admin", "Incorrecta1", true)]
    [InlineData("noexiste", CONTRASENA, true)]
    [InlineData("admin", CONTRASENA, false)]
    public async Task CredencialesRechazadasNoGeneranToken(string nombreUsuario, string contrasena, bool usuarioActivo)
    {
        _usuarios["admin"].EstaActivo = usuarioActivo;
        var generador = Substitute.For<IGeneradorToken>();

        var excepcion = await Assert.ThrowsAsync<CredencialesInvalidasException>(() =>
            Servicio(generador).IniciarSesionAsync(Solicitud(nombreUsuario, contrasena), CancellationToken.None));

        Assert.Equal(CredencialesInvalidasException.MENSAJE, excepcion.Message);
        generador.DidNotReceiveWithAnyArgs().Generar(default!);
    }

    [Fact]
    public async Task SolicitudInvalidaSeRechazaSinConsultarUsuario()
    {
        var generador = Substitute.For<IGeneradorToken>();

        var excepcion = await Assert.ThrowsAsync<ValidacionSolicitudException>(() =>
            Servicio(generador).IniciarSesionAsync(Solicitud("admin", string.Empty), CancellationToken.None));

        Assert.Equal([nameof(IniciarSesionSolicitud.Contrasena)], excepcion.Errores.Keys);
        await _usuarioRepositorio.DidNotReceiveWithAnyArgs().ObtenerPorNombreUsuarioAsync(default!, default);
        generador.DidNotReceiveWithAnyArgs().Generar(default!);
    }

    [Theory]
    [InlineData(31, false)]
    [InlineData(32, true)]
    public void ClaveDeFirmaRequiereTreintaYDosBytes(int longitud, bool esValida)
    {
        var excepcion = Record.Exception(() => Generador(Opciones(clave: new string('k', longitud))));

        Assert.Equal(esValida, excepcion is null);
        if (!esValida)
        {
            Assert.IsType<InvalidOperationException>(excepcion);
        }
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void DuracionDelTokenNoPositivaSeRechaza(int duracionMinutos)
    {
        var excepcion = Assert.Throws<InvalidOperationException>(() => Generador(Opciones(duracionMinutos: duracionMinutos)));

        Assert.Equal(MENSAJE_DURACION_INVALIDA, excepcion.Message);
    }

    [Fact]
    public void DuracionDeUnMinutoExpiraUnMinutoDespues()
    {
        var token = Generador(Opciones(duracionMinutos: 1)).Generar(_usuarios["admin"]);

        Assert.Equal(_ahora.UtcDateTime.AddMinutes(1), token.FechaExpiracion);
    }
}
