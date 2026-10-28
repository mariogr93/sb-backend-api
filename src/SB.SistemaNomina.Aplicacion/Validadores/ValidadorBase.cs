using FluentValidation;

namespace SB.SistemaNomina.Aplicacion.Validadores;

/// <summary>Detiene cada regla en su primer error: un mensaje por campo.</summary>
public abstract class ValidadorBase<T> : AbstractValidator<T>
{
    protected ValidadorBase()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
    }
}
