using SB.SistemaNomina.Infraestructura;

var constructorAplicacion = WebApplication.CreateBuilder(args);

constructorAplicacion.Services.AddEndpointsApiExplorer();
constructorAplicacion.Services.AddSwaggerGen();
constructorAplicacion.Services.AddInfraestructura(
    constructorAplicacion.Configuration,
    constructorAplicacion.Environment.ContentRootPath);

var aplicacion = constructorAplicacion.Build();

await aplicacion.Services.InicializarDatosAsync();

if (aplicacion.Environment.IsDevelopment())
{
    aplicacion.UseSwagger();
    aplicacion.UseSwaggerUI();
}

aplicacion.UseHttpsRedirection();

await aplicacion.RunAsync();
