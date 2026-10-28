using FluentValidation;
using Microsoft.Extensions.Logging;
using SB.SistemaNomina.Aplicacion.Excepciones;
using SB.SistemaNomina.Aplicacion.Interfaces.Repositorios;
using SB.SistemaNomina.Aplicacion.Interfaces.Seguridad;
using SB.SistemaNomina.Aplicacion.Interfaces.Servicios;
using SB.SistemaNomina.Aplicacion.Modelos.Autenticacion;
using SB.SistemaNomina.Aplicacion.Validadores;
using SB.SistemaNomina.Dominio.Entidades;

namespace SB.SistemaNomina.Servicios.Autenticacion;

public sealed class AutenticacionServicio : IAutenticacionServicio
{
    private const string CONTRASENA_FICTICIA = "VerificacionFicticia1";

    // Hash ficticio: usuarios inexistentes también pagan el costo del hash (evita enumeración por tiempo).
    private static string? _hashFicticio;

    private readonly IUsuarioRepositorio _usuarioRepositorio;
    private readonly IServicioHashContrasena _servicioHash;
    private readonly IGeneradorToken _generadorToken;
    private readonly IValidator<IniciarSesionSolicitud> _validadorInicioSesion;
    private readonly ILogger<AutenticacionServicio> _registro;

    public AutenticacionServicio(
        IUsuarioRepositorio usuarioRepositorio,
        IServicioHashContrasena servicioHash,
        IGeneradorToken generadorToken,
        IValidator<IniciarSesionSolicitud> validadorInicioSesion,
        ILogger<AutenticacionServicio> registro)
    {
        _usuarioRepositorio = usuarioRepositorio;
        _servicioHash = servicioHash;
        _generadorToken = generadorToken;
        _validadorInicioSesion = validadorInicioSesion;
        _registro = registro;
    }

    public async Task<InicioSesionRespuesta> IniciarSesionAsync(IniciarSesionSolicitud solicitud, CancellationToken tokenCancelacion)
    {
        await _validadorInicioSesion.ValidarSolicitudAsync(solicitud, tokenCancelacion);

        var nombreUsuario = solicitud.NombreUsuario.Trim();
        var usuario = await _usuarioRepositorio.ObtenerPorNombreUsuarioAsync(nombreUsuario, tokenCancelacion);
        var usuarioVerificado = usuario ?? new Usuario { HashContrasena = ObtenerHashFicticio() };
        var contrasenaValida = _servicioHash.Verificar(usuarioVerificado, solicitud.Contrasena);

        if (usuario is not { EstaActivo: true } || !contrasenaValida)
        {
            _registro.LogWarning("Inicio de sesión fallido para {NombreUsuario}.", nombreUsuario);
            throw new CredencialesInvalidasException();
        }

        var token = _generadorToken.Generar(usuario);
        _registro.LogInformation("Usuario {UsuarioId} inició sesión.", usuario.Id);

        return new InicioSesionRespuesta(token.Token, token.FechaExpiracion, usuario.NombreUsuario, usuario.NombreCompleto, usuario.Rol);
    }

    private string ObtenerHashFicticio() =>
        _hashFicticio ??= _servicioHash.GenerarHash(new Usuario(), CONTRASENA_FICTICIA);
}
