using Microsoft.EntityFrameworkCore;
using SB.SistemaNomina.Dominio.Entidades;

namespace SB.SistemaNomina.Infraestructura.Persistencia.EntityFramework;

public sealed class NominaContexto : DbContext
{
    /// <summary>Sin mayúsculas ni acentos, igual que el proveedor JSON ("perez" encuentra "Pérez").</summary>
    public const string COLACION = "Latin1_General_100_CI_AI";

    public NominaContexto(DbContextOptions<NominaContexto> opciones) : base(opciones)
    {
    }

    public DbSet<Empleado> Empleados => Set<Empleado>();

    public DbSet<Departamento> Departamentos => Set<Departamento>();

    public DbSet<EntidadGubernamental> EntidadesGubernamentales => Set<EntidadGubernamental>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Nomina> Nominas => Set<Nomina>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation(COLACION);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NominaContexto).Assembly);
    }
}
