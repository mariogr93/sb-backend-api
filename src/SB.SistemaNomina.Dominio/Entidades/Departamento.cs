namespace SB.SistemaNomina.Dominio.Entidades;

public class Departamento
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Nombre { get; set; } = string.Empty;

    public bool EstaActivo { get; set; } = true;
}
