namespace SB.SistemaNomina.Servicios.Comun;

public static class MensajesServicio
{
    public const string NUMERO_SEGURO_SOCIAL_DUPLICADO = "Ya existe un empleado con ese número de seguro social.";

    public const string DEPARTAMENTO_NO_DISPONIBLE = "El departamento no existe o está inactivo.";

    public const string DEPARTAMENTO_DUPLICADO = "Ya existe un departamento con ese nombre.";

    public const string DEPARTAMENTO_CON_EMPLEADOS_ACTIVOS = "No se puede desactivar un departamento con empleados activos.";

    public const string ENTIDAD_DUPLICADA = "Ya existe una entidad gubernamental con ese nombre.";

    public const string NOMBRE_USUARIO_DUPLICADO = "Ya existe un usuario con ese nombre de usuario.";

    public const string NO_PUEDE_MODIFICARSE_A_SI_MISMO = "No puede quitarse el rol de administrador ni desactivarse a sí mismo.";

    public const string ULTIMO_ADMINISTRADOR = "Debe quedar al menos un administrador activo.";

    public const string NOMINA_SEMANA_DUPLICADA = "Ya existe una nómina guardada para esa semana.";

    public const string TIPO_EMPLEADO_NO_SOPORTADO = "El tipo de empleado no es soportado.";

    public const string TIPO_EMPLEADO_NO_COINCIDE = "El empleado no corresponde al tipo del creador.";
}
