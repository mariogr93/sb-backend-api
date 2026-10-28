namespace SB.SistemaNomina.Aplicacion.Excepciones;

/// <summary>HTTP 409: el dato ya existe (ej. número de seguro social duplicado).</summary>
public class ConflictoException : Exception
{
    public ConflictoException(string mensaje) : base(mensaje)
    {
    }
}
