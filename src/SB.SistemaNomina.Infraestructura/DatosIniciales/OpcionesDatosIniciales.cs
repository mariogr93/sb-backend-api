namespace SB.SistemaNomina.Infraestructura.DatosIniciales;

/// <summary>Sección "DatosIniciales". Contraseñas solo en appsettings.Development.json o variables de entorno.</summary>
public sealed class OpcionesDatosIniciales
{
    public const string SECCION = "DatosIniciales";

    public string NombreUsuarioAdministrador { get; init; } = "admin";

    /// <summary>Requerida si no existe un administrador activo.</summary>
    public string? ContrasenaAdministrador { get; init; }

    /// <summary>Usuario de solo lectura; se crea si hay contraseña configurada.</summary>
    public string NombreUsuarioConsulta { get; init; } = "consulta";

    public string? ContrasenaUsuarioConsulta { get; init; }

    /// <summary>Carga empleados-demostracion.json. Opcional: --DatosIniciales:IncluirEmpleadosDemostracion=true.</summary>
    public bool IncluirEmpleadosDemostracion { get; init; }
}
