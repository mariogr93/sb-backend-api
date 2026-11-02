using SB.SistemaNomina.Dominio.Entidades;

namespace SB.SistemaNomina.Aplicacion.Interfaces.Seguridad;

public interface IServicioHashContrasena
{
    string GenerarHash(Usuario usuario, string contrasena);

    bool Verificar(Usuario usuario, string contrasena);
}
