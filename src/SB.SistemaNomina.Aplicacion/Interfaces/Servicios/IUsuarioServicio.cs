using SB.SistemaNomina.Aplicacion.Modelos.Usuarios;

namespace SB.SistemaNomina.Aplicacion.Interfaces.Servicios;

public interface IUsuarioServicio
{
    Task<IReadOnlyList<UsuarioRespuesta>> ListarAsync(CancellationToken tokenCancelacion);

    Task<UsuarioRespuesta> ObtenerPorIdAsync(Guid id, CancellationToken tokenCancelacion);

    Task<UsuarioRespuesta> CrearAsync(CrearUsuarioSolicitud solicitud, CancellationToken tokenCancelacion);

    /// <summary>No permite quitarse el rol Admin ni desactivarse a sí mismo, ni dejar el sistema sin administradores.</summary>
    Task<UsuarioRespuesta> ActualizarAsync(Guid id, ActualizarUsuarioSolicitud solicitud, CancellationToken tokenCancelacion);

    Task CambiarContrasenaAsync(Guid id, CambiarContrasenaSolicitud solicitud, CancellationToken tokenCancelacion);

    Task DesactivarAsync(Guid id, CancellationToken tokenCancelacion);
}
