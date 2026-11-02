using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SB.SistemaNomina.Dominio.Entidades;

namespace SB.SistemaNomina.Infraestructura.Persistencia.EntityFramework.Configuraciones;

public sealed class NominaConfiguracion : IEntityTypeConfiguration<Nomina>
{
    public void Configure(EntityTypeBuilder<Nomina> constructor)
    {
        constructor.ToTable("Nominas");
        constructor.HasKey(nomina => nomina.Id);
        constructor.Property(nomina => nomina.Id).ValueGeneratedNever();

        // Una nómina por semana también en la base de datos.
        constructor.HasIndex(nomina => nomina.FechaInicioSemana).IsUnique();
        constructor.Property(nomina => nomina.CreadoPor).HasMaxLength(ConstantesColumnas.LONGITUD_NOMBRE_USUARIO).IsRequired();
        constructor.Property(nomina => nomina.MontoTotal).HasColumnType(ConstantesColumnas.TIPO_MONTO);

        constructor.HasMany(nomina => nomina.Detalles)
            .WithOne()
            .HasForeignKey(detalle => detalle.NominaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>Copia congelada del pago: sin FK al empleado (puede cambiar o desactivarse después).</summary>
public sealed class DetalleNominaConfiguracion : IEntityTypeConfiguration<DetalleNomina>
{
    private const string CLAVE_LINEA = "Id";
    private const string CLAVE_FORANEA_LINEA = "DetalleNominaId";

    public void Configure(EntityTypeBuilder<DetalleNomina> constructor)
    {
        constructor.ToTable("DetallesNomina");
        constructor.HasKey(detalle => detalle.Id);
        constructor.Property(detalle => detalle.Id).ValueGeneratedNever();
        constructor.Property(detalle => detalle.NombreCompletoEmpleado).HasMaxLength(ConstantesColumnas.LONGITUD_NOMBRE_COMPLETO).IsRequired();
        constructor.Property(detalle => detalle.NombreDepartamento).HasMaxLength(ConstantesColumnas.LONGITUD_NOMBRE_PERSONA).IsRequired();
        constructor.Property(detalle => detalle.TipoEmpleado).HasConversion<string>().HasMaxLength(ConstantesColumnas.LONGITUD_ENUMERACION);
        constructor.Property(detalle => detalle.PagoBruto).HasColumnType(ConstantesColumnas.TIPO_MONTO);

        // Clave identidad: conserva el orden de las líneas al leer.
        constructor.OwnsMany(detalle => detalle.LineasDesglose, linea =>
        {
            linea.ToTable("LineasDesgloseNomina");
            linea.WithOwner().HasForeignKey(CLAVE_FORANEA_LINEA);
            linea.Property<int>(CLAVE_LINEA).ValueGeneratedOnAdd();
            linea.HasKey(CLAVE_LINEA);
            linea.Property(lineaDesglose => lineaDesglose.Concepto).HasMaxLength(ConstantesColumnas.LONGITUD_CONCEPTO).IsRequired();
            linea.Property(lineaDesglose => lineaDesglose.Formula).HasMaxLength(ConstantesColumnas.LONGITUD_FORMULA).IsRequired();
            linea.Property(lineaDesglose => lineaDesglose.Monto).HasColumnType(ConstantesColumnas.TIPO_MONTO);
        });
    }
}
