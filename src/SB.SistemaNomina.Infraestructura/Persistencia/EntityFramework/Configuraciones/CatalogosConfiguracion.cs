using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SB.SistemaNomina.Dominio.Entidades;

namespace SB.SistemaNomina.Infraestructura.Persistencia.EntityFramework.Configuraciones;

public sealed class DepartamentoConfiguracion : IEntityTypeConfiguration<Departamento>
{
    public void Configure(EntityTypeBuilder<Departamento> constructor)
    {
        constructor.ToTable("Departamentos");
        constructor.HasKey(departamento => departamento.Id);
        constructor.Property(departamento => departamento.Id).ValueGeneratedNever();
        constructor.Property(departamento => departamento.Nombre).HasMaxLength(ConstantesColumnas.LONGITUD_NOMBRE_PERSONA).IsRequired();
        constructor.HasIndex(departamento => departamento.Nombre).IsUnique();
    }
}

public sealed class EntidadGubernamentalConfiguracion : IEntityTypeConfiguration<EntidadGubernamental>
{
    public void Configure(EntityTypeBuilder<EntidadGubernamental> constructor)
    {
        constructor.ToTable("EntidadesGubernamentales");
        constructor.HasKey(entidad => entidad.Id);
        constructor.Property(entidad => entidad.Id).ValueGeneratedNever();
        constructor.Property(entidad => entidad.Nombre).HasMaxLength(ConstantesColumnas.LONGITUD_NOMBRE_ENTIDAD).IsRequired();
        constructor.HasIndex(entidad => entidad.Nombre).IsUnique();
        constructor.Property(entidad => entidad.Categoria).HasMaxLength(ConstantesColumnas.LONGITUD_CLASIFICACION_ENTIDAD).IsRequired();
        constructor.Property(entidad => entidad.PoderDelEstado).HasMaxLength(ConstantesColumnas.LONGITUD_CLASIFICACION_ENTIDAD).IsRequired();
        constructor.Property(entidad => entidad.Sector).HasMaxLength(ConstantesColumnas.LONGITUD_CLASIFICACION_ENTIDAD).IsRequired();
    }
}

public sealed class UsuarioConfiguracion : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> constructor)
    {
        constructor.ToTable("Usuarios");
        constructor.HasKey(usuario => usuario.Id);
        constructor.Property(usuario => usuario.Id).ValueGeneratedNever();
        constructor.Ignore(usuario => usuario.EsAdministrador);
        constructor.Property(usuario => usuario.NombreUsuario).HasMaxLength(ConstantesColumnas.LONGITUD_NOMBRE_USUARIO).IsRequired();
        constructor.HasIndex(usuario => usuario.NombreUsuario).IsUnique();
        constructor.Property(usuario => usuario.NombreCompleto).HasMaxLength(ConstantesColumnas.LONGITUD_NOMBRE_PERSONA).IsRequired();
        constructor.Property(usuario => usuario.HashContrasena).HasMaxLength(ConstantesColumnas.LONGITUD_HASH_CONTRASENA).IsRequired();
        constructor.Property(usuario => usuario.Rol).HasMaxLength(ConstantesColumnas.LONGITUD_ENUMERACION).IsRequired();
    }
}
