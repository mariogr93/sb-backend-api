using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.Dominio.Enumeraciones;

namespace SB.SistemaNomina.Infraestructura.Persistencia;

/// <summary>Subclases de Empleado descubiertas por reflexión: tipo nuevo no requiere editar la persistencia (JSON ni SQL Server).</summary>
public static class SubtiposEmpleado
{
    public static IReadOnlyDictionary<Type, TipoEmpleado> Todos { get; } = typeof(Empleado).Assembly.GetTypes()
        .Where(tipo => tipo is { IsClass: true, IsAbstract: false } && tipo.IsSubclassOf(typeof(Empleado)))
        .ToDictionary(tipo => tipo, tipo => ((Empleado)Activator.CreateInstance(tipo)!).Tipo);
}
