using System.Text.Json.Serialization;
using SB.SistemaNomina.API.Extensiones;
using SB.SistemaNomina.API.Middlewares;
using SB.SistemaNomina.Infraestructura;
using SB.SistemaNomina.Servicios;
using Serilog;

// Registro mínimo hasta leer la configuración; captura fallos de arranque.
Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var constructorAplicacion = WebApplication.CreateBuilder(args);
    var configuracion = constructorAplicacion.Configuration;

    constructorAplicacion.Host.UseSerilog((contexto, servicios, registro) => registro
        .ReadFrom.Configuration(contexto.Configuration)
        .ReadFrom.Services(servicios)
        .Enrich.FromLogContext());

    constructorAplicacion.Services
        .AddControllers()
        .AddJsonOptions(opciones => opciones.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
        .ConfigurarValidacionAutomatica();
    constructorAplicacion.Services.AddProblemasEnEspanol();
    constructorAplicacion.Services.AddSwaggerNomina();
    constructorAplicacion.Services.AddAutenticacionJwt(configuracion);
    constructorAplicacion.Services.AddProteccionHttp(configuracion);
    constructorAplicacion.Services.AddServicios();
    constructorAplicacion.Services.AddInfraestructura(configuracion, constructorAplicacion.Environment.ContentRootPath);

    var aplicacion = constructorAplicacion.Build();

    await aplicacion.Services.InicializarDatosAsync();

    // Registro de solicitudes por fuera: ve el código final, no la excepción ya manejada.
    aplicacion.UseSerilogRequestLogging(opciones =>
        opciones.EnrichDiagnosticContext = (diagnostico, contexto) =>
        {
            diagnostico.Set("NombreUsuario", contexto.User.Identity?.Name ?? "anonimo");
            diagnostico.Set("TraceId", contexto.TraceIdentifier);
        });
    aplicacion.UseMiddleware<ManejoExcepcionesMiddleware>();
    aplicacion.UseProblemasCodigoEstado();

    if (aplicacion.Environment.IsDevelopment())
    {
        aplicacion.UseSwagger();
        aplicacion.UseSwaggerUI();
    }

    aplicacion.UseHttpsRedirection();
    aplicacion.UseCors(ExtensionesProteccion.POLITICA_CORS);
    aplicacion.UseAuthentication();
    aplicacion.UseRateLimiter();
    aplicacion.UseAuthorization();
    aplicacion.MapControllers();

    await aplicacion.RunAsync();
}
catch (Exception excepcion) when (excepcion is not HostAbortedException)
{
    Log.Fatal(excepcion, "La aplicación terminó por un error de arranque.");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}
