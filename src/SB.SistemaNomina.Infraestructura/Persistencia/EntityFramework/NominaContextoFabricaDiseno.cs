using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SB.SistemaNomina.Infraestructura.Persistencia.EntityFramework;

/// <summary>Solo para dotnet ef: usa ConnectionStrings:SqlServer de los appsettings de la API (proyecto de inicio) o de variables de entorno.</summary>
public sealed class NominaContextoFabricaDiseno : IDesignTimeDbContextFactory<NominaContexto>
{
    private const string VARIABLE_ENTORNO = "ASPNETCORE_ENVIRONMENT";
    private const string ENTORNO_PREDETERMINADO = "Development";
    private const string ARCHIVO_CONFIGURACION = "appsettings.json";
    private const string PATRON_ARCHIVO_ENTORNO = "appsettings.{0}.json";

    public NominaContexto CreateDbContext(string[] args)
    {
        var entorno = Environment.GetEnvironmentVariable(VARIABLE_ENTORNO) ?? ENTORNO_PREDETERMINADO;

        // dotnet ef carga la salida del proyecto de inicio, que incluye sus appsettings.
        var configuracion = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(ARCHIVO_CONFIGURACION, optional: true)
            .AddJsonFile(string.Format(PATRON_ARCHIVO_ENTORNO, entorno), optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        var cadena = configuracion.GetConnectionString(InyeccionDependencias.CADENA_SQL_SERVER);
        if (string.IsNullOrWhiteSpace(cadena))
        {
            throw new InvalidOperationException(InyeccionDependencias.MENSAJE_CADENA_SQL_SERVER);
        }

        return new NominaContexto(new DbContextOptionsBuilder<NominaContexto>().UseSqlServer(cadena).Options);
    }
}
