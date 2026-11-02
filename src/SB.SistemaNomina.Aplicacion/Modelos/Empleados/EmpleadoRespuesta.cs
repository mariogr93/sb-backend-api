using SB.SistemaNomina.Aplicacion.Modelos.Comun;
using SB.SistemaNomina.Dominio.Enumeraciones;

namespace SB.SistemaNomina.Aplicacion.Modelos.Empleados;

/// <summary>Cedula completa para Admin; enmascarada (***-****567-1) para Usuario.</summary>
public sealed record EmpleadoRespuesta(
    Guid Id,
    string PrimerNombre,
    string ApellidoPaterno,
    string NombreCompleto,
    string Cedula,
    Guid DepartamentoId,
    string NombreDepartamento,
    TipoEmpleado Tipo,
    EstadoEmpleado Estado,
    CompensacionEmpleado Compensacion,
    DesglosePagoRespuesta PagoSemanal,
    DateTime FechaCreacion,
    DateTime? FechaActualizacion);
