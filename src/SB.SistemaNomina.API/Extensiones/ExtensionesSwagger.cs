using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace SB.SistemaNomina.API.Extensiones;

public static class ExtensionesSwagger
{
    private const string PREFIJO_DOCUMENTACION = "SB.SistemaNomina.";

    /// <summary>Swagger con esquema Bearer y comentarios XML de la solución.</summary>
    public static IServiceCollection AddSwaggerNomina(this IServiceCollection servicios)
    {
        servicios.AddEndpointsApiExplorer();
        servicios.AddSwaggerGen(opciones =>
        {
            opciones.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SB Sistema de Nómina",
                Version = "v1",
                Description = "API de nómina semanal, empleados, departamentos y entidades gubernamentales."
            });

            var esquemaBearer = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Token JWT obtenido en /api/autenticacion/iniciar-sesion.",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme.ToLowerInvariant(),
                BearerFormat = "JWT",
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = JwtBearerDefaults.AuthenticationScheme }
            };
            opciones.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, esquemaBearer);
            opciones.AddSecurityRequirement(new OpenApiSecurityRequirement { [esquemaBearer] = [] });

            foreach (var archivoXml in Directory.EnumerateFiles(AppContext.BaseDirectory, $"{PREFIJO_DOCUMENTACION}*.xml"))
            {
                opciones.IncludeXmlComments(archivoXml);
            }
        });

        return servicios;
    }
}
