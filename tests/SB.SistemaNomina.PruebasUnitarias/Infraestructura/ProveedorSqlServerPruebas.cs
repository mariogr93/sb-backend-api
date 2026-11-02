using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SB.SistemaNomina.Aplicacion.Interfaces.Repositorios;
using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.Infraestructura;
using SB.SistemaNomina.Infraestructura.Persistencia.EntityFramework;
using SB.SistemaNomina.Infraestructura.Persistencia.EntityFramework.Repositorios;
using SB.SistemaNomina.Infraestructura.Persistencia.Json;

namespace SB.SistemaNomina.PruebasUnitarias.Infraestructura;

/// <summary>Selección de proveedor y modelo EF de SQL Server, sin base de datos real.</summary>
public sealed class ProveedorSqlServerPruebas
{
    private const string CADENA_PRUEBA = "Server=.;Database=Pruebas";

    private static readonly Type[] _repositorios =
    [
        typeof(IEmpleadoRepositorio), typeof(IDepartamentoRepositorio), typeof(IEntidadGubernamentalRepositorio),
        typeof(IUsuarioRepositorio), typeof(INominaRepositorio)
    ];

    private static IServiceCollection Registrar(string? proveedor, string? cadenaSqlServer = null)
    {
        var valores = new Dictionary<string, string?>
        {
            ["Persistencia:Proveedor"] = proveedor,
            ["ConnectionStrings:SqlServer"] = cadenaSqlServer
        };
        var configuracion = new ConfigurationBuilder().AddInMemoryCollection(valores).Build();
        return new ServiceCollection().AddInfraestructura(configuracion, Path.GetTempPath());
    }

    private static ServiceDescriptor Registro(IServiceCollection servicios, Type tipo) =>
        Assert.Single(servicios, descriptor => descriptor.ServiceType == tipo);

    // Modelo de diseño: conserva la colación, que el modelo de ejecución omite.
    private static IModel Modelo() =>
        new NominaContexto(new DbContextOptionsBuilder<NominaContexto>().UseSqlServer(CADENA_PRUEBA).Options)
            .GetService<IDesignTimeModel>().Model;

    [Theory]
    [InlineData(null)]
    [InlineData("Json")]
    public void SinProveedorOJsonRegistraRepositoriosJson(string? proveedor)
    {
        var servicios = Registrar(proveedor);

        Assert.Equal(typeof(JsonEmpleadoRepositorio), Registro(servicios, typeof(IEmpleadoRepositorio)).ImplementationType);
        Assert.Equal(typeof(JsonNominaRepositorio), Registro(servicios, typeof(INominaRepositorio)).ImplementationType);
        Assert.DoesNotContain(servicios, descriptor => descriptor.ServiceType == typeof(NominaContexto));
    }

    [Theory]
    [InlineData("SqlServer")]
    [InlineData("sqlserver")]
    public void SqlServerRegistraContextoYRepositoriosEfPorAlcance(string proveedor)
    {
        var servicios = Registrar(proveedor, CADENA_PRUEBA);

        Assert.Equal(ServiceLifetime.Scoped, Registro(servicios, typeof(NominaContexto)).Lifetime);
        Assert.All(_repositorios, tipo =>
        {
            var registro = Registro(servicios, tipo);
            Assert.Equal(ServiceLifetime.Scoped, registro.Lifetime);
            Assert.Equal(typeof(EfEmpleadoRepositorio).Namespace, registro.ImplementationType!.Namespace);
        });
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    public void SqlServerSinCadenaDeConexionFalla(string? cadena)
    {
        var excepcion = Assert.Throws<InvalidOperationException>(() => Registrar("SqlServer", cadena));

        Assert.Contains("ConnectionStrings:SqlServer", excepcion.Message);
    }

    [Fact]
    public void ProveedorDesconocidoFalla()
    {
        var excepcion = Assert.Throws<InvalidOperationException>(() => Registrar("Oracle"));

        Assert.Contains("Oracle", excepcion.Message);
    }

    [Fact]
    public void EmpleadosEnUnaTablaConDiscriminadorPorTipo()
    {
        var modelo = Modelo();

        var esperados = new Dictionary<Type, string>
        {
            [typeof(EmpleadoAsalariado)] = "Asalariado",
            [typeof(EmpleadoPorHoras)] = "PorHoras",
            [typeof(EmpleadoPorComision)] = "PorComision",
            [typeof(EmpleadoAsalariadoPorComision)] = "AsalariadoPorComision"
        };
        Assert.All(esperados, par =>
        {
            var tipo = modelo.FindEntityType(par.Key)!;
            Assert.Equal("Empleados", tipo.GetTableName());
            Assert.Equal(par.Value, tipo.GetDiscriminatorValue());
        });
        Assert.Equal("TipoEmpleado", modelo.FindEntityType(typeof(Empleado))!.FindDiscriminatorProperty()!.Name);
    }

    [Theory]
    [InlineData(typeof(EmpleadoAsalariado), nameof(EmpleadoAsalariado.SalarioSemanal), "decimal(18,2)")]
    [InlineData(typeof(EmpleadoPorHoras), nameof(EmpleadoPorHoras.SueldoPorHora), "decimal(18,2)")]
    [InlineData(typeof(EmpleadoPorHoras), nameof(EmpleadoPorHoras.HorasTrabajadas), "decimal(6,3)")]
    [InlineData(typeof(EmpleadoPorComision), nameof(EmpleadoPorComision.VentasBrutas), "decimal(18,2)")]
    [InlineData(typeof(EmpleadoPorComision), nameof(EmpleadoPorComision.TarifaComision), "decimal(7,6)")]
    [InlineData(typeof(EmpleadoAsalariadoPorComision), nameof(EmpleadoAsalariadoPorComision.SalarioBase), "decimal(18,2)")]
    [InlineData(typeof(Nomina), nameof(Nomina.MontoTotal), "decimal(18,2)")]
    public void ColumnasDecimalesCoincidenConLaValidacion(Type entidad, string propiedad, string tipoColumna)
    {
        Assert.Equal(tipoColumna, Modelo().FindEntityType(entidad)!.FindProperty(propiedad)!.GetColumnType());
    }

    [Theory]
    [InlineData(typeof(Empleado), nameof(Empleado.Cedula))]
    [InlineData(typeof(Departamento), nameof(Departamento.Nombre))]
    [InlineData(typeof(EntidadGubernamental), nameof(EntidadGubernamental.Nombre))]
    [InlineData(typeof(Usuario), nameof(Usuario.NombreUsuario))]
    [InlineData(typeof(Nomina), nameof(Nomina.FechaInicioSemana))]
    public void ValoresIrrepetiblesTienenIndiceUnico(Type entidad, string propiedad)
    {
        var tipo = Modelo().FindEntityType(entidad)!;

        var indice = Assert.Single(tipo.GetIndexes(), indice => indice.Properties.Select(p => p.Name).SequenceEqual([propiedad]));
        Assert.True(indice.IsUnique);
    }

    [Fact]
    public void ColacionSinMayusculasNiAcentos()
    {
        Assert.Equal("Latin1_General_100_CI_AI", Modelo().GetCollation());
    }

    [Fact]
    public void LineasDeDesgloseSeGuardanEnSuPropiaTabla()
    {
        var navegacion = Modelo().FindEntityType(typeof(DetalleNomina))!.FindNavigation(nameof(DetalleNomina.LineasDesglose))!;

        Assert.True(navegacion.ForeignKey.IsOwnership);
        Assert.Equal("LineasDesgloseNomina", navegacion.TargetEntityType.GetTableName());
    }
}
