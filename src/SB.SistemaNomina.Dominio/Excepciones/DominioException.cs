namespace SB.SistemaNomina.Dominio.Excepciones;

/// <summary>Regla de negocio violada.</summary>
public class DominioException : Exception
{
    public DominioException(string mensaje) : base(mensaje)
    {
    }
}
