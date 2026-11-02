using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Modelos.Autenticacion;
using SB.SistemaNomina.Aplicacion.Modelos.Departamentos;
using SB.SistemaNomina.Aplicacion.Modelos.Empleados;
using SB.SistemaNomina.Aplicacion.Modelos.EntidadesGubernamentales;
using SB.SistemaNomina.Aplicacion.Validadores.Autenticacion;
using SB.SistemaNomina.Aplicacion.Validadores.Departamentos;
using SB.SistemaNomina.Aplicacion.Validadores.Empleados;
using SB.SistemaNomina.Aplicacion.Validadores.EntidadesGubernamentales;
using SB.SistemaNomina.Dominio.Enumeraciones;

namespace SB.SistemaNomina.PruebasUnitarias.Aplicacion;

/// <summary>Inicio de sesión, departamentos, entidades y filtros. Longitudes literales del requisito.</summary>
public class ValidadoresCatalogosPruebas
{
    private static string Texto(int longitud) => new('a', longitud);

    [Theory]
    [InlineData(100, 128, true)]
    [InlineData(101, 8, false)]
    [InlineData(5, 129, false)]
    [InlineData(5, 1, true)]
    public async Task InicioSesionLimitaLongitudSinExigirComplejidad(int longitudUsuario, int longitudContrasena, bool esValido)
    {
        var solicitud = new IniciarSesionSolicitud { NombreUsuario = Texto(longitudUsuario), Contrasena = Texto(longitudContrasena) };

        var resultado = await new IniciarSesionSolicitudValidador().ValidateAsync(solicitud);

        Assert.Equal(esValido, resultado.IsValid);
        if (!esValido)
        {
            Assert.Equal("MaximumLengthValidator", Assert.Single(resultado.Errors).ErrorCode);
        }
    }

    [Theory]
    [InlineData("", "x", nameof(IniciarSesionSolicitud.NombreUsuario))]
    [InlineData(null, "x", nameof(IniciarSesionSolicitud.NombreUsuario))]
    [InlineData("mgarcia", "", nameof(IniciarSesionSolicitud.Contrasena))]
    [InlineData("mgarcia", null, nameof(IniciarSesionSolicitud.Contrasena))]
    public async Task InicioSesionExigeUsuarioYContrasena(string? nombreUsuario, string? contrasena, string campo)
    {
        var solicitud = new IniciarSesionSolicitud { NombreUsuario = nombreUsuario!, Contrasena = contrasena! };

        var resultado = await new IniciarSesionSolicitudValidador().ValidateAsync(solicitud);

        var error = Assert.Single(resultado.Errors);
        Assert.Equal(campo, error.PropertyName);
        Assert.Equal("NotEmptyValidator", error.ErrorCode);
    }

    [Theory]
    [InlineData(100, null)]
    [InlineData(101, "MaximumLengthValidator")]
    [InlineData(0, "NotEmptyValidator")]
    public async Task NombreDepartamentoHastaCienCaracteres(int longitud, string? codigoError)
    {
        var resultado = await new GuardarDepartamentoSolicitudValidador()
            .ValidateAsync(new GuardarDepartamentoSolicitud { Nombre = Texto(longitud) });

        AfirmarResultado(resultado, nameof(GuardarDepartamentoSolicitud.Nombre), codigoError);
    }

    [Fact]
    public async Task NombreDepartamentoNuloEsRequerido()
    {
        var resultado = await new GuardarDepartamentoSolicitudValidador()
            .ValidateAsync(new GuardarDepartamentoSolicitud { Nombre = null! });

        AfirmarResultado(resultado, nameof(GuardarDepartamentoSolicitud.Nombre), "NotEmptyValidator");
    }

    private static GuardarEntidadGubernamentalSolicitud EntidadValida() => new()
    {
        Nombre = "Superintendencia de Bancos",
        Categoria = "Órgano autónomo",
        PoderDelEstado = "Ejecutivo",
        Sector = "Financiero"
    };

    [Theory]
    [InlineData(nameof(GuardarEntidadGubernamentalSolicitud.Nombre), 200, null)]
    [InlineData(nameof(GuardarEntidadGubernamentalSolicitud.Nombre), 201, "MaximumLengthValidator")]
    [InlineData(nameof(GuardarEntidadGubernamentalSolicitud.Nombre), 0, "NotEmptyValidator")]
    [InlineData(nameof(GuardarEntidadGubernamentalSolicitud.Categoria), 100, null)]
    [InlineData(nameof(GuardarEntidadGubernamentalSolicitud.Categoria), 101, "MaximumLengthValidator")]
    [InlineData(nameof(GuardarEntidadGubernamentalSolicitud.Categoria), 0, "NotEmptyValidator")]
    [InlineData(nameof(GuardarEntidadGubernamentalSolicitud.PoderDelEstado), 100, null)]
    [InlineData(nameof(GuardarEntidadGubernamentalSolicitud.PoderDelEstado), 101, "MaximumLengthValidator")]
    [InlineData(nameof(GuardarEntidadGubernamentalSolicitud.PoderDelEstado), 0, "NotEmptyValidator")]
    [InlineData(nameof(GuardarEntidadGubernamentalSolicitud.Sector), 100, null)]
    [InlineData(nameof(GuardarEntidadGubernamentalSolicitud.Sector), 101, "MaximumLengthValidator")]
    [InlineData(nameof(GuardarEntidadGubernamentalSolicitud.Sector), 0, "NotEmptyValidator")]
    public async Task CamposDeEntidadRespetanLongitudYSonRequeridos(string campo, int longitud, string? codigoError)
    {
        var texto = Texto(longitud);
        var solicitud = campo switch
        {
            nameof(GuardarEntidadGubernamentalSolicitud.Nombre) => EntidadValida() with { Nombre = texto },
            nameof(GuardarEntidadGubernamentalSolicitud.Categoria) => EntidadValida() with { Categoria = texto },
            nameof(GuardarEntidadGubernamentalSolicitud.PoderDelEstado) => EntidadValida() with { PoderDelEstado = texto },
            _ => EntidadValida() with { Sector = texto }
        };

        var resultado = await new GuardarEntidadGubernamentalSolicitudValidador().ValidateAsync(solicitud);

        AfirmarResultado(resultado, campo, codigoError);
    }

    [Theory]
    [InlineData(100, null, null, null)]
    [InlineData(101, null, null, nameof(FiltroEmpleados.Nombre))]
    [InlineData(0, EstadoEmpleado.Inactivo, TipoEmpleado.PorHoras, null)]
    [InlineData(0, (EstadoEmpleado)99, null, nameof(FiltroEmpleados.Estado))]
    [InlineData(0, null, (TipoEmpleado)99, nameof(FiltroEmpleados.Tipo))]
    public async Task FiltroEmpleadosValidaTextoYEnumeraciones(int longitudNombre, EstadoEmpleado? estado, TipoEmpleado? tipo, string? campoInvalido)
    {
        var filtro = new FiltroEmpleados { Nombre = longitudNombre == 0 ? null : Texto(longitudNombre), Estado = estado, Tipo = tipo };

        var resultado = await new FiltroEmpleadosValidador().ValidateAsync(filtro);

        AfirmarCampoInvalido(resultado, campoInvalido);
    }

    [Theory]
    [InlineData(0, 20, nameof(FiltroEmpleados.Pagina))]
    [InlineData(1, 101, nameof(FiltroEmpleados.TamanoPagina))]
    public async Task FiltrosHeredanReglasDePaginacion(int pagina, int tamanoPagina, string campo)
    {
        var resultadoEmpleados = await new FiltroEmpleadosValidador()
            .ValidateAsync(new FiltroEmpleados { Pagina = pagina, TamanoPagina = tamanoPagina });
        var resultadoEntidades = await new FiltroEntidadesGubernamentalesValidador()
            .ValidateAsync(new FiltroEntidadesGubernamentales { Pagina = pagina, TamanoPagina = tamanoPagina });

        AfirmarCampoInvalido(resultadoEmpleados, campo);
        AfirmarCampoInvalido(resultadoEntidades, campo);
    }

    [Theory]
    [InlineData(nameof(FiltroEntidadesGubernamentales.Nombre), 100, false)]
    [InlineData(nameof(FiltroEntidadesGubernamentales.Nombre), 101, true)]
    [InlineData(nameof(FiltroEntidadesGubernamentales.Sector), 100, false)]
    [InlineData(nameof(FiltroEntidadesGubernamentales.Sector), 101, true)]
    [InlineData(nameof(FiltroEntidadesGubernamentales.PoderDelEstado), 100, false)]
    [InlineData(nameof(FiltroEntidadesGubernamentales.PoderDelEstado), 101, true)]
    public async Task FiltroEntidadesLimitaCadaTexto(string campo, int longitud, bool esInvalido)
    {
        var texto = Texto(longitud);
        var filtro = campo switch
        {
            nameof(FiltroEntidadesGubernamentales.Nombre) => new FiltroEntidadesGubernamentales { Nombre = texto },
            nameof(FiltroEntidadesGubernamentales.Sector) => new FiltroEntidadesGubernamentales { Sector = texto },
            _ => new FiltroEntidadesGubernamentales { PoderDelEstado = texto }
        };

        var resultado = await new FiltroEntidadesGubernamentalesValidador().ValidateAsync(filtro);

        AfirmarCampoInvalido(resultado, esInvalido ? campo : null);
    }

    private static void AfirmarResultado(FluentValidation.Results.ValidationResult resultado, string campo, string? codigoError)
    {
        if (codigoError is null)
        {
            Assert.True(resultado.IsValid);
            return;
        }

        var error = Assert.Single(resultado.Errors);
        Assert.Equal(campo, error.PropertyName);
        Assert.Equal(codigoError, error.ErrorCode);
    }

    private static void AfirmarCampoInvalido(FluentValidation.Results.ValidationResult resultado, string? campo)
    {
        if (campo is null)
        {
            Assert.True(resultado.IsValid);
            return;
        }

        var error = Assert.Single(resultado.Errors);
        Assert.Equal(campo, error.PropertyName);
        if (campo is nameof(FiltroEmpleados.Estado) or nameof(FiltroEmpleados.Tipo))
        {
            Assert.Equal(MensajesValidacion.VALOR_NO_VALIDO.Replace("{PropertyName}", campo), error.ErrorMessage);
        }
    }
}
