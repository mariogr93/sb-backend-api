using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.Dominio.Enumeraciones;

namespace SB.SistemaNomina.Infraestructura.Persistencia.EntityFramework.Configuraciones;

/// <summary>Una tabla para los cuatro tipos (TPH); tipo nuevo = valor nuevo del discriminador.</summary>
public sealed class EmpleadoConfiguracion : IEntityTypeConfiguration<Empleado>
{
    public void Configure(EntityTypeBuilder<Empleado> constructor)
    {
        constructor.ToTable("Empleados");
        constructor.HasKey(empleado => empleado.Id);
        constructor.Property(empleado => empleado.Id).ValueGeneratedNever();
        constructor.Ignore(empleado => empleado.Tipo);
        constructor.Ignore(empleado => empleado.NombreCompleto);
        constructor.Ignore(empleado => empleado.EstaActivo);

        constructor.Property(empleado => empleado.PrimerNombre).HasMaxLength(ConstantesColumnas.LONGITUD_NOMBRE_PERSONA).IsRequired();
        constructor.Property(empleado => empleado.ApellidoPaterno).HasMaxLength(ConstantesColumnas.LONGITUD_NOMBRE_PERSONA).IsRequired();
        constructor.Property(empleado => empleado.Cedula).HasMaxLength(ConstantesColumnas.LONGITUD_CEDULA).IsRequired();
        constructor.HasIndex(empleado => empleado.Cedula).IsUnique();
        constructor.Property(empleado => empleado.Estado).HasConversion<string>().HasMaxLength(ConstantesColumnas.LONGITUD_ENUMERACION);
        constructor.HasIndex(empleado => new { empleado.Estado, empleado.DepartamentoId });

        constructor.HasOne<Departamento>()
            .WithMany()
            .HasForeignKey(empleado => empleado.DepartamentoId)
            .OnDelete(DeleteBehavior.Restrict);

        constructor.HasDiscriminator<string>(ConstantesColumnas.DISCRIMINADOR_EMPLEADO)
            .HasValue<EmpleadoAsalariado>(nameof(TipoEmpleado.Asalariado))
            .HasValue<EmpleadoPorHoras>(nameof(TipoEmpleado.PorHoras))
            .HasValue<EmpleadoPorComision>(nameof(TipoEmpleado.PorComision))
            .HasValue<EmpleadoAsalariadoPorComision>(nameof(TipoEmpleado.AsalariadoPorComision));
        constructor.Property<string>(ConstantesColumnas.DISCRIMINADOR_EMPLEADO).HasMaxLength(ConstantesColumnas.LONGITUD_ENUMERACION);
    }
}

public sealed class EmpleadoAsalariadoConfiguracion : IEntityTypeConfiguration<EmpleadoAsalariado>
{
    public void Configure(EntityTypeBuilder<EmpleadoAsalariado> constructor) =>
        constructor.Property(empleado => empleado.SalarioSemanal).HasColumnType(ConstantesColumnas.TIPO_MONTO);
}

public sealed class EmpleadoPorHorasConfiguracion : IEntityTypeConfiguration<EmpleadoPorHoras>
{
    public void Configure(EntityTypeBuilder<EmpleadoPorHoras> constructor)
    {
        constructor.Property(empleado => empleado.SueldoPorHora).HasColumnType(ConstantesColumnas.TIPO_MONTO);
        constructor.Property(empleado => empleado.HorasTrabajadas).HasColumnType(ConstantesColumnas.TIPO_HORAS);
    }
}

public sealed class EmpleadoPorComisionConfiguracion : IEntityTypeConfiguration<EmpleadoPorComision>
{
    public void Configure(EntityTypeBuilder<EmpleadoPorComision> constructor)
    {
        constructor.Property(empleado => empleado.VentasBrutas).HasColumnType(ConstantesColumnas.TIPO_MONTO);
        constructor.Property(empleado => empleado.TarifaComision).HasColumnType(ConstantesColumnas.TIPO_TARIFA);
    }
}

public sealed class EmpleadoAsalariadoPorComisionConfiguracion : IEntityTypeConfiguration<EmpleadoAsalariadoPorComision>
{
    public void Configure(EntityTypeBuilder<EmpleadoAsalariadoPorComision> constructor) =>
        constructor.Property(empleado => empleado.SalarioBase).HasColumnType(ConstantesColumnas.TIPO_MONTO);
}
