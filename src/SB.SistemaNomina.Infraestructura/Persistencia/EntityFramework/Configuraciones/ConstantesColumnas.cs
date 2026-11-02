namespace SB.SistemaNomina.Infraestructura.Persistencia.EntityFramework.Configuraciones;

/// <summary>Tipos y longitudes de columna; deben coincidir con los límites de validación.</summary>
public static class ConstantesColumnas
{
    /// <summary>Columna discriminadora de la tabla de empleados (TPH).</summary>
    public const string DISCRIMINADOR_EMPLEADO = "TipoEmpleado";

    public const string TIPO_MONTO = "decimal(18,2)";

    public const string TIPO_HORAS = "decimal(6,3)";

    public const string TIPO_TARIFA = "decimal(7,6)";

    public const int LONGITUD_NOMBRE_PERSONA = 100;

    public const int LONGITUD_NOMBRE_COMPLETO = 201;

    public const int LONGITUD_CEDULA = 13;

    public const int LONGITUD_ENUMERACION = 30;

    public const int LONGITUD_NOMBRE_ENTIDAD = 200;

    public const int LONGITUD_CLASIFICACION_ENTIDAD = 100;

    public const int LONGITUD_NOMBRE_USUARIO = 50;

    public const int LONGITUD_HASH_CONTRASENA = 500;

    public const int LONGITUD_CONCEPTO = 100;

    public const int LONGITUD_FORMULA = 200;
}
