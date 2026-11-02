using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SB.SistemaNomina.Dominio.Entidades;

namespace SB.SistemaNomina.Infraestructura.Persistencia.EntityFramework.Configuraciones;

/// <summary>Una tabla para todos los tipos (TPH); el discriminador se registra solo por cada subclase.</summary>
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

        var discriminador = constructor.HasDiscriminator<string>(ConstantesColumnas.DISCRIMINADOR_EMPLEADO);
        foreach (var (subtipo, tipoEmpleado) in SubtiposEmpleado.Todos)
        {
            discriminador.HasValue(subtipo, tipoEmpleado.ToString());
        }

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
