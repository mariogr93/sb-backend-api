namespace SB.SistemaNomina.Aplicacion.Comun;

/// <summary>Mensajes de validación; {PropertyName} y {MaxLength} los reemplaza FluentValidation.</summary>
public static class MensajesValidacion
{
    public const string SOLICITUD_INVALIDA = "La solicitud contiene datos inválidos.";

    public const string CAMPO_REQUERIDO = "El campo {PropertyName} es requerido.";

    public const string LONGITUD_MAXIMA = "El campo {PropertyName} admite máximo {MaxLength} caracteres.";

    public const string NOMBRE_INVALIDO = "El campo {PropertyName} solo admite letras, espacios, apóstrofo, punto y guion.";

    public const string NUMERO_SEGURO_SOCIAL_INVALIDO = "El número de seguro social debe tener el formato ###-##-####.";

    public const string MONTO_FUERA_RANGO = "El campo {PropertyName} debe estar entre 0 y 99,999,999.99.";

    public const string HORAS_FUERA_RANGO = "Las horas trabajadas deben estar entre 0 y 168.";

    public const string TARIFA_COMISION_FUERA_RANGO = "La tarifa de comisión debe ser mayor que 0 y menor o igual a 1.";

    public const string TIPO_EMPLEADO_NO_SOPORTADO = "El tipo de empleado no es soportado.";

    public const string VALOR_NO_VALIDO = "El valor de {PropertyName} no es válido.";

    public const string NOMBRE_USUARIO_INVALIDO = "El nombre de usuario debe tener de 3 a 50 caracteres: letras, números, punto, guion o guion bajo.";

    public const string CONTRASENA_LONGITUD = "La contraseña debe tener entre 8 y 128 caracteres.";

    public const string CONTRASENA_COMPLEJIDAD = "La contraseña debe incluir mayúscula, minúscula y número.";

    public const string ROL_INVALIDO = "El rol debe ser Admin o Usuario.";

    public const string PAGINA_INVALIDA = "La página debe estar entre 1 y 1,000,000.";

    public const string TAMANO_PAGINA_INVALIDO = "El tamaño de página debe estar entre 1 y 100.";

    public const string FECHA_INICIO_SEMANA_NO_LUNES = "La fecha de inicio de semana debe ser lunes.";

    public const string FECHA_INICIO_SEMANA_FUTURA = "La fecha de inicio de semana no puede ser futura.";
}
