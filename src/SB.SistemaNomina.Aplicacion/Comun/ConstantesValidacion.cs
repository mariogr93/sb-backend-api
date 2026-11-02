namespace SB.SistemaNomina.Aplicacion.Comun;

public static class ConstantesValidacion
{
    public const int LONGITUD_MAXIMA_NOMBRE_PERSONA = 100;

    public const int LONGITUD_MAXIMA_NOMBRE_DEPARTAMENTO = 100;

    public const int LONGITUD_MAXIMA_NOMBRE_ENTIDAD = 200;

    public const int LONGITUD_MAXIMA_CLASIFICACION_ENTIDAD = 100;

    public const int LONGITUD_MAXIMA_FILTRO_TEXTO = 100;

    public const int LONGITUD_MINIMA_CONTRASENA = 8;

    public const int LONGITUD_MAXIMA_CONTRASENA = 128;

    public const decimal HORAS_MAXIMAS_SEMANA = 168m;

    /// <summary>Tope de montos; evita desbordar decimal en el cálculo de pago.</summary>
    public const decimal MONTO_MAXIMO = 99_999_999.99m;

    public const decimal TARIFA_COMISION_MAXIMA = 1m;

    // Dígitos y decimales admitidos; coinciden con las columnas de SQL Server (sin redondeo silencioso).
    public const int PRECISION_MONTO = 18;

    public const int DECIMALES_MONTO = 2;

    public const int PRECISION_HORAS = 6;

    public const int DECIMALES_HORAS = 3;

    public const int PRECISION_TARIFA = 7;

    public const int DECIMALES_TARIFA = 6;

    /// <summary>Cédula dominicana ###-#######-#.</summary>
    public const string PATRON_CEDULA = @"^\d{3}-\d{7}-\d$";

    /// <summary>Letras (con acentos), espacios, apóstrofo, punto y guion.</summary>
    public const string PATRON_NOMBRE_PERSONA = @"^\p{L}[\p{L} '.-]*$";

    /// <summary>3 a 50 caracteres: letras, números, punto, guion y guion bajo.</summary>
    public const string PATRON_NOMBRE_USUARIO = @"^[a-zA-Z0-9._-]{3,50}$";

    public const string PATRON_MAYUSCULA = "[A-Z]";

    public const string PATRON_MINUSCULA = "[a-z]";

    public const string PATRON_DIGITO = "[0-9]";
}
