using FluentValidation;
using SB.SistemaNomina.Aplicacion.Modelos.Empleados;
using SB.SistemaNomina.Dominio.Enumeraciones;

namespace SB.SistemaNomina.Aplicacion.Validadores.Empleados;

/// <summary>Reglas de compensación de un tipo de empleado. Tipo nuevo = clase nueva; se registra sola por escaneo.</summary>
public interface IValidadorCompensacionEmpleado : IValidator<GuardarEmpleadoSolicitud>
{
    TipoEmpleado Tipo { get; }
}
