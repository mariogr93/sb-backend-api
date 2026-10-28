using SB.SistemaNomina.Dominio.Enumeraciones;

namespace SB.SistemaNomina.Aplicacion.Modelos.Nominas;

public sealed record TotalTipoEmpleadoRespuesta(TipoEmpleado TipoEmpleado, int CantidadEmpleados, decimal MontoTotal);
