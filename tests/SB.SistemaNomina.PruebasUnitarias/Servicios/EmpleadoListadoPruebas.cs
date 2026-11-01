using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SB.SistemaNomina.Aplicacion.Excepciones;
using SB.SistemaNomina.Aplicacion.Interfaces.Repositorios;
using SB.SistemaNomina.Aplicacion.Interfaces.Seguridad;
using SB.SistemaNomina.Aplicacion.Interfaces.Servicios;
using SB.SistemaNomina.Aplicacion.Modelos.Empleados;
using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.Dominio.Enumeraciones;
using SB.SistemaNomina.PruebasUnitarias.Comun;

namespace SB.SistemaNomina.PruebasUnitarias.Servicios;

/// <summary>Listado de empleados con servicio y repositorios JSON reales: filtros, orden, paginación y cédula por rol.</summary>
public sealed class EmpleadoListadoPruebas : IDisposable
{
    private static readonly DateTimeOffset _ahora = new(2026, 11, 2, 15, 30, 0, TimeSpan.Zero);

    private readonly CarpetaDatosTemporal _carpeta = new();
    private readonly Departamento _finanzas = new() { Nombre = "Finanzas" };
    private readonly Departamento _legal = new() { Nombre = "Legal", EstaActivo = false };
    private readonly IUsuarioActual _usuarioActual = Substitute.For<IUsuarioActual>();
    private readonly IEmpleadoServicio _servicio;

    public EmpleadoListadoPruebas()
    {
        var empleados = _carpeta.Empleados();
        var departamentos = _carpeta.Departamentos();
        _servicio = ContenedorPruebas
            .Crear(_ahora, servicios => servicios
                .AddSingleton<IEmpleadoRepositorio>(empleados)
                .AddSingleton<IDepartamentoRepositorio>(departamentos)
                .AddSingleton(_usuarioActual))
            .GetRequiredService<IEmpleadoServicio>();

        // Insertados fuera de orden; "Zoe Pérez" antes que "Ana Pérez" prueba el desempate por nombre.
        departamentos.AgregarVariosAsync([_finanzas, _legal], CancellationToken.None).GetAwaiter().GetResult();
        empleados.AgregarVariosAsync(
            [
                new EmpleadoAsalariado { PrimerNombre = "Marta", ApellidoPaterno = "Ruiz", Cedula = "001-0000004-4", DepartamentoId = _finanzas.Id, SalarioSemanal = 900m, Estado = EstadoEmpleado.Inactivo },
                new EmpleadoAsalariado { PrimerNombre = "Zoe", ApellidoPaterno = "Pérez", Cedula = "001-0000005-5", DepartamentoId = _finanzas.Id, SalarioSemanal = 800m },
                new EmpleadoPorComision { PrimerNombre = "Ana", ApellidoPaterno = "Rosario", Cedula = "001-0000003-3", DepartamentoId = _legal.Id, VentasBrutas = 1000m, TarifaComision = 0.1m },
                new EmpleadoAsalariado { PrimerNombre = "Ana", ApellidoPaterno = "Pérez", Cedula = "001-0000001-1", DepartamentoId = _finanzas.Id, SalarioSemanal = 1000m },
                new EmpleadoPorHoras { PrimerNombre = "Luis", ApellidoPaterno = "Gómez", Cedula = "001-0000002-2", DepartamentoId = _finanzas.Id, SueldoPorHora = 100m, HorasTrabajadas = 40m }
            ],
            CancellationToken.None).GetAwaiter().GetResult();
    }

    public void Dispose() => _carpeta.Dispose();

    private FiltroEmpleados Filtro(string escenario) => escenario switch
    {
        "nombre" => new FiltroEmpleados { Nombre = "ANA" },
        "departamento" => new FiltroEmpleados { DepartamentoId = _finanzas.Id },
        "estado" => new FiltroEmpleados { Estado = EstadoEmpleado.Inactivo },
        "tipo" => new FiltroEmpleados { Tipo = TipoEmpleado.PorHoras },
        "combinado" => new FiltroEmpleados { Nombre = "a", DepartamentoId = _finanzas.Id, Estado = EstadoEmpleado.Activo, Tipo = TipoEmpleado.Asalariado },
        _ => new FiltroEmpleados()
    };

    [Theory]
    [InlineData("todos", "Luis Gómez,Ana Pérez,Zoe Pérez,Ana Rosario,Marta Ruiz")]
    [InlineData("nombre", "Ana Pérez,Ana Rosario")]
    [InlineData("departamento", "Luis Gómez,Ana Pérez,Zoe Pérez,Marta Ruiz")]
    [InlineData("estado", "Marta Ruiz")]
    [InlineData("tipo", "Luis Gómez")]
    [InlineData("combinado", "Ana Pérez")]
    public async Task FiltrosSeleccionanYOrdenanPorApellidoYNombre(string escenario, string nombresEsperados)
    {
        var resultado = await _servicio.ListarAsync(Filtro(escenario), CancellationToken.None);

        Assert.Equal(nombresEsperados.Split(','), resultado.Elementos.Select(empleado => empleado.NombreCompleto));
        Assert.Equal(resultado.Elementos.Count, resultado.TotalElementos);
    }

    [Fact]
    public async Task PaginaDevuelveSuTramoYMetadatos()
    {
        var resultado = await _servicio.ListarAsync(new FiltroEmpleados { Pagina = 2, TamanoPagina = 3 }, CancellationToken.None);

        Assert.Equal(["Ana Rosario", "Marta Ruiz"], resultado.Elementos.Select(empleado => empleado.NombreCompleto));
        Assert.Equal((2, 3, 5, 2), (resultado.Pagina, resultado.TamanoPagina, resultado.TotalElementos, resultado.TotalPaginas));
    }

    [Fact]
    public async Task FiltroInvalidoSeRechaza()
    {
        var excepcion = await Assert.ThrowsAsync<ValidacionSolicitudException>(() =>
            _servicio.ListarAsync(new FiltroEmpleados { TamanoPagina = 101 }, CancellationToken.None));

        Assert.Equal([nameof(FiltroEmpleados.TamanoPagina)], excepcion.Errores.Keys);
    }

    [Fact]
    public async Task EmpleadoDeDepartamentoInactivoMuestraElNombreDelDepartamento()
    {
        var resultado = await _servicio.ListarAsync(new FiltroEmpleados { Tipo = TipoEmpleado.PorComision }, CancellationToken.None);

        var empleado = Assert.Single(resultado.Elementos);
        Assert.Equal((_legal.Id, "Legal"), (empleado.DepartamentoId, empleado.NombreDepartamento));
    }

    [Theory]
    // Orden del listado: Gómez, Ana Pérez, Zoe Pérez, Rosario, Ruiz.
    [InlineData(true, "001-0000002-2,001-0000001-1,001-0000005-5,001-0000003-3,001-0000004-4")]
    [InlineData(false, "***-****002-2,***-****001-1,***-****005-5,***-****003-3,***-****004-4")]
    public async Task ListadoMuestraCedulaSegunRol(bool esAdministrador, string cedulasEsperadas)
    {
        _usuarioActual.EsAdministrador.Returns(esAdministrador);

        var resultado = await _servicio.ListarAsync(new FiltroEmpleados(), CancellationToken.None);

        Assert.Equal(cedulasEsperadas.Split(','), resultado.Elementos.Select(empleado => empleado.Cedula));
    }
}
