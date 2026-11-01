namespace SB.SistemaNomina.Aplicacion.Excepciones;

/// <summary>HTTP 409: el dato ya existe (ej. cédula duplicada).</summary>
public class ConflictoException : Exception
{
    public ConflictoException(string mensaje) : base(mensaje)
    {
    }
}
