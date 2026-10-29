# SB.SistemaNomina — API

API RESTful para la gestión de empleados, cálculo de pagos semanales, reportes de nómina y mantenimiento de entidades gubernamentales de la República Dominicana.

> 🚧 En construcción. Este README se completa en la fase final (instrucciones de ejecución, base de datos, pruebas y supuestos).

## Tecnologías

- C# / .NET 8 / ASP.NET Core Web API
- Arquitectura Onion: Dominio, Aplicación, Servicios, Infraestructura
- Autenticación JWT (roles `Admin` y `Usuario`)
- Serilog, Swagger
- Base de datos: archivos de texto plano (JSON) dentro del proyecto; proveedor alterno EF Core + SQL Server

## Estructura

```
src/
├── SB.SistemaNomina.Dominio          Entidades y reglas de negocio
├── SB.SistemaNomina.Aplicacion       Contratos, modelos y validaciones
├── SB.SistemaNomina.Servicios        Implementación de casos de uso
├── SB.SistemaNomina.Infraestructura  Persistencia y seguridad
└── SB.SistemaNomina.API              Controladores y configuración
tests/
└── SB.SistemaNomina.PruebasUnitarias
```

## Requisitos

- .NET SDK 8.0

## Datos iniciales

Al iniciar por primera vez, la API crea en `src/SB.SistemaNomina.API/BaseDatos/`:

- Los 6 departamentos y las 181 entidades gubernamentales del listado oficial (`ListaEntidadesGubernamentales.xlsx`).
- El usuario administrador `admin` y, en desarrollo, el usuario de consulta `consulta` (contraseñas en `appsettings.Development.json`).

### Empleados de demostración (opcional)

Incluimos 240 empleados de ejemplo para explorar la aplicación sin cargar datos a mano: 10 por cada tipo de empleado en cada departamento, con salarios que varían según el departamento y el nivel de experiencia. Algunos están inactivos para probar los filtros y el reporte de nómina.

No se cargan por defecto. Para incluirlos, ejecute la API una vez con:

```bash
dotnet run --project src/SB.SistemaNomina.API -- --DatosIniciales:IncluirEmpleadosDemostracion=true
```

Solo se cargan si aún no existen empleados. Para volver a empezar desde cero, elimine la carpeta `src/SB.SistemaNomina.API/BaseDatos/` y ejecute de nuevo.

## Supuestos de diseño

- Nuevo tipo de empleado = nueva subclase de `Empleado` + su creador + un valor nuevo en `TipoEmpleado`. Solo se agrega código; la lógica existente no cambia.
