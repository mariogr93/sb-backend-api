using System.Text.Json;

namespace SB.SistemaNomina.Infraestructura.Persistencia.Json;

/// <summary>
/// Un archivo JSON por agregado. Se carga una vez en memoria; escrituras serializadas y atómicas (archivo temporal + Move).
/// Entrega y recibe copias: quien llama nunca modifica el estado en memoria sin guardar.
/// </summary>
public sealed class AlmacenArchivoJson<TEntidad> where TEntidad : class
{
    private const string EXTENSION_TEMPORAL = ".tmp";

    private readonly string _rutaArchivo;
    private readonly Func<TEntidad, Guid> _obtenerId;
    private readonly JsonSerializerOptions _opcionesJson;
    private readonly SemaphoreSlim _cerrojo = new(1, 1);

    // Se reemplaza completo en cada escritura; las lecturas no necesitan bloqueo.
    private volatile Dictionary<Guid, TEntidad>? _elementos;

    public AlmacenArchivoJson(string rutaArchivo, Func<TEntidad, Guid> obtenerId, JsonSerializerOptions opcionesJson)
    {
        _rutaArchivo = rutaArchivo;
        _obtenerId = obtenerId;
        _opcionesJson = opcionesJson;
    }

    public async Task<IReadOnlyList<TEntidad>> ListarAsync(
        Func<IEnumerable<TEntidad>, IEnumerable<TEntidad>>? consulta,
        CancellationToken tokenCancelacion)
    {
        var elementos = await CargarAsync(tokenCancelacion);
        var resultado = consulta is null ? elementos.Values : consulta(elementos.Values);
        return resultado.Select(Clonar).ToList();
    }

    public async Task<(IReadOnlyList<TEntidad> Elementos, int Total)> PaginarAsync(
        Func<IEnumerable<TEntidad>, IEnumerable<TEntidad>> consulta,
        int omitir,
        int tomar,
        CancellationToken tokenCancelacion)
    {
        var elementos = await CargarAsync(tokenCancelacion);
        var filtrados = consulta(elementos.Values).ToList();
        return (filtrados.Skip(omitir).Take(tomar).Select(Clonar).ToList(), filtrados.Count);
    }

    public async Task<TEntidad?> ObtenerAsync(Guid id, CancellationToken tokenCancelacion)
    {
        var elementos = await CargarAsync(tokenCancelacion);
        return elementos.TryGetValue(id, out var entidad) ? Clonar(entidad) : null;
    }

    public async Task<TEntidad?> BuscarAsync(Func<TEntidad, bool> condicion, CancellationToken tokenCancelacion)
    {
        var elementos = await CargarAsync(tokenCancelacion);
        var entidad = elementos.Values.FirstOrDefault(condicion);
        return entidad is null ? null : Clonar(entidad);
    }

    public async Task<bool> ExisteAsync(Func<TEntidad, bool> condicion, CancellationToken tokenCancelacion) =>
        (await CargarAsync(tokenCancelacion)).Values.Any(condicion);

    public async Task<int> ContarAsync(Func<TEntidad, bool> condicion, CancellationToken tokenCancelacion) =>
        (await CargarAsync(tokenCancelacion)).Values.Count(condicion);

    public Task GuardarAsync(TEntidad entidad, CancellationToken tokenCancelacion) =>
        GuardarVariosAsync([entidad], tokenCancelacion);

    /// <summary>Inserta o reemplaza por Id en una sola escritura: se guardan todas o ninguna.</summary>
    public async Task GuardarVariosAsync(IEnumerable<TEntidad> entidades, CancellationToken tokenCancelacion)
    {
        await _cerrojo.WaitAsync(tokenCancelacion);
        try
        {
            var actuales = _elementos ?? await LeerArchivoAsync(tokenCancelacion);
            var nuevos = new Dictionary<Guid, TEntidad>(actuales);
            foreach (var entidad in entidades)
            {
                nuevos[_obtenerId(entidad)] = Clonar(entidad);
            }

            await EscribirArchivoAsync(nuevos.Values, tokenCancelacion);
            _elementos = nuevos;
        }
        finally
        {
            _cerrojo.Release();
        }
    }

    private async Task<Dictionary<Guid, TEntidad>> CargarAsync(CancellationToken tokenCancelacion)
    {
        if (_elementos is { } cargados)
        {
            return cargados;
        }

        await _cerrojo.WaitAsync(tokenCancelacion);
        try
        {
            return _elementos ??= await LeerArchivoAsync(tokenCancelacion);
        }
        finally
        {
            _cerrojo.Release();
        }
    }

    private async Task<Dictionary<Guid, TEntidad>> LeerArchivoAsync(CancellationToken tokenCancelacion)
    {
        if (!File.Exists(_rutaArchivo))
        {
            return [];
        }

        // Un archivo corrupto lanza excepción: nunca se sobrescribe en silencio.
        await using var flujo = File.OpenRead(_rutaArchivo);
        var lista = await JsonSerializer.DeserializeAsync<List<TEntidad>>(flujo, _opcionesJson, tokenCancelacion) ?? [];
        return lista.ToDictionary(_obtenerId);
    }

    private async Task EscribirArchivoAsync(IEnumerable<TEntidad> entidades, CancellationToken tokenCancelacion)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_rutaArchivo)!);
        var rutaTemporal = _rutaArchivo + EXTENSION_TEMPORAL;

        await using (var flujo = File.Create(rutaTemporal))
        {
            await JsonSerializer.SerializeAsync(flujo, entidades.ToList(), _opcionesJson, tokenCancelacion);
        }

        File.Move(rutaTemporal, _rutaArchivo, overwrite: true);
    }

    private TEntidad Clonar(TEntidad entidad) =>
        JsonSerializer.Deserialize<TEntidad>(JsonSerializer.SerializeToUtf8Bytes(entidad, _opcionesJson), _opcionesJson)!;
}
