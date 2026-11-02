using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using SB.SistemaNomina.API.Extensiones;
using SB.SistemaNomina.Aplicacion.Interfaces.Servicios;
using SB.SistemaNomina.Dominio.Constantes;

namespace SB.SistemaNomina.PruebasUnitarias.API;

/// <summary>Callback OnTokenValidated tal como lo registra la API: revoca tokens de usuarios no vigentes.</summary>
public sealed class RevocacionTokenPruebas : IDisposable
{
    private readonly IAutenticacionServicio _autenticacionServicio = Substitute.For<IAutenticacionServicio>();
    private readonly ServiceProvider _proveedor;

    public RevocacionTokenPruebas()
    {
        var configuracion = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Autenticacion:Emisor"] = "SB.SistemaNomina",
                ["Autenticacion:Audiencia"] = "SB.SistemaNomina.Frontend",
                ["Autenticacion:ClaveFirma"] = new string('k', 32),
                ["Autenticacion:DuracionTokenMinutos"] = "60"
            })
            .Build();

        var servicios = new ServiceCollection().AddLogging();
        servicios.AddAutenticacionJwt(configuracion);
        servicios.AddSingleton(_autenticacionServicio);
        _proveedor = servicios.BuildServiceProvider();
    }

    public void Dispose() => _proveedor.Dispose();

    private async Task<TokenValidatedContext> ValidarAsync(params Claim[] reclamaciones)
    {
        var opciones = _proveedor.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(JwtBearerDefaults.AuthenticationScheme);
        var esquema = new AuthenticationScheme(JwtBearerDefaults.AuthenticationScheme, null, typeof(JwtBearerHandler));
        var contexto = new TokenValidatedContext(new DefaultHttpContext { RequestServices = _proveedor }, esquema, opciones)
        {
            Principal = new ClaimsPrincipal(new ClaimsIdentity(reclamaciones, JwtBearerDefaults.AuthenticationScheme))
        };

        await opciones.Events.TokenValidated(contexto);
        return contexto;
    }

    [Theory]
    [InlineData(NombresRoles.ADMINISTRADOR)]
    [InlineData(NombresRoles.USUARIO)]
    public async Task SesionVigenteNoInterrumpeLaSolicitud(string rol)
    {
        var usuarioId = Guid.NewGuid();
        _autenticacionServicio.SesionVigenteAsync(usuarioId, rol, Arg.Any<CancellationToken>()).Returns(true);

        var contexto = await ValidarAsync(new Claim(JwtRegisteredClaimNames.Sub, usuarioId.ToString()), new Claim(ClaimTypes.Role, rol));

        Assert.Null(contexto.Result);
    }

    [Fact]
    public async Task SesionNoVigenteFallaLaAutenticacion()
    {
        var usuarioId = Guid.NewGuid();
        _autenticacionServicio.SesionVigenteAsync(usuarioId, NombresRoles.ADMINISTRADOR, Arg.Any<CancellationToken>()).Returns(false);

        var contexto = await ValidarAsync(
            new Claim(JwtRegisteredClaimNames.Sub, usuarioId.ToString()), new Claim(ClaimTypes.Role, NombresRoles.ADMINISTRADOR));

        Assert.NotNull(contexto.Result?.Failure);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("no-es-guid")]
    public async Task TokenSinIdentificadorValidoFallaSinConsultarUsuario(string? identificador)
    {
        var reclamaciones = new List<Claim> { new(ClaimTypes.Role, NombresRoles.ADMINISTRADOR) };
        if (identificador is not null)
        {
            reclamaciones.Add(new Claim(JwtRegisteredClaimNames.Sub, identificador));
        }

        var contexto = await ValidarAsync([.. reclamaciones]);

        Assert.NotNull(contexto.Result?.Failure);
        await _autenticacionServicio.DidNotReceiveWithAnyArgs().SesionVigenteAsync(default, default, default);
    }
}
