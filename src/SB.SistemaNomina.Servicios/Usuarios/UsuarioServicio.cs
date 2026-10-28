using FluentValidation;
using Microsoft.Extensions.Logging;
using SB.SistemaNomina.Aplicacion.Excepciones;
using SB.SistemaNomina.Aplicacion.Interfaces.Repositorios;
using SB.SistemaNomina.Aplicacion.Interfaces.Seguridad;
using SB.SistemaNomina.Aplicacion.Interfaces.Servicios;
using SB.SistemaNomina.Aplicacion.Mapeos;
using SB.SistemaNomina.Aplicacion.Modelos.Usuarios;
using SB.SistemaNomina.Aplicacion.Validadores;
using SB.SistemaNomina.Dominio.Constantes;
using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.Servicios.Comun;

namespace SB.SistemaNomina.Servicios.Usuarios;

public sealed class UsuarioServicio : IUsuarioServicio
{
    // Serializa cambios de rol/estado: dos admins no pueden desactivarse en paralelo (una instancia de la API).
    private static readonly SemaphoreSlim _cerrojoAdministradores = new(1, 1);

    private readonly IUsuarioRepositorio _usuarioRepositorio;
    private readonly IServicioHashContrasena _servicioHash;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IValidator<CrearUsuarioSolicitud> _validadorCrear;
    private readonly IValidator<ActualizarUsuarioSolicitud> _validadorActualizar;
    private readonly IValidator<CambiarContrasenaSolicitud> _validadorContrasena;
    private readonly TimeProvider _proveedorTiempo;
    private readonly ILogger<UsuarioServicio> _registro;

    public UsuarioServicio(
        IUsuarioRepositorio usuarioRepositorio,
        IServicioHashContrasena servicioHash,
        IUsuarioActual usuarioActual,
        IValidator<CrearUsuarioSolicitud> validadorCrear,
        IValidator<ActualizarUsuarioSolicitud> validadorActualizar,
        IValidator<CambiarContrasenaSolicitud> validadorContrasena,
        TimeProvider proveedorTiempo,
        ILogger<UsuarioServicio> registro)
    {
        _usuarioRepositorio = usuarioRepositorio;
        _servicioHash = servicioHash;
        _usuarioActual = usuarioActual;
        _validadorCrear = validadorCrear;
        _validadorActualizar = validadorActualizar;
        _validadorContrasena = validadorContrasena;
        _proveedorTiempo = proveedorTiempo;
        _registro = registro;
    }

    public async Task<IReadOnlyList<UsuarioRespuesta>> ListarAsync(CancellationToken tokenCancelacion)
    {
        var usuarios = await _usuarioRepositorio.ListarAsync(tokenCancelacion);
        return usuarios.Select(usuario => usuario.ARespuesta()).ToList();
    }

    public async Task<UsuarioRespuesta> ObtenerPorIdAsync(Guid id, CancellationToken tokenCancelacion) =>
        (await ObtenerExistenteAsync(id, tokenCancelacion)).ARespuesta();

    public async Task<UsuarioRespuesta> CrearAsync(CrearUsuarioSolicitud solicitud, CancellationToken tokenCancelacion)
    {
        await _validadorCrear.ValidarSolicitudAsync(solicitud, tokenCancelacion);
        var nombreUsuario = solicitud.NombreUsuario.Trim();
        if (await _usuarioRepositorio.ExisteNombreUsuarioAsync(nombreUsuario, tokenCancelacion))
        {
            throw new ConflictoException(MensajesServicio.NOMBRE_USUARIO_DUPLICADO);
        }

        var usuario = new Usuario
        {
            NombreUsuario = nombreUsuario,
            NombreCompleto = solicitud.NombreCompleto.Trim(),
            Rol = solicitud.Rol,
            FechaCreacion = _proveedorTiempo.GetUtcNow().UtcDateTime
        };
        usuario.HashContrasena = _servicioHash.GenerarHash(usuario, solicitud.Contrasena);
        await _usuarioRepositorio.AgregarAsync(usuario, tokenCancelacion);

        _registro.LogInformation("Usuario {UsuarioId} creado con rol {Rol}.", usuario.Id, usuario.Rol);
        return usuario.ARespuesta();
    }

    public async Task<UsuarioRespuesta> ActualizarAsync(Guid id, ActualizarUsuarioSolicitud solicitud, CancellationToken tokenCancelacion)
    {
        await _validadorActualizar.ValidarSolicitudAsync(solicitud, tokenCancelacion);

        await _cerrojoAdministradores.WaitAsync(tokenCancelacion);
        try
        {
            var usuario = await ObtenerExistenteAsync(id, tokenCancelacion);
            var pierdeAdministracion = solicitud.Rol != NombresRoles.ADMINISTRADOR || !solicitud.EstaActivo;
            if (pierdeAdministracion)
            {
                await ValidarPuedePerderAdministracionAsync(usuario, tokenCancelacion);
            }

            usuario.NombreCompleto = solicitud.NombreCompleto.Trim();
            usuario.Rol = solicitud.Rol;
            usuario.EstaActivo = solicitud.EstaActivo;
            await _usuarioRepositorio.ActualizarAsync(usuario, tokenCancelacion);

            _registro.LogInformation("Usuario {UsuarioId} actualizado.", usuario.Id);
            return usuario.ARespuesta();
        }
        finally
        {
            _cerrojoAdministradores.Release();
        }
    }

    public async Task CambiarContrasenaAsync(Guid id, CambiarContrasenaSolicitud solicitud, CancellationToken tokenCancelacion)
    {
        var usuario = await ObtenerExistenteAsync(id, tokenCancelacion);
        await _validadorContrasena.ValidarSolicitudAsync(solicitud, tokenCancelacion);

        usuario.HashContrasena = _servicioHash.GenerarHash(usuario, solicitud.ContrasenaNueva);
        await _usuarioRepositorio.ActualizarAsync(usuario, tokenCancelacion);

        _registro.LogInformation("Contraseña del usuario {UsuarioId} cambiada.", usuario.Id);
    }

    public async Task DesactivarAsync(Guid id, CancellationToken tokenCancelacion)
    {
        await _cerrojoAdministradores.WaitAsync(tokenCancelacion);
        try
        {
            var usuario = await ObtenerExistenteAsync(id, tokenCancelacion);
            await ValidarPuedePerderAdministracionAsync(usuario, tokenCancelacion);

            usuario.EstaActivo = false;
            await _usuarioRepositorio.ActualizarAsync(usuario, tokenCancelacion);

            _registro.LogInformation("Usuario {UsuarioId} desactivado.", usuario.Id);
        }
        finally
        {
            _cerrojoAdministradores.Release();
        }
    }

    /// <summary>Bloquea autodesactivación/autodegradación y dejar el sistema sin administradores activos.</summary>
    private async Task ValidarPuedePerderAdministracionAsync(Usuario usuario, CancellationToken tokenCancelacion)
    {
        if (usuario.Id == _usuarioActual.Id)
        {
            throw new ReglaNegocioException(MensajesServicio.NO_PUEDE_MODIFICARSE_A_SI_MISMO);
        }

        if (usuario is { EsAdministrador: true, EstaActivo: true }
            && await _usuarioRepositorio.ContarAdministradoresActivosAsync(tokenCancelacion) <= 1)
        {
            throw new ReglaNegocioException(MensajesServicio.ULTIMO_ADMINISTRADOR);
        }
    }

    private async Task<Usuario> ObtenerExistenteAsync(Guid id, CancellationToken tokenCancelacion) =>
        await _usuarioRepositorio.ObtenerPorIdAsync(id, tokenCancelacion)
        ?? throw new RecursoNoEncontradoException(NombresRecursos.USUARIO, id);
}
