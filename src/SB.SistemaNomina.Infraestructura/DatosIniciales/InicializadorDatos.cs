using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SB.SistemaNomina.Aplicacion.Comun;
using Microsoft.Extensions.Options;
using SB.SistemaNomina.Aplicacion.Interfaces.Repositorios;
using SB.SistemaNomina.Aplicacion.Interfaces.Seguridad;
using SB.SistemaNomina.Aplicacion.Modelos.Empleados;
using SB.SistemaNomina.Aplicacion.Modelos.EntidadesGubernamentales;
using SB.SistemaNomina.Dominio.Constantes;
using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.Infraestructura.Persistencia.Json;

namespace SB.SistemaNomina.Infraestructura.DatosIniciales;

/// <summary>Carga datos iniciales al arrancar. Idempotente: cada catálogo solo se llena si está vacío.</summary>
public sealed class InicializadorDatos
{
    private const string ARCHIVO_DEPARTAMENTOS = "departamentos.json";
    private const string ARCHIVO_ENTIDADES = "entidades-gubernamentales.json";
    private const string NOMBRE_COMPLETO_ADMINISTRADOR = "Administrador del sistema";
    private const string NOMBRE_COMPLETO_CONSULTA = "Usuario de consulta";
    private const string MENSAJE_CONTRASENA_ADMINISTRADOR =
        "Configure DatosIniciales:ContrasenaAdministrador (8-128 caracteres con mayúscula, minúscula y número) para crear el primer administrador.";

    private const string ARCHIVO_EMPLEADOS_DEMOSTRACION = "empleados-demostracion.json";

    private static readonly JsonSerializerOptions _opcionesLectura = new() { PropertyNameCaseInsensitive = true };

    // Mismo formato polimórfico del almacén ("tipo" + campos de la subclase).
    private static readonly JsonSerializerOptions _opcionesEmpleados = OpcionesSerializacionJson.Crear();

    private readonly IDepartamentoRepositorio _departamentoRepositorio;
    private readonly IEntidadGubernamentalRepositorio _entidadRepositorio;
    private readonly IUsuarioRepositorio _usuarioRepositorio;
    private readonly IEmpleadoRepositorio _empleadoRepositorio;
    private readonly IServicioHashContrasena _servicioHash;
    private readonly OpcionesDatosIniciales _opciones;
    private readonly TimeProvider _proveedorTiempo;
    private readonly ILogger<InicializadorDatos> _registro;

    public InicializadorDatos(
        IDepartamentoRepositorio departamentoRepositorio,
        IEntidadGubernamentalRepositorio entidadRepositorio,
        IUsuarioRepositorio usuarioRepositorio,
        IEmpleadoRepositorio empleadoRepositorio,
        IServicioHashContrasena servicioHash,
        IOptions<OpcionesDatosIniciales> opciones,
        TimeProvider proveedorTiempo,
        ILogger<InicializadorDatos> registro)
    {
        _departamentoRepositorio = departamentoRepositorio;
        _entidadRepositorio = entidadRepositorio;
        _usuarioRepositorio = usuarioRepositorio;
        _empleadoRepositorio = empleadoRepositorio;
        _servicioHash = servicioHash;
        _opciones = opciones.Value;
        _proveedorTiempo = proveedorTiempo;
        _registro = registro;
    }

    public async Task InicializarAsync(CancellationToken tokenCancelacion)
    {
        await SembrarDepartamentosAsync(tokenCancelacion);
        await SembrarEntidadesAsync(tokenCancelacion);
        await SembrarAdministradorAsync(tokenCancelacion);
        await SembrarUsuarioConsultaAsync(tokenCancelacion);

        if (_opciones.IncluirEmpleadosDemostracion)
        {
            await SembrarEmpleadosDemostracionAsync(tokenCancelacion);
        }
    }

    private async Task SembrarDepartamentosAsync(CancellationToken tokenCancelacion)
    {
        if ((await _departamentoRepositorio.ListarAsync(incluirInactivos: true, tokenCancelacion)).Count > 0)
        {
            return;
        }

        var nombres = await LeerArchivoAsync<List<string>>(ARCHIVO_DEPARTAMENTOS, tokenCancelacion);
        await _departamentoRepositorio.AgregarVariosAsync(
            nombres.Select(nombre => new Departamento { Nombre = nombre }).ToList(),
            tokenCancelacion);

        _registro.LogInformation("Datos iniciales: {Cantidad} departamentos creados.", nombres.Count);
    }

    private async Task SembrarEntidadesAsync(CancellationToken tokenCancelacion)
    {
        var existentes = await _entidadRepositorio.ListarAsync(
            new FiltroEntidadesGubernamentales { TamanoPagina = 1, IncluirInactivas = true },
            tokenCancelacion);
        if (existentes.TotalElementos > 0)
        {
            return;
        }

        var entidades = await LeerArchivoAsync<List<EntidadSemilla>>(ARCHIVO_ENTIDADES, tokenCancelacion);
        await _entidadRepositorio.AgregarVariosAsync(
            entidades.Select(entidad => new EntidadGubernamental
            {
                Nombre = entidad.Nombre,
                Categoria = entidad.Categoria,
                PoderDelEstado = entidad.PoderDelEstado,
                Sector = entidad.Sector
            }).ToList(),
            tokenCancelacion);

        _registro.LogInformation("Datos iniciales: {Cantidad} entidades gubernamentales creadas.", entidades.Count);
    }

    private async Task SembrarAdministradorAsync(CancellationToken tokenCancelacion)
    {
        if (await _usuarioRepositorio.ContarAdministradoresActivosAsync(tokenCancelacion) > 0)
        {
            return;
        }

        var contrasena = _opciones.ContrasenaAdministrador;
        if (!CumplePoliticaContrasena(contrasena))
        {
            throw new InvalidOperationException(MENSAJE_CONTRASENA_ADMINISTRADOR);
        }

        await CrearUsuarioSiNoExisteAsync(
            _opciones.NombreUsuarioAdministrador, NOMBRE_COMPLETO_ADMINISTRADOR, contrasena, NombresRoles.ADMINISTRADOR, tokenCancelacion);
    }

    private async Task SembrarUsuarioConsultaAsync(CancellationToken tokenCancelacion)
    {
        if (string.IsNullOrWhiteSpace(_opciones.ContrasenaUsuarioConsulta))
        {
            return;
        }

        await CrearUsuarioSiNoExisteAsync(
            _opciones.NombreUsuarioConsulta, NOMBRE_COMPLETO_CONSULTA, _opciones.ContrasenaUsuarioConsulta, NombresRoles.USUARIO, tokenCancelacion);
    }

    private async Task CrearUsuarioSiNoExisteAsync(
        string nombreUsuario,
        string nombreCompleto,
        string contrasena,
        string rol,
        CancellationToken tokenCancelacion)
    {
        if (await _usuarioRepositorio.ExisteNombreUsuarioAsync(nombreUsuario, tokenCancelacion))
        {
            _registro.LogDebug("Datos iniciales: el usuario {NombreUsuario} ya existe; no se modifica.", nombreUsuario);
            return;
        }

        var usuario = new Usuario
        {
            NombreUsuario = nombreUsuario,
            NombreCompleto = nombreCompleto,
            Rol = rol,
            FechaCreacion = _proveedorTiempo.GetUtcNow().UtcDateTime
        };
        usuario.HashContrasena = _servicioHash.GenerarHash(usuario, contrasena);
        await _usuarioRepositorio.AgregarAsync(usuario, tokenCancelacion);

        _registro.LogInformation("Datos iniciales: usuario {NombreUsuario} creado con rol {Rol}.", nombreUsuario, rol);
    }

    private async Task SembrarEmpleadosDemostracionAsync(CancellationToken tokenCancelacion)
    {
        var existentes = await _empleadoRepositorio.ListarAsync(new FiltroEmpleados { TamanoPagina = 1 }, tokenCancelacion);
        if (existentes.TotalElementos > 0)
        {
            return;
        }

        var departamentos = await _departamentoRepositorio.ListarAsync(incluirInactivos: true, tokenCancelacion);
        var idsPorNombre = departamentos.ToDictionary(departamento => departamento.Nombre, departamento => departamento.Id, StringComparer.OrdinalIgnoreCase);
        var semillas = await LeerArchivoAsync<List<EmpleadoSemilla>>(ARCHIVO_EMPLEADOS_DEMOSTRACION, tokenCancelacion, _opcionesEmpleados);
        var fechaCreacion = _proveedorTiempo.GetUtcNow().UtcDateTime;
        var empleados = new List<Empleado>(semillas.Count);

        foreach (var semilla in semillas)
        {
            if (!idsPorNombre.TryGetValue(semilla.Departamento, out var departamentoId))
            {
                _registro.LogWarning("Datos iniciales: departamento {Departamento} no existe; empleado omitido.", semilla.Departamento);
                continue;
            }

            var empleado = semilla.Empleado;
            empleado.Id = Guid.NewGuid();
            empleado.DepartamentoId = departamentoId;
            empleado.FechaCreacion = fechaCreacion;
            empleado.FechaActualizacion = empleado.EstaActivo ? null : fechaCreacion;
            empleados.Add(empleado);
        }

        await _empleadoRepositorio.AgregarVariosAsync(empleados, tokenCancelacion);
        _registro.LogInformation("Datos iniciales: {Cantidad} empleados de demostración creados.", empleados.Count);
    }

    /// <summary>Misma política que la API (ReglasContrasena).</summary>
    private static bool CumplePoliticaContrasena([NotNullWhen(true)] string? contrasena) =>
        contrasena is { Length: >= ConstantesValidacion.LONGITUD_MINIMA_CONTRASENA and <= ConstantesValidacion.LONGITUD_MAXIMA_CONTRASENA }
        && Regex.IsMatch(contrasena, ConstantesValidacion.PATRON_MAYUSCULA)
        && Regex.IsMatch(contrasena, ConstantesValidacion.PATRON_MINUSCULA)
        && Regex.IsMatch(contrasena, ConstantesValidacion.PATRON_DIGITO);

    private static async Task<T> LeerArchivoAsync<T>(
        string nombreArchivo,
        CancellationToken tokenCancelacion,
        JsonSerializerOptions? opciones = null)
    {
        await using var flujo = typeof(InicializadorDatos).Assembly.GetManifestResourceStream(nombreArchivo)
            ?? throw new InvalidOperationException($"Recurso de datos iniciales no encontrado: {nombreArchivo}.");
        return await JsonSerializer.DeserializeAsync<T>(flujo, opciones ?? _opcionesLectura, tokenCancelacion)
            ?? throw new InvalidOperationException($"Recurso de datos iniciales vacío: {nombreArchivo}.");
    }

    private sealed record EntidadSemilla(string Nombre, string Categoria, string PoderDelEstado, string Sector);

    /// <summary>Departamento por nombre: el archivo no depende de Ids generados.</summary>
    private sealed record EmpleadoSemilla(string Departamento, Empleado Empleado);
}
