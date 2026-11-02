namespace SB.SistemaNomina.Aplicacion.Excepciones;

/// <summary>HTTP 404.</summary>
public class RecursoNoEncontradoException : Exception
{
    public RecursoNoEncontradoException(string recurso, object identificador)
        : base($"{recurso} con identificador '{identificador}' no existe.")
    {
    }
}
