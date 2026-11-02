using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SB.SistemaNomina.Infraestructura.Persistencia.EntityFramework;

/// <summary>Solo para dotnet ef (migraciones/scripts): no se conecta a la base de datos.</summary>
public sealed class NominaContextoFabricaDiseno : IDesignTimeDbContextFactory<NominaContexto>
{
    // Marcador sin servidor real: la cadena de ejecución vive en appsettings.
    private const string CADENA_DISENO = "Server=.;Database=DisenoMigraciones";

    public NominaContexto CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<NominaContexto>().UseSqlServer(CADENA_DISENO).Options);
}
