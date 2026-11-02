using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.Dominio.Enumeraciones;
using SB.SistemaNomina.PruebasUnitarias.Comun;

namespace SB.SistemaNomina.PruebasUnitarias.Infraestructura;

/// <summary>Almacén JSON real: lo guardado sobrevive a un reinicio y un error nunca destruye lo que ya estaba.</summary>
public sealed class PersistenciaJsonPruebas : IDisposable
{
    private readonly CarpetaDatosTemporal _carpeta = new();

    public void Dispose() => _carpeta.Dispose();

    private static EmpleadoAsalariado Asalariado() =>
        new() { PrimerNombre = "Ana", ApellidoPaterno = "Pérez", Cedula = "001-0000001-1", SalarioSemanal = 1500m };

    [Fact]
    public async Task CuatroSubtiposSobrevivenAUnReinicio()
    {
        var departamentoId = Guid.NewGuid();
        var asalariado = Asalariado();
        var porHoras = new EmpleadoPorHoras { Cedula = "001-0000002-2", SueldoPorHora = 500m, HorasTrabajadas = 45m, Estado = EstadoEmpleado.Inactivo };
        var porComision = new EmpleadoPorComision { Cedula = "001-0000003-3", VentasBrutas = 10000m, TarifaComision = 0.06m };
        var asalariadoPorComision = new EmpleadoAsalariadoPorComision
        {
            Cedula = "001-0000004-4",
            DepartamentoId = departamentoId,
            VentasBrutas = 10000m,
            TarifaComision = 0.06m,
            SalarioBase = 1000m
        };
        await _carpeta.Empleados().AgregarVariosAsync([asalariado, porHoras, porComision, asalariadoPorComision], CancellationToken.None);

        var reiniciado = _carpeta.Empleados();

        var leidoAsalariado = Assert.IsType<EmpleadoAsalariado>(await reiniciado.ObtenerPorIdAsync(asalariado.Id, CancellationToken.None));
        Assert.Equal(("Ana", "Pérez", "001-0000001-1", 1500m), (leidoAsalariado.PrimerNombre, leidoAsalariado.ApellidoPaterno, leidoAsalariado.Cedula, leidoAsalariado.SalarioSemanal));

        var leidoPorHoras = Assert.IsType<EmpleadoPorHoras>(await reiniciado.ObtenerPorIdAsync(porHoras.Id, CancellationToken.None));
        Assert.Equal((500m, 45m, EstadoEmpleado.Inactivo, 23750m), (leidoPorHoras.SueldoPorHora, leidoPorHoras.HorasTrabajadas, leidoPorHoras.Estado, leidoPorHoras.CalcularPagoSemanal().Total));

        var leidoPorComision = Assert.IsType<EmpleadoPorComision>(await reiniciado.ObtenerPorIdAsync(porComision.Id, CancellationToken.None));
        Assert.Equal((10000m, 0.06m, 600m), (leidoPorComision.VentasBrutas, leidoPorComision.TarifaComision, leidoPorComision.CalcularPagoSemanal().Total));

        var leidoMixto = Assert.IsType<EmpleadoAsalariadoPorComision>(await reiniciado.ObtenerPorIdAsync(asalariadoPorComision.Id, CancellationToken.None));
        Assert.Equal((departamentoId, 1000m, 1700m), (leidoMixto.DepartamentoId, leidoMixto.SalarioBase, leidoMixto.CalcularPagoSemanal().Total));
    }

    [Fact]
    public async Task CedulaExistenteSeDetectaSalvoParaElMismoEmpleado()
    {
        var repositorio = _carpeta.Empleados();
        var activo = Asalariado();
        var inactivo = new EmpleadoAsalariado { Cedula = "001-0000009-9", Estado = EstadoEmpleado.Inactivo };
        await repositorio.AgregarVariosAsync([activo, inactivo], CancellationToken.None);

        Assert.True(await repositorio.ExisteCedulaAsync("001-0000001-1", null, CancellationToken.None));
        Assert.False(await repositorio.ExisteCedulaAsync("001-0000008-8", null, CancellationToken.None));
        Assert.False(await repositorio.ExisteCedulaAsync("001-0000001-1", activo.Id, CancellationToken.None));
        Assert.True(await repositorio.ExisteCedulaAsync("001-0000001-1", inactivo.Id, CancellationToken.None));
        Assert.True(await repositorio.ExisteCedulaAsync("001-0000009-9", null, CancellationToken.None));
    }

    [Fact]
    public async Task CambiosSinGuardarNoAlteranLoAlmacenado()
    {
        var repositorio = _carpeta.Empleados();
        var empleado = Asalariado();
        await repositorio.AgregarAsync(empleado, CancellationToken.None);

        empleado.SalarioSemanal = 1m;
        var leido = Assert.IsType<EmpleadoAsalariado>(await repositorio.ObtenerPorIdAsync(empleado.Id, CancellationToken.None));
        leido.PrimerNombre = "Cambiado";
        var listado = Assert.IsType<EmpleadoAsalariado>(Assert.Single(await repositorio.ListarActivosAsync(null, CancellationToken.None)));
        listado.SalarioSemanal = 2m;

        var releido = Assert.IsType<EmpleadoAsalariado>(await repositorio.ObtenerPorIdAsync(empleado.Id, CancellationToken.None));
        Assert.Equal(("Ana", 1500m), (releido.PrimerNombre, releido.SalarioSemanal));
    }

    [Fact]
    public async Task DetallesDeNominaLeidosSonCopias()
    {
        var repositorio = _carpeta.Nominas();
        var nomina = new Nomina
        {
            FechaInicioSemana = new DateOnly(2026, 10, 26),
            MontoTotal = 1500m,
            Detalles = [new DetalleNomina { PagoBruto = 1500m, LineasDesglose = [new("Salario semanal", "1500", 1500m)] }]
        };
        await repositorio.AgregarAsync(nomina, CancellationToken.None);

        var leida = await repositorio.ObtenerPorIdAsync(nomina.Id, CancellationToken.None);
        Assert.NotNull(leida);
        leida.Detalles[0].PagoBruto = 0m;
        leida.Detalles[0].LineasDesglose.Clear();

        var releida = await repositorio.ObtenerPorIdAsync(nomina.Id, CancellationToken.None);
        Assert.NotNull(releida);
        var detalle = Assert.Single(releida.Detalles);
        Assert.Equal((1500m, 1), (detalle.PagoBruto, detalle.LineasDesglose.Count));
    }

    [Fact]
    public async Task ArchivoCorruptoLanzaErrorYNoSeSobrescribe()
    {
        const string CONTENIDO_CORRUPTO = "{ esto no es json";
        var rutaArchivo = _carpeta.RutaArchivo(CarpetaDatosTemporal.ARCHIVO_EMPLEADOS);
        await File.WriteAllTextAsync(rutaArchivo, CONTENIDO_CORRUPTO);

        var excepcion = await Record.ExceptionAsync(() => _carpeta.Empleados().AgregarAsync(Asalariado(), CancellationToken.None));

        Assert.NotNull(excepcion);
        Assert.Equal(CONTENIDO_CORRUPTO, await File.ReadAllTextAsync(rutaArchivo));
    }

    [Fact]
    public async Task EscrituraFallidaConservaLoYaGuardado()
    {
        var repositorio = _carpeta.Empleados();
        var existente = Asalariado();
        await repositorio.AgregarAsync(existente, CancellationToken.None);

        // Una carpeta en la ruta del archivo temporal hace fallar la escritura.
        Directory.CreateDirectory(_carpeta.RutaArchivo(CarpetaDatosTemporal.ARCHIVO_EMPLEADOS + ".tmp"));
        var excepcion = await Record.ExceptionAsync(() => repositorio.AgregarAsync(Asalariado(), CancellationToken.None));

        Assert.NotNull(excepcion);
        Assert.Equal([existente.Id], (await repositorio.ListarActivosAsync(null, CancellationToken.None)).Select(empleado => empleado.Id));
        Assert.Equal([existente.Id], (await _carpeta.Empleados().ListarActivosAsync(null, CancellationToken.None)).Select(empleado => empleado.Id));
    }
}
