namespace SB.SistemaNomina.Servicios.Comun;

/// <summary>
/// Serializa leer → validar → guardar de un agregado (una instancia de la API).
/// Evita claves duplicadas y que una escritura con datos viejos deshaga otra más reciente.
/// </summary>
public sealed class CerrojoEscritura
{
    private readonly SemaphoreSlim _semaforo = new(1, 1);

    public async Task<T> EjecutarAsync<T>(Func<Task<T>> operacion, CancellationToken tokenCancelacion)
    {
        await _semaforo.WaitAsync(tokenCancelacion);
        try
        {
            return await operacion();
        }
        finally
        {
            _semaforo.Release();
        }
    }

    public Task EjecutarAsync(Func<Task> operacion, CancellationToken tokenCancelacion) =>
        EjecutarAsync(
            async () =>
            {
                await operacion();
                return true;
            },
            tokenCancelacion);
}
