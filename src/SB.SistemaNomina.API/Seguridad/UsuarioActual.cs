using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SB.SistemaNomina.Aplicacion.Interfaces.Seguridad;

namespace SB.SistemaNomina.API.Seguridad;

/// <summary>Lee el usuario autenticado desde las reclamaciones del token.</summary>
public sealed class UsuarioActual : IUsuarioActual
{
    private readonly IHttpContextAccessor _accesoContexto;

    public UsuarioActual(IHttpContextAccessor accesoContexto)
    {
        _accesoContexto = accesoContexto;
    }

    public Guid? Id =>
        Guid.TryParse(Usuario?.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id) ? id : null;

    public string NombreUsuario => Usuario?.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

    private ClaimsPrincipal? Usuario => _accesoContexto.HttpContext?.User;
}
