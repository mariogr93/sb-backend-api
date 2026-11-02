using SB.SistemaNomina.Aplicacion.Comun;

namespace SB.SistemaNomina.Aplicacion.Excepciones;

/// <summary>HTTP 400 con errores por campo.</summary>
public class ValidacionSolicitudException : Exception
{
    public ValidacionSolicitudException(IReadOnlyDictionary<string, string[]> errores)
        : base(MensajesValidacion.SOLICITUD_INVALIDA)
    {
        Errores = errores;
    }

    public IReadOnlyDictionary<string, string[]> Errores { get; }
}
