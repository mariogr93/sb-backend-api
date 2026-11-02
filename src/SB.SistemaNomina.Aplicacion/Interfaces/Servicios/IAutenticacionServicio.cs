using SB.SistemaNomina.Aplicacion.Modelos.Autenticacion;

namespace SB.SistemaNomina.Aplicacion.Interfaces.Servicios;

public interface IAutenticacionServicio
{
    /// <summary>Lanza CredencialesInvalidasException si el usuario no existe, está inactivo o la contraseña no coincide.</summary>
    Task<InicioSesionRespuesta> IniciarSesionAsync(IniciarSesionSolicitud solicitud, CancellationToken tokenCancelacion);

    /// <summary>True si el usuario del token sigue existiendo, activo y con el mismo rol.</summary>
    Task<bool> SesionVigenteAsync(Guid usuarioId, string? rol, CancellationToken tokenCancelacion);
}
