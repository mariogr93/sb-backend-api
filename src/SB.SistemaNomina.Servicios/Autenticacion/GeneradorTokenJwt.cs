using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SB.SistemaNomina.Aplicacion.Interfaces.Seguridad;
using SB.SistemaNomina.Aplicacion.Modelos.Autenticacion;
using SB.SistemaNomina.Dominio.Entidades;

namespace SB.SistemaNomina.Servicios.Autenticacion;

public sealed class GeneradorTokenJwt : IGeneradorToken
{
    private const string MENSAJE_CLAVE_INVALIDA = "Autenticacion:ClaveFirma debe tener al menos 32 bytes.";

    private const string MENSAJE_DURACION_INVALIDA = "Autenticacion:DuracionTokenMinutos debe ser mayor que cero.";

    private readonly OpcionesAutenticacion _opciones;
    private readonly SigningCredentials _credenciales;
    private readonly TimeProvider _proveedorTiempo;

    public GeneradorTokenJwt(IOptions<OpcionesAutenticacion> opciones, TimeProvider proveedorTiempo)
    {
        _opciones = opciones.Value;
        _proveedorTiempo = proveedorTiempo;

        // Falla al arrancar si la configuración es insegura.
        var clave = Encoding.UTF8.GetBytes(_opciones.ClaveFirma);
        if (clave.Length < OpcionesAutenticacion.LONGITUD_MINIMA_CLAVE_BYTES)
        {
            throw new InvalidOperationException(MENSAJE_CLAVE_INVALIDA);
        }

        if (_opciones.DuracionTokenMinutos <= 0)
        {
            throw new InvalidOperationException(MENSAJE_DURACION_INVALIDA);
        }

        _credenciales = new SigningCredentials(new SymmetricSecurityKey(clave), SecurityAlgorithms.HmacSha256);
    }

    public TokenGenerado Generar(Usuario usuario)
    {
        var ahora = _proveedorTiempo.GetUtcNow().UtcDateTime;
        var fechaExpiracion = ahora.AddMinutes(_opciones.DuracionTokenMinutos);

        Claim[] reclamaciones =
        [
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, usuario.NombreUsuario),
            new(ClaimTypes.Role, usuario.Rol)
        ];

        var token = new JwtSecurityToken(
            _opciones.Emisor,
            _opciones.Audiencia,
            reclamaciones,
            notBefore: ahora,
            expires: fechaExpiracion,
            signingCredentials: _credenciales);

        return new TokenGenerado(new JwtSecurityTokenHandler().WriteToken(token), fechaExpiracion);
    }
}
