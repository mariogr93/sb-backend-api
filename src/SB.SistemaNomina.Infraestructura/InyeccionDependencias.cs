using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SB.SistemaNomina.Aplicacion.Interfaces.Repositorios;
using SB.SistemaNomina.Aplicacion.Interfaces.Seguridad;
using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.Infraestructura.DatosIniciales;
using SB.SistemaNomina.Infraestructura.Persistencia.EntityFramework;
using SB.SistemaNomina.Infraestructura.Persistencia.EntityFramework.Repositorios;
using SB.SistemaNomina.Infraestructura.Persistencia.Json;
using SB.SistemaNomina.Infraestructura.Seguridad;

namespace SB.SistemaNomina.Infraestructura;

public static class InyeccionDependencias
{
    public const string CLAVE_PROVEEDOR = "Persistencia:Proveedor";
    public const string PROVEEDOR_JSON = "Json";
    public const string PROVEEDOR_SQL_SERVER = "SqlServer";
    public const string CADENA_BASE_DATOS_JSON = "BaseDatosJson";
    public const string CADENA_SQL_SERVER = "SqlServer";

    private const string CARPETA_PREDETERMINADA = "BaseDatos";
    private const string ARCHIVO_EMPLEADOS = "empleados.json";
    private const string ARCHIVO_DEPARTAMENTOS = "departamentos.json";
    private const string ARCHIVO_ENTIDADES = "entidades-gubernamentales.json";
    private const string ARCHIVO_USUARIOS = "usuarios.json";
    private const string ARCHIVO_NOMINAS = "nominas.json";
    internal const string MENSAJE_CADENA_SQL_SERVER =
        "Configure ConnectionStrings:SqlServer (o la variable ConnectionStrings__SqlServer) para usar el proveedor SqlServer.";

    /// <summary>Persistencia según Persistencia:Proveedor, hash de contraseñas y datos iniciales. Carpeta relativa se resuelve contra rutaContenido.</summary>
    public static IServiceCollection AddInfraestructura(this IServiceCollection servicios, IConfiguration configuracion, string rutaContenido)
    {
        var proveedor = configuracion[CLAVE_PROVEEDOR] ?? PROVEEDOR_JSON;
        if (string.Equals(proveedor, PROVEEDOR_JSON, StringComparison.OrdinalIgnoreCase))
        {
            var carpeta = configuracion.GetConnectionString(CADENA_BASE_DATOS_JSON) ?? CARPETA_PREDETERMINADA;
            AgregarPersistenciaJson(servicios, Path.GetFullPath(carpeta, rutaContenido));
        }
        else if (string.Equals(proveedor, PROVEEDOR_SQL_SERVER, StringComparison.OrdinalIgnoreCase))
        {
            var cadena = configuracion.GetConnectionString(CADENA_SQL_SERVER);
            if (string.IsNullOrWhiteSpace(cadena))
            {
                throw new InvalidOperationException(MENSAJE_CADENA_SQL_SERVER);
            }

            AgregarPersistenciaSqlServer(servicios, cadena);
        }
        else
        {
            throw new InvalidOperationException($"Proveedor de persistencia no soportado: {proveedor}. Use {PROVEEDOR_JSON} o {PROVEEDOR_SQL_SERVER}.");
        }

        servicios.TryAddSingleton(TimeProvider.System);
        servicios.AddSingleton<IServicioHashContrasena, ServicioHashContrasena>();
        servicios.Configure<OpcionesDatosIniciales>(configuracion.GetSection(OpcionesDatosIniciales.SECCION));
        servicios.AddScoped<InicializadorDatos>();

        return servicios;
    }

    /// <summary>Con SQL Server aplica migraciones pendientes; luego carga los datos iniciales.</summary>
    public static async Task InicializarDatosAsync(this IServiceProvider proveedorServicios, CancellationToken tokenCancelacion = default)
    {
        await using var alcance = proveedorServicios.CreateAsyncScope();
        if (alcance.ServiceProvider.GetService<NominaContexto>() is { } contexto)
        {
            await contexto.Database.MigrateAsync(tokenCancelacion);
        }

        await alcance.ServiceProvider.GetRequiredService<InicializadorDatos>().InicializarAsync(tokenCancelacion);
    }

    private static void AgregarPersistenciaSqlServer(IServiceCollection servicios, string cadenaConexion)
    {
        servicios.AddDbContext<NominaContexto>(opciones => opciones.UseSqlServer(cadenaConexion));
        servicios.AddScoped<IEmpleadoRepositorio, EfEmpleadoRepositorio>();
        servicios.AddScoped<IDepartamentoRepositorio, EfDepartamentoRepositorio>();
        servicios.AddScoped<IEntidadGubernamentalRepositorio, EfEntidadGubernamentalRepositorio>();
        servicios.AddScoped<IUsuarioRepositorio, EfUsuarioRepositorio>();
        servicios.AddScoped<INominaRepositorio, EfNominaRepositorio>();
    }

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
