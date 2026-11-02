using System.Text.Json;
using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.Infraestructura.Persistencia.Json;

namespace SB.SistemaNomina.PruebasUnitarias.Comun;

/// <summary>Carpeta temporal con repositorios JSON reales. Cada llamada crea un almacén nuevo (sin caché), como al reiniciar.</summary>
public sealed class CarpetaDatosTemporal : IDisposable
{
    public const string ARCHIVO_EMPLEADOS = "empleados.json";

    private static readonly JsonSerializerOptions _opcionesJson = OpcionesSerializacionJson.Crear();

    public CarpetaDatosTemporal()
    {
        Directory.CreateDirectory(Ruta);
    }

    public string Ruta { get; } = Path.Combine(Path.GetTempPath(), $"nomina-pruebas-{Guid.NewGuid():N}");

    public string RutaArchivo(string nombreArchivo) => Path.Combine(Ruta, nombreArchivo);

    public JsonEmpleadoRepositorio Empleados() =>
        new(new AlmacenArchivoJson<Empleado>(RutaArchivo(ARCHIVO_EMPLEADOS), empleado => empleado.Id, _opcionesJson));

    public JsonDepartamentoRepositorio Departamentos() =>
        new(new AlmacenArchivoJson<Departamento>(RutaArchivo("departamentos.json"), departamento => departamento.Id, _opcionesJson));

    public JsonNominaRepositorio Nominas() =>
        new(new AlmacenArchivoJson<Nomina>(RutaArchivo("nominas.json"), nomina => nomina.Id, _opcionesJson));

    public void Dispose() => Directory.Delete(Ruta, recursive: true);
}
