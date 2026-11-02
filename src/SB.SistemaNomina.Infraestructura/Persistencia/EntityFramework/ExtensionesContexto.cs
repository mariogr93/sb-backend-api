namespace SB.SistemaNomina.Infraestructura.Persistencia.EntityFramework;

public static class ExtensionesContexto
{
    /// <summary>Guarda y suelta el seguimiento: los repositorios trabajan con copias, como el proveedor JSON.</summary>
    public static async Task GuardarAsync(this NominaContexto contexto, CancellationToken tokenCancelacion)
    {
        try
        {
            await contexto.SaveChangesAsync(tokenCancelacion);
        }
        finally
        {
            contexto.ChangeTracker.Clear();
        }
    }
}
