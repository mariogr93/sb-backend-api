using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SB.SistemaNomina.Aplicacion.Interfaces.Repositorios;
using SB.SistemaNomina.Aplicacion.Interfaces.Seguridad;
using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.Infraestructura.DatosIniciales;
using SB.SistemaNomina.Infraestructura.Persistencia.Json;
using SB.SistemaNomina.Infraestructura.Seguridad;

namespace SB.SistemaNomina.Infraestructura;

public static class InyeccionDependencias
{
    public const string CLAVE_PROVEEDOR = "Persistencia:Proveedor";
    public const string PROVEEDOR_JSON = "Json";
    public const string CADENA_BASE_DATOS_JSON = "BaseDatosJson";

    private const string CARPETA_PREDETERMINADA = "BaseDatos";
    private const string ARCHIVO_EMPLEADOS = "empleados.json";
    private const string ARCHIVO_DEPARTAMENTOS = "departamentos.json";
    private const string ARCHIVO_ENTIDADES = "entidades-gubernamentales.json";
    private const string ARCHIVO_USUARIOS = "usuarios.json";
    private const string ARCHIVO_NOMINAS = "nominas.json";

    /// <summary>Persistencia según Persistencia:Proveedor, hash de contraseñas y datos iniciales. Carpeta relativa se resuelve contra rutaContenido.</summary>
    public static IServiceCollection AddInfraestructura(this IServiceCollection servicios, IConfiguration configuracion, string rutaContenido)
    {
        var proveedor = configuracion[CLAVE_PROVEEDOR] ?? PROVEEDOR_JSON;
        if (!string.Equals(proveedor, PROVEEDOR_JSON, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Proveedor de persistencia no soportado: {proveedor}.");
        }

        var carpeta = configuracion.GetConnectionString(CADENA_BASE_DATOS_JSON) ?? CARPETA_PREDETERMINADA;
        AgregarPersistenciaJson(servicios, Path.GetFullPath(carpeta, rutaContenido));

        servicios.TryAddSingleton(TimeProvider.System);
        servicios.AddSingleton<IServicioHashContrasena, ServicioHashContrasena>();
        servicios.Configure<OpcionesDatosIniciales>(configuracion.GetSection(OpcionesDatosIniciales.SECCION));
        servicios.AddSingleton<InicializadorDatos>();

        return servicios;
    }

    public static Task InicializarDatosAsync(this IServiceProvider proveedorServicios, CancellationToken tokenCancelacion = default) =>
        proveedorServicios.GetRequiredService<InicializadorDatos>().InicializarAsync(tokenCancelacion);

    private static void AgregarPersistenciaJson(IServiceCollection servicios, string rutaCarpeta)
    {
        var opcionesJson = OpcionesSerializacionJson.Crear();

        servicios.AddSingleton(new AlmacenArchivoJson<Empleado>(Path.Combine(rutaCarpeta, ARCHIVO_EMPLEADOS), empleado => empleado.Id, opcionesJson));
        servicios.AddSingleton(new AlmacenArchivoJson<Departamento>(Path.Combine(rutaCarpeta, ARCHIVO_DEPARTAMENTOS), departamento => departamento.Id, opcionesJson));
        servicios.AddSingleton(new AlmacenArchivoJson<EntidadGubernamental>(Path.Combine(rutaCarpeta, ARCHIVO_ENTIDADES), entidad => entidad.Id, opcionesJson));
        servicios.AddSingleton(new AlmacenArchivoJson<Usuario>(Path.Combine(rutaCarpeta, ARCHIVO_USUARIOS), usuario => usuario.Id, opcionesJson));
        servicios.AddSingleton(new AlmacenArchivoJson<Nomina>(Path.Combine(rutaCarpeta, ARCHIVO_NOMINAS), nomina => nomina.Id, opcionesJson));

        servicios.AddSingleton<IEmpleadoRepositorio, JsonEmpleadoRepositorio>();
        servicios.AddSingleton<IDepartamentoRepositorio, JsonDepartamentoRepositorio>();
        servicios.AddSingleton<IEntidadGubernamentalRepositorio, JsonEntidadGubernamentalRepositorio>();
        servicios.AddSingleton<IUsuarioRepositorio, JsonUsuarioRepositorio>();
        servicios.AddSingleton<INominaRepositorio, JsonNominaRepositorio>();
    }
}
