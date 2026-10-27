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
