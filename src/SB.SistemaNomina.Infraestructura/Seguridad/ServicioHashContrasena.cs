using Microsoft.AspNetCore.Identity;
using SB.SistemaNomina.Aplicacion.Interfaces.Seguridad;
using SB.SistemaNomina.Dominio.Entidades;

namespace SB.SistemaNomina.Infraestructura.Seguridad;

/// <summary>PBKDF2 con sal por usuario (PasswordHasher de ASP.NET Core Identity).</summary>
public sealed class ServicioHashContrasena : IServicioHashContrasena
{
    private readonly PasswordHasher<Usuario> _generadorHash = new();

    public string GenerarHash(Usuario usuario, string contrasena) =>
        _generadorHash.HashPassword(usuario, contrasena);

    public bool Verificar(Usuario usuario, string contrasena)
    {
        if (string.IsNullOrEmpty(usuario.HashContrasena))
        {
            return false;
        }

        return _generadorHash.VerifyHashedPassword(usuario, usuario.HashContrasena, contrasena) != PasswordVerificationResult.Failed;
    }
}
