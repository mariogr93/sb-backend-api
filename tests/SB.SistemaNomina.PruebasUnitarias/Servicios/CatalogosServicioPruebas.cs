using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SB.SistemaNomina.Aplicacion.Excepciones;
using SB.SistemaNomina.Aplicacion.Interfaces.Repositorios;
using SB.SistemaNomina.Aplicacion.Interfaces.Servicios;
using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.PruebasUnitarias.Comun;
using SB.SistemaNomina.Servicios.Comun;

namespace SB.SistemaNomina.PruebasUnitarias.Servicios;

/// <summary>Departamentos y entidades gubernamentales: desactivar y reactivar con servicios reales y repositorios dobles.</summary>
public sealed class CatalogosServicioPruebas
{
    private static readonly DateTimeOffset _ahora = new(2026, 11, 2, 15, 30, 0, TimeSpan.Zero);

    private readonly IDepartamentoRepositorio _departamentoRepositorio = Substitute.For<IDepartamentoRepositorio>();
    private readonly IEmpleadoRepositorio _empleadoRepositorio = Substitute.For<IEmpleadoRepositorio>();
    private readonly IEntidadGubernamentalRepositorio _entidadRepositorio = Substitute.For<IEntidadGubernamentalRepositorio>();
    private readonly IDepartamentoServicio _departamentos;
    private readonly IEntidadGubernamentalServicio _entidades;

    public CatalogosServicioPruebas()
    {
        var proveedor = ContenedorPruebas.Crear(_ahora, servicios => servicios
            .AddSingleton(_departamentoRepositorio)
            .AddSingleton(_empleadoRepositorio)
            .AddSingleton(_entidadRepositorio));
        _departamentos = proveedor.GetRequiredService<IDepartamentoServicio>();
        _entidades = proveedor.GetRequiredService<IEntidadGubernamentalServicio>();
    }

    private Departamento Departamento(bool estaActivo)
    {
        var departamento = new Departamento { Nombre = "Finanzas", EstaActivo = estaActivo };
        _departamentoRepositorio.ObtenerPorIdAsync(departamento.Id, Arg.Any<CancellationToken>()).Returns(departamento);
        return departamento;
    }

    private EntidadGubernamental Entidad(bool estaActiva)
    {
        var entidad = new EntidadGubernamental { Nombre = "Ministerio de Prueba", EstaActiva = estaActiva };
        _entidadRepositorio.ObtenerPorIdAsync(entidad.Id, Arg.Any<CancellationToken>()).Returns(entidad);
        return entidad;
    }

    [Fact]
    public async Task ActivarDepartamentoInactivoLoDejaActivoYGuarda()
    {
        var departamento = Departamento(estaActivo: false);

        await _departamentos.ActivarAsync(departamento.Id, CancellationToken.None);

        Assert.True(departamento.EstaActivo);
        await _departamentoRepositorio.Received(1).ActualizarAsync(departamento, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ActivarDepartamentoInexistenteLanzaNoEncontradoYNoGuarda()
    {
        await Assert.ThrowsAsync<RecursoNoEncontradoException>(() => _departamentos.ActivarAsync(Guid.NewGuid(), CancellationToken.None));

        await _departamentoRepositorio.DidNotReceiveWithAnyArgs().ActualizarAsync(default!, default);
    }

    [Fact]
    public async Task DesactivarDepartamentoSinEmpleadosActivosLoDesactiva()
    {
        var departamento = Departamento(estaActivo: true);
        _empleadoRepositorio.ListarActivosAsync(departamento.Id, Arg.Any<CancellationToken>()).Returns([]);

        await _departamentos.DesactivarAsync(departamento.Id, CancellationToken.None);

        Assert.False(departamento.EstaActivo);
        await _departamentoRepositorio.Received(1).ActualizarAsync(departamento, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DesactivarDepartamentoConEmpleadosActivosLanzaReglaDeNegocioYNoGuarda()
    {
        var departamento = Departamento(estaActivo: true);
        _empleadoRepositorio.ListarActivosAsync(departamento.Id, Arg.Any<CancellationToken>()).Returns([new EmpleadoAsalariado()]);

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(() => _departamentos.DesactivarAsync(departamento.Id, CancellationToken.None));

        Assert.Equal(MensajesServicio.DEPARTAMENTO_CON_EMPLEADOS_ACTIVOS, excepcion.Message);
        Assert.True(departamento.EstaActivo);
        await _departamentoRepositorio.DidNotReceiveWithAnyArgs().ActualizarAsync(default!, default);
    }

    [Fact]
    public async Task ActivarEntidadInactivaLaDejaActivaYGuarda()
    {
        var entidad = Entidad(estaActiva: false);

        await _entidades.ActivarAsync(entidad.Id, CancellationToken.None);

        Assert.True(entidad.EstaActiva);
        await _entidadRepositorio.Received(1).ActualizarAsync(entidad, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ActivarEntidadInexistenteLanzaNoEncontradoYNoGuarda()
    {
        await Assert.ThrowsAsync<RecursoNoEncontradoException>(() => _entidades.ActivarAsync(Guid.NewGuid(), CancellationToken.None));

        await _entidadRepositorio.DidNotReceiveWithAnyArgs().ActualizarAsync(default!, default);
    }

    [Fact]
    public async Task DesactivarEntidadActivaLaDejaInactivaYGuarda()
    {
        var entidad = Entidad(estaActiva: true);

        await _entidades.DesactivarAsync(entidad.Id, CancellationToken.None);

        Assert.False(entidad.EstaActiva);
        await _entidadRepositorio.Received(1).ActualizarAsync(entidad, Arg.Any<CancellationToken>());
    }
}
