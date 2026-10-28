using SB.SistemaNomina.Aplicacion.Modelos.Comun;
using SB.SistemaNomina.Dominio.Enumeraciones;

namespace SB.SistemaNomina.Aplicacion.Modelos.Empleados;

public sealed record EmpleadoRespuesta(
    Guid Id,
    string PrimerNombre,
    string ApellidoPaterno,
    string NombreCompleto,
    string NumeroSeguroSocial,
    Guid DepartamentoId,
    string NombreDepartamento,
    TipoEmpleado Tipo,
    EstadoEmpleado Estado,
    CompensacionEmpleado Compensacion,
    DesglosePagoRespuesta PagoSemanal,
    DateTime FechaCreacion,
    DateTime? FechaActualizacion);
