var constructorAplicacion = WebApplication.CreateBuilder(args);

constructorAplicacion.Services.AddEndpointsApiExplorer();
constructorAplicacion.Services.AddSwaggerGen();

var aplicacion = constructorAplicacion.Build();

if (aplicacion.Environment.IsDevelopment())
{
    aplicacion.UseSwagger();
    aplicacion.UseSwaggerUI();
}

aplicacion.UseHttpsRedirection();

aplicacion.Run();
