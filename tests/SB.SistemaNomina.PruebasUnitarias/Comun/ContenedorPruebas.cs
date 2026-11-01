using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SB.SistemaNomina.Servicios;

namespace SB.SistemaNomina.PruebasUnitarias.Comun;

/// <summary>Servicios reales (validadores, fábrica, creadores) con reloj fijo; la prueba registra repositorios y dobles.</summary>
public static class ContenedorPruebas
{
    public static ServiceProvider Crear(DateTimeOffset ahora, Action<IServiceCollection> registrar)
    {
        var servicios = new ServiceCollection()
            .AddSingleton<TimeProvider>(new ProveedorTiempoFijo(ahora))
            .AddSingleton(typeof(ILogger<>), typeof(NullLogger<>))
            .AddServicios();
        registrar(servicios);
        return servicios.BuildServiceProvider();
    }
}
