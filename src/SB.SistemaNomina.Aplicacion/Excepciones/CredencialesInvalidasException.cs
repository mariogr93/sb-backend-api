namespace SB.SistemaNomina.Aplicacion.Excepciones;

/// <summary>HTTP 401. Mensaje genérico: no revela si falló el usuario o la contraseña.</summary>
public class CredencialesInvalidasException : Exception
{
    public const string MENSAJE = "Usuario o contraseña incorrectos.";

    public CredencialesInvalidasException() : base(MENSAJE)
    {
    }
}
