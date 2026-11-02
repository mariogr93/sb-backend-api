namespace SB.SistemaNomina.Dominio.Constantes;

public static class NombresRoles
{
    public const string ADMINISTRADOR = "Admin";

    public const string USUARIO = "Usuario";

    public static readonly IReadOnlyCollection<string> TODOS = [ADMINISTRADOR, USUARIO];
}
