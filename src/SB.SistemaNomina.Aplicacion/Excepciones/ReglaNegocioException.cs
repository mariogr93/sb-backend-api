namespace SB.SistemaNomina.Aplicacion.Excepciones;

/// <summary>HTTP 422: operación válida en forma pero no permitida (ej. administrador desactivándose a sí mismo).</summary>
public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string mensaje) : base(mensaje)
    {
    }
}
