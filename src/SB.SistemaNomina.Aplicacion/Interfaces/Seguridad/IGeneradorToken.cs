using SB.SistemaNomina.Aplicacion.Modelos.Autenticacion;
using SB.SistemaNomina.Dominio.Entidades;

namespace SB.SistemaNomina.Aplicacion.Interfaces.Seguridad;

public interface IGeneradorToken
{
    TokenGenerado Generar(Usuario usuario);
}
