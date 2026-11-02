using Microsoft.Extensions.DependencyInjection;
using SB.SistemaNomina.Aplicacion;
using SB.SistemaNomina.Aplicacion.Interfaces.Fabricas;
using SB.SistemaNomina.Aplicacion.Interfaces.Servicios;
using SB.SistemaNomina.Aplicacion.Interfaces.Seguridad;
using SB.SistemaNomina.Servicios.Autenticacion;
using SB.SistemaNomina.Servicios.Departamentos;
using SB.SistemaNomina.Servicios.Empleados;
using SB.SistemaNomina.Servicios.EntidadesGubernamentales;
using SB.SistemaNomina.Servicios.Nominas;
using SB.SistemaNomina.Servicios.Usuarios;

namespace SB.SistemaNomina.Servicios;

public static class InyeccionDependencias
{
    /// <summary>Registra aplicación, fábrica, servicios y token. Requiere repositorios, hash, usuario actual, TimeProvider y OpcionesAutenticacion del host.</summary>
    public static IServiceCollection AddServicios(this IServiceCollection servicios)
    {
        servicios.AddAplicacion();

        // Tipo nuevo de empleado = creador nuevo; se registra solo.
        var creadores = typeof(InyeccionDependencias).Assembly.GetTypes()
            .Where(tipo => tipo is { IsClass: true, IsAbstract: false } && typeof(ICreadorEmpleado).IsAssignableFrom(tipo));
        foreach (var creador in creadores)
        {
            servicios.AddSingleton(typeof(ICreadorEmpleado), creador);
        }

        servicios.AddSingleton<IFabricaEmpleado, FabricaEmpleado>();
        servicios.AddSingleton<IGeneradorToken, GeneradorTokenJwt>();
        servicios.AddScoped<IEmpleadoServicio, EmpleadoServicio>();
        servicios.AddScoped<IDepartamentoServicio, DepartamentoServicio>();
        servicios.AddScoped<IEntidadGubernamentalServicio, EntidadGubernamentalServicio>();
        servicios.AddScoped<IUsuarioServicio, UsuarioServicio>();
        servicios.AddScoped<IAutenticacionServicio, AutenticacionServicio>();
        servicios.AddScoped<INominaServicio, NominaServicio>();

        return servicios;
    }
}
