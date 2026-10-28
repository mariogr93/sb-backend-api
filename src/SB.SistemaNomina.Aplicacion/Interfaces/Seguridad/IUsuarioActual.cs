namespace SB.SistemaNomina.Aplicacion.Interfaces.Seguridad;

/// <summary>Usuario autenticado de la solicitud en curso (implementado en la API desde el token).</summary>
public interface IUsuarioActual
{
    Guid? Id { get; }

    string NombreUsuario { get; }
}
