using SB.SistemaNomina.Aplicacion.Modelos.Comun;
using SB.SistemaNomina.Dominio.Enumeraciones;

namespace SB.SistemaNomina.Aplicacion.Modelos.Nominas;

public sealed record PagoEmpleadoRespuesta(
    Guid EmpleadoId,
    string NombreCompleto,
    TipoEmpleado TipoEmpleado,
    string NombreDepartamento,
    DesglosePagoRespuesta Desglose);
