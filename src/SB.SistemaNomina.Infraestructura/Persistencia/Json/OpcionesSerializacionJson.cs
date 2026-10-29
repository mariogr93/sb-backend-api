using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using SB.SistemaNomina.Dominio.Entidades;

namespace SB.SistemaNomina.Infraestructura.Persistencia.Json;

public static class OpcionesSerializacionJson
{
    /// <summary>Propiedad que guarda el tipo de empleado en el archivo.</summary>
    public const string DISCRIMINADOR_TIPO_EMPLEADO = "tipo";

    // Subclases de Empleado descubiertas por reflexión: tipo nuevo no requiere editar este archivo.
    private static readonly Type[] _subtiposEmpleado = typeof(Empleado).Assembly.GetTypes()
        .Where(tipo => tipo is { IsClass: true, IsAbstract: false } && tipo.IsSubclassOf(typeof(Empleado)))
        .ToArray();

    public static JsonSerializerOptions Crear()
    {
        var resolutor = new DefaultJsonTypeInfoResolver();
        resolutor.Modifiers.Add(QuitarPropiedadesCalculadas);
        resolutor.Modifiers.Add(ConfigurarEmpleadosPolimorficos);

        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            TypeInfoResolver = resolutor,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    /// <summary>Solo se guardan datos; NombreCompleto, Tipo, EstaActivo, etc. se recalculan.</summary>
    private static void QuitarPropiedadesCalculadas(JsonTypeInfo informacionTipo)
    {
        if (informacionTipo.Kind != JsonTypeInfoKind.Object)
        {
            return;
        }

        for (var indice = informacionTipo.Properties.Count - 1; indice >= 0; indice--)
        {
            if (informacionTipo.Properties[indice].Set is null)
            {
                informacionTipo.Properties.RemoveAt(indice);
            }
        }
    }

    private static void ConfigurarEmpleadosPolimorficos(JsonTypeInfo informacionTipo)
    {
        if (informacionTipo.Type != typeof(Empleado))
        {
            return;
        }

        informacionTipo.PolymorphismOptions = new JsonPolymorphismOptions
        {
            TypeDiscriminatorPropertyName = DISCRIMINADOR_TIPO_EMPLEADO,
            UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization
        };

        foreach (var subtipo in _subtiposEmpleado)
        {
            var tipoEmpleado = ((Empleado)Activator.CreateInstance(subtipo)!).Tipo;
            informacionTipo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(subtipo, tipoEmpleado.ToString()));
        }
    }
}
