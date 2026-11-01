using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SB.SistemaNomina.Aplicacion.Excepciones;
using SB.SistemaNomina.Aplicacion.Interfaces.Repositorios;
using SB.SistemaNomina.Aplicacion.Interfaces.Seguridad;
using SB.SistemaNomina.Aplicacion.Interfaces.Servicios;
using SB.SistemaNomina.Aplicacion.Modelos.Usuarios;
using SB.SistemaNomina.Dominio.Constantes;
using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.PruebasUnitarias.Comun;
using SB.SistemaNomina.Servicios.Comun;

namespace SB.SistemaNomina.PruebasUnitarias.Servicios;

/// <summary>Hash de contraseñas y salvaguardas de administración (autodesactivación y último administrador).</summary>
public sealed class UsuarioServicioPruebas
{
    private const string CONTRASENA = "Segura123";
    private const string HASH_GENERADO = "hash-generado";

    private static readonly DateTimeOffset _ahora = new(2026, 11, 2, 15, 30, 0, TimeSpan.Zero);

    private readonly IUsuarioRepositorio _usuarioRepositorio = Substitute.For<IUsuarioRepositorio>();
    private readonly IServicioHashContrasena _servicioHash = Substitute.For<IServicioHashContrasena>();
    private readonly IUsuarioActual _usuarioActual = Substitute.For<IUsuarioActual>();
    private readonly Usuario _administradorActual = CrearUsuario("admin", NombresRoles.ADMINISTRADOR);
    private readonly IUsuarioServicio _servicio;
    private Usuario? _agregado;
    private Usuario? _actualizado;

    public UsuarioServicioPruebas()
    {
        _usuarioActual.Id.Returns(_administradorActual.Id);
        Registrar(_administradorActual);
        _servicioHash.GenerarHash(Arg.Any<Usuario>(), Arg.Any<string>()).Returns(HASH_GENERADO);
        _usuarioRepositorio
            .When(repositorio => repositorio.AgregarAsync(Arg.Any<Usuario>(), Arg.Any<CancellationToken>()))
            .Do(llamada => _agregado = llamada.Arg<Usuario>());
        _usuarioRepositorio
            .When(repositorio => repositorio.ActualizarAsync(Arg.Any<Usuario>(), Arg.Any<CancellationToken>()))
            .Do(llamada => _actualizado = llamada.Arg<Usuario>());

        _servicio = ContenedorPruebas
            .Crear(_ahora, servicios => servicios
                .AddSingleton(_usuarioRepositorio)
                .AddSingleton(_servicioHash)
                .AddSingleton(_usuarioActual))
            .GetRequiredService<IUsuarioServicio>();
    }

    private static Usuario CrearUsuario(string nombreUsuario, string rol) =>
        new() { NombreUsuario = nombreUsuario, NombreCompleto = "Persona de prueba", Rol = rol, HashContrasena = "hash-viejo" };

    private Usuario Registrar(Usuario usuario)
    {
        _usuarioRepositorio.ObtenerPorIdAsync(usuario.Id, Arg.Any<CancellationToken>()).Returns(usuario);
        return usuario;
    }

    private void AdministradoresActivos(int cantidad) =>
        _usuarioRepositorio.ContarAdministradoresActivosAsync(Arg.Any<CancellationToken>()).Returns(cantidad);

    private static ActualizarUsuarioSolicitud Actualizacion(string rol, bool estaActivo) =>
        new() { NombreCompleto = "Persona de prueba", Rol = rol, EstaActivo = estaActivo };

    private async Task NoHuboEscriturasAsync()
    {
        await _usuarioRepositorio.DidNotReceiveWithAnyArgs().AgregarAsync(default!, default);
        await _usuarioRepositorio.DidNotReceiveWithAnyArgs().ActualizarAsync(default!, default);
    }

    [Fact]
    public async Task CrearGuardaElHashDeLaContrasenaRecibida()
    {
        var solicitud = new CrearUsuarioSolicitud { NombreUsuario = "mgarcia", NombreCompleto = "María García", Contrasena = CONTRASENA, Rol = NombresRoles.USUARIO };

        var respuesta = await _servicio.CrearAsync(solicitud, CancellationToken.None);

        _servicioHash.Received(1).GenerarHash(Arg.Any<Usuario>(), CONTRASENA);
        Assert.NotNull(_agregado);
        Assert.Equal(("mgarcia", "María García", NombresRoles.USUARIO, HASH_GENERADO), (_agregado.NombreUsuario, _agregado.NombreCompleto, _agregado.Rol, _agregado.HashContrasena));
        Assert.Equal(_ahora.UtcDateTime, _agregado.FechaCreacion);
        Assert.Equal((_agregado.Id, "mgarcia", true), (respuesta.Id, respuesta.NombreUsuario, respuesta.EstaActivo));
    }

    [Fact]
    public async Task NombreDeUsuarioDuplicadoLanzaConflictoYNoGuarda()
    {
        _usuarioRepositorio.ExisteNombreUsuarioAsync("mgarcia", Arg.Any<CancellationToken>()).Returns(true);
        var solicitud = new CrearUsuarioSolicitud { NombreUsuario = "mgarcia", NombreCompleto = "María García", Contrasena = CONTRASENA, Rol = NombresRoles.USUARIO };

        var excepcion = await Assert.ThrowsAsync<ConflictoException>(() => _servicio.CrearAsync(solicitud, CancellationToken.None));

        Assert.Equal(MensajesServicio.NOMBRE_USUARIO_DUPLICADO, excepcion.Message);
        await NoHuboEscriturasAsync();
    }

    [Fact]
    public async Task CambiarContrasenaReemplazaElHash()
    {
        var usuario = Registrar(CrearUsuario("mgarcia", NombresRoles.USUARIO));

        await _servicio.CambiarContrasenaAsync(usuario.Id, new CambiarContrasenaSolicitud { ContrasenaNueva = "Nueva1234" }, CancellationToken.None);

        _servicioHash.Received(1).GenerarHash(Arg.Is<Usuario>(recibido => recibido.Id == usuario.Id), "Nueva1234");
        Assert.NotNull(_actualizado);
        Assert.Equal((usuario.Id, HASH_GENERADO), (_actualizado.Id, _actualizado.HashContrasena));
    }

    [Theory]
    [InlineData(NombresRoles.USUARIO, true)]
    [InlineData(NombresRoles.ADMINISTRADOR, false)]
    public async Task AdministradorNoPuedeQuitarseElRolNiDesactivarse(string rol, bool estaActivo)
    {
        AdministradoresActivos(3);

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            _servicio.ActualizarAsync(_administradorActual.Id, Actualizacion(rol, estaActivo), CancellationToken.None));

        Assert.Equal(MensajesServicio.NO_PUEDE_MODIFICARSE_A_SI_MISMO, excepcion.Message);
        await NoHuboEscriturasAsync();
    }

    [Fact]
    public async Task AdministradorNoPuedeDesactivarseASiMismo()
    {
        AdministradoresActivos(3);

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            _servicio.DesactivarAsync(_administradorActual.Id, CancellationToken.None));

        Assert.Equal(MensajesServicio.NO_PUEDE_MODIFICARSE_A_SI_MISMO, excepcion.Message);
        await NoHuboEscriturasAsync();
    }

    [Fact]
    public async Task UltimoAdministradorActivoNoSePuedeDesactivarNiDegradar()
    {
        var otroAdministrador = Registrar(CrearUsuario("admin2", NombresRoles.ADMINISTRADOR));
        AdministradoresActivos(1);

        var desactivar = await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            _servicio.DesactivarAsync(otroAdministrador.Id, CancellationToken.None));
        var degradar = await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            _servicio.ActualizarAsync(otroAdministrador.Id, Actualizacion(NombresRoles.USUARIO, true), CancellationToken.None));

        Assert.Equal(MensajesServicio.ULTIMO_ADMINISTRADOR, desactivar.Message);
        Assert.Equal(MensajesServicio.ULTIMO_ADMINISTRADOR, degradar.Message);
        await NoHuboEscriturasAsync();
    }

    [Fact]
    public async Task ConDosAdministradoresSePuedeDesactivarAlOtro()
    {
        var otroAdministrador = Registrar(CrearUsuario("admin2", NombresRoles.ADMINISTRADOR));
        AdministradoresActivos(2);

        await _servicio.DesactivarAsync(otroAdministrador.Id, CancellationToken.None);

        await _usuarioRepositorio.Received(1).ActualizarAsync(Arg.Any<Usuario>(), Arg.Any<CancellationToken>());
        Assert.NotNull(_actualizado);
        Assert.Equal((otroAdministrador.Id, false), (_actualizado.Id, _actualizado.EstaActivo));
    }

    [Fact]
    public async Task UsuarioNoAdministradorSeDesactivaSinContarAdministradores()
    {
        var usuario = Registrar(CrearUsuario("consulta", NombresRoles.USUARIO));
        AdministradoresActivos(1);

        await _servicio.DesactivarAsync(usuario.Id, CancellationToken.None);

        await _usuarioRepositorio.DidNotReceive().ContarAdministradoresActivosAsync(Arg.Any<CancellationToken>());
        Assert.NotNull(_actualizado);
        Assert.False(_actualizado.EstaActivo);
    }

    // (rol inicial, activo inicial, rol pedido, activo pedido, administradores activos)
    [Theory]
    [InlineData(NombresRoles.USUARIO, true, NombresRoles.ADMINISTRADOR, true, 1)]
    [InlineData(NombresRoles.ADMINISTRADOR, true, NombresRoles.USUARIO, true, 2)]
    [InlineData(NombresRoles.USUARIO, false, NombresRoles.USUARIO, true, 1)]
    [InlineData(NombresRoles.USUARIO, true, NombresRoles.USUARIO, true, 1)]
    public async Task ActualizacionPermitidaGuardaNombreRolYEstado(string rolInicial, bool activoInicial, string rolPedido, bool activoPedido, int administradores)
    {
        var usuario = CrearUsuario("mgarcia", rolInicial);
        usuario.EstaActivo = activoInicial;
        Registrar(usuario);
        AdministradoresActivos(administradores);
        var solicitud = new ActualizarUsuarioSolicitud { NombreCompleto = "María García", Rol = rolPedido, EstaActivo = activoPedido };

        var respuesta = await _servicio.ActualizarAsync(usuario.Id, solicitud, CancellationToken.None);

        await _usuarioRepositorio.Received(1).ActualizarAsync(Arg.Any<Usuario>(), Arg.Any<CancellationToken>());
        Assert.NotNull(_actualizado);
        Assert.Equal((usuario.Id, "mgarcia", "hash-viejo"), (_actualizado.Id, _actualizado.NombreUsuario, _actualizado.HashContrasena));
        Assert.Equal(("María García", rolPedido, activoPedido), (_actualizado.NombreCompleto, _actualizado.Rol, _actualizado.EstaActivo));
        Assert.Equal((usuario.Id, "María García", rolPedido, activoPedido), (respuesta.Id, respuesta.NombreCompleto, respuesta.Rol, respuesta.EstaActivo));
    }

    [Fact]
    public async Task CrearConDatosInvalidosNoGeneraHashNiGuarda()
    {
        var solicitud = new CrearUsuarioSolicitud { NombreUsuario = "m@", NombreCompleto = "María García", Contrasena = CONTRASENA, Rol = NombresRoles.USUARIO };

        var excepcion = await Assert.ThrowsAsync<ValidacionSolicitudException>(() => _servicio.CrearAsync(solicitud, CancellationToken.None));

        Assert.Equal([nameof(CrearUsuarioSolicitud.NombreUsuario)], excepcion.Errores.Keys);
        _servicioHash.DidNotReceiveWithAnyArgs().GenerarHash(default!, default!);
        await NoHuboEscriturasAsync();
    }

    [Fact]
    public async Task ActualizarConRolInvalidoNoGuarda()
    {
        var usuario = Registrar(CrearUsuario("mgarcia", NombresRoles.USUARIO));

        var excepcion = await Assert.ThrowsAsync<ValidacionSolicitudException>(() =>
            _servicio.ActualizarAsync(usuario.Id, Actualizacion("SuperAdmin", true), CancellationToken.None));

        Assert.Equal([nameof(ActualizarUsuarioSolicitud.Rol)], excepcion.Errores.Keys);
        await NoHuboEscriturasAsync();
    }

    [Fact]
    public async Task ContrasenaDebilNoGeneraHashNiGuarda()
    {
        var usuario = Registrar(CrearUsuario("mgarcia", NombresRoles.USUARIO));

        var excepcion = await Assert.ThrowsAsync<ValidacionSolicitudException>(() =>
            _servicio.CambiarContrasenaAsync(usuario.Id, new CambiarContrasenaSolicitud { ContrasenaNueva = "debil" }, CancellationToken.None));

        Assert.Equal([nameof(CambiarContrasenaSolicitud.ContrasenaNueva)], excepcion.Errores.Keys);
        _servicioHash.DidNotReceiveWithAnyArgs().GenerarHash(default!, default!);
        await NoHuboEscriturasAsync();
    }
}
