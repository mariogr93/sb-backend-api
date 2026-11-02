# SB.SistemaNomina — API

API RESTful para la gestión de empleados, cálculo de pagos semanales, reportes de nómina y mantenimiento de entidades gubernamentales de la República Dominicana.

## Tecnologías

- C# / .NET 8 / ASP.NET Core Web API
- Arquitectura Onion: Dominio, Aplicación, Servicios, Infraestructura
- Autenticación JWT con roles `Admin` y `Usuario`
- FluentValidation, Serilog, Swagger
- Persistencia intercambiable: archivos JSON (predeterminado) o EF Core 8 + SQL Server
- Pruebas: xUnit + NSubstitute

## Estructura

```
src/
├── SB.SistemaNomina.Dominio          Entidades, tipos de empleado y cálculo de pago
├── SB.SistemaNomina.Aplicacion       Contratos, modelos, validaciones y excepciones
├── SB.SistemaNomina.Servicios        Casos de uso (empleados, nómina, usuarios, autenticación)
├── SB.SistemaNomina.Infraestructura  Persistencia JSON y SQL Server, hash de contraseñas, datos iniciales
└── SB.SistemaNomina.API              Controladores, middleware, seguridad y configuración
tests/
└── SB.SistemaNomina.PruebasUnitarias
base-datos/scripts/                   Scripts SQL de creación y datos iniciales
docker-compose.yml                    SQL Server para desarrollo
```

Dependencias: `Dominio` ← `Aplicacion` ← `Servicios` / `Infraestructura` ← `API`. El dominio no depende de ningún framework.

## Requisitos

- .NET SDK 8.0
- Opcional, para SQL Server: Docker Desktop

## Ejecución rápida (archivos JSON)

```bash
dotnet run --project src/SB.SistemaNomina.API --launch-profile http
```

- Swagger: http://localhost:5297/swagger (solo en Development).
- Usuarios iniciales: `admin` (rol Admin) y `consulta` (rol Usuario). Las contraseñas de desarrollo están en `src/SB.SistemaNomina.API/appsettings.Development.json`.
- En Swagger: `POST /api/autenticacion/iniciar-sesion`, copie el `token` y péguelo en **Authorize**.
- `src/SB.SistemaNomina.API/SB.SistemaNomina.API.http` contiene 23 solicitudes de ejemplo (VS Code REST Client o Visual Studio).

Los datos se guardan en `src/SB.SistemaNomina.API/BaseDatos/` (un archivo JSON por colección; escritura atómica).

## Ejecución con SQL Server

1. Levante SQL Server en Docker (solo accesible desde este equipo):

   ```bash
   cp .env.example .env
   docker compose up -d --wait
   ```

   La contraseña de `sa` de `.env.example` coincide con `ConnectionStrings:SqlServer` de `appsettings.Development.json`. Si la cambia, cambie ambas.

2. Ejecute la API con el proveedor SQL Server:

   ```bash
   dotnet run --project src/SB.SistemaNomina.API --launch-profile http -- --Persistencia:Proveedor=SqlServer
   ```

   Al arrancar, la API crea la base de datos `SistemaNomina`, aplica las migraciones de EF Core y carga los datos iniciales.

También puede fijar el proveedor en `appsettings.json` (`"Persistencia": { "Proveedor": "SqlServer" }`) o con la variable `Persistencia__Proveedor=SqlServer`.

### Migraciones de EF Core (`dotnet ef database update`)

Alternativa a la migración automática al arrancar. `dotnet ef` usa la cadena `ConnectionStrings:SqlServer` de los appsettings de la API (o la variable `ConnectionStrings__SqlServer`):

```bash
dotnet tool restore
dotnet ef database update --project src/SB.SistemaNomina.Infraestructura --startup-project src/SB.SistemaNomina.API
```

Tras un cambio de modelo, cree una migración nueva y regenere el script de esquema:

```bash
dotnet ef migrations add <Nombre> --project src/SB.SistemaNomina.Infraestructura --startup-project src/SB.SistemaNomina.API --output-dir Persistencia/EntityFramework/Migrations
dotnet ef migrations script --idempotent --project src/SB.SistemaNomina.Infraestructura --startup-project src/SB.SistemaNomina.API
```

### Scripts SQL

Otra alternativa, por ejemplo para un DBA. Ejecútelos en este orden:

| Script | Contenido |
|---|---|
| `base-datos/scripts/crear-base-datos.sql` | Crea `SistemaNomina` (colación `Latin1_General_100_CI_AI`) y todas las tablas, índices y llaves. Equivale a las migraciones. |
| `base-datos/scripts/datos-iniciales.sql` | 6 departamentos y 181 entidades gubernamentales. |
| `base-datos/scripts/empleados-demostracion.sql` | Opcional: 240 empleados de ejemplo de los cuatro tipos. |
| `base-datos/scripts/reporte-nomina-semanal.sql` | Consulta: reporte semanal calculado en SQL (pago por empleado activo, totales por tipo y total general). Mismo resultado que la API. |

Todos son idempotentes. Los usuarios no se incluyen: la API los crea al arrancar con la contraseña configurada (así ninguna contraseña queda en un script).

```bash
for script in crear-base-datos datos-iniciales empleados-demostracion reporte-nomina-semanal; do
  docker cp base-datos/scripts/$script.sql sb-nomina-sqlserver:/tmp/
  docker exec sb-nomina-sqlserver bash -c "/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P \"\$MSSQL_SA_PASSWORD\" -C -b -i /tmp/$script.sql"
done
```

## Configuración

| Clave | Descripción |
|---|---|
| `Persistencia:Proveedor` | `Json` (predeterminado) o `SqlServer`. |
| `ConnectionStrings:BaseDatosJson` | Carpeta de los archivos JSON (relativa al proyecto API). |
| `ConnectionStrings:SqlServer` | Cadena de conexión de SQL Server. |
| `Autenticacion:ClaveFirma` | Clave HMAC del JWT, mínimo 32 bytes. Vacía en `appsettings.json`: la API no arranca sin ella. |
| `Autenticacion:DuracionTokenMinutos` | Vigencia del token (60). |
| `DatosIniciales:ContrasenaAdministrador` | Contraseña del primer `admin` (8+ caracteres con mayúscula, minúscula y número). |
| `DatosIniciales:ContrasenaUsuarioConsulta` | Si se define, crea el usuario `consulta`. |
| `DatosIniciales:IncluirEmpleadosDemostracion` | `true` carga 240 empleados de ejemplo. |
| `Cors:OrigenesPermitidos` | Orígenes del frontend (`http://localhost:5173`). |
| `LimiteSolicitudes` | 100 solicitudes/60 s por usuario o IP; 5 intentos de inicio de sesión/60 s por IP. |

Fuera de Development, defina los secretos con variables de entorno, por ejemplo `Autenticacion__ClaveFirma`, `ConnectionStrings__SqlServer` y `DatosIniciales__ContrasenaAdministrador`. Ningún secreto de producción está en el repositorio.

## Datos iniciales

Al iniciar por primera vez (con cualquiera de los dos proveedores), la API crea:

- Los 6 departamentos y las 181 entidades gubernamentales del listado oficial (`ListaEntidadesGubernamentales.xlsx`).
- El usuario administrador `admin` y, si se configura su contraseña, el usuario de consulta `consulta`.

Cada catálogo solo se carga si está vacío; los usuarios existentes no se modifican.

### Empleados de demostración (opcional)

240 empleados de ejemplo: 10 por cada tipo de empleado en cada departamento, con salarios que varían según el departamento y el nivel de experiencia. Algunos están inactivos para probar los filtros y el reporte de nómina. Solo se cargan si aún no existen empleados:

```bash
dotnet run --project src/SB.SistemaNomina.API --launch-profile http -- --DatosIniciales:IncluirEmpleadosDemostracion=true
```

Para empezar desde cero: con JSON, elimine `src/SB.SistemaNomina.API/BaseDatos/`; con SQL Server, `docker compose down -v` (borra el volumen).

## Endpoints

Todas las rutas requieren token salvo el inicio de sesión. **Admin** = solo rol Admin; el resto, cualquier usuario autenticado.

| Método | Ruta | Rol | Descripción |
|---|---|---|---|
| POST | `/api/autenticacion/iniciar-sesion` | anónimo | Devuelve el JWT. |
| GET | `/api/empleados` | | Lista paginada; filtros `nombre`, `departamentoId`, `estado`, `tipo`. |
| GET | `/api/empleados/{id}` | | Empleado con desglose del pago semanal. |
| POST / PUT | `/api/empleados`, `/api/empleados/{id}` | Admin | Crear / actualizar (permite cambiar de tipo conservando el Id). |
| DELETE | `/api/empleados/{id}` | Admin | Desactivación lógica (`Inactivo`). |
| GET | `/api/departamentos`, `/{id}` | | Catálogo de departamentos. |
| POST / PUT / DELETE | `/api/departamentos`, `/{id}` | Admin | Mantenimiento; no se desactiva con empleados activos. |
| GET | `/api/entidades-gubernamentales`, `/{id}` | | Lista paginada; filtros `nombre`, `sector`, `poderDelEstado`, `incluirInactivas`. |
| POST / PUT / DELETE | `/api/entidades-gubernamentales`, `/{id}` | Admin | Mantenimiento (desactivación lógica). |
| GET | `/api/reportes-nomina/semanal` | | Reporte en vivo: pago por empleado activo, totales por tipo y total general. |
| POST | `/api/reportes-nomina/nominas` | Admin | Guarda la nómina de una semana (una por semana). |
| GET | `/api/reportes-nomina/nominas`, `/nominas/{id}` | | Historial paginado y detalle de una nómina guardada. |
| GET / POST / PUT / DELETE | `/api/usuarios`, `/{id}` | Admin | Gestión de usuarios y roles. |
| PUT | `/api/usuarios/{id}/contrasena` | Admin | Cambio de contraseña. |

Paginación: `pagina` (desde 1) y `tamanoPagina` (20 por defecto, máximo 100).

Errores: `ProblemDetails` en español con `traceId`. 400 datos inválidos (errores por campo), 401 sin token o token revocado, 403 rol insuficiente, 404 no existe, 409 duplicado, 422 regla de negocio, 429 demasiadas solicitudes.

## Cálculo del pago semanal

| Tipo | Fórmula |
|---|---|
| `Asalariado` | Salario semanal fijo. |
| `PorHoras` | Hasta 40 h: horas × sueldo. Más de 40 h: 40 × sueldo + (horas − 40) × sueldo × 1.5. |
| `PorComision` | Ventas brutas × tarifa de comisión. |
| `AsalariadoPorComision` | Salario base + ventas × tarifa + 10 % del salario base. |

Cada pago incluye su desglose por línea; cada línea se redondea a 2 decimales y el total es la suma de las líneas.

## Seguridad

- Contraseñas con `PasswordHasher<T>` (PBKDF2 con sal por usuario); nunca se registran ni se devuelven.
- JWT HS256 validado (emisor, audiencia, vigencia, firma). En cada solicitud se verifica que el usuario siga activo y conserve su rol: degradar o desactivar un usuario invalida sus tokens de inmediato.
- Un administrador no puede desactivarse ni quitarse el rol a sí mismo, y siempre queda al menos un administrador activo.
- Cédula visible completa solo para Admin; el rol Usuario la ve enmascarada (`***-****567-1`).
- Límite de solicitudes, CORS con orígenes explícitos y errores 500 sin detalles internos fuera de Development.
- Logs estructurados (Serilog) en consola y `Logs/nomina-*.log`, sin datos sensibles.

## Pruebas

```bash
dotnet test
```

347 pruebas: fórmulas de pago y desglose, validaciones, servicios (empleados, nómina, usuarios, autenticación), revocación de tokens en cada solicitud, persistencia JSON, selección de proveedor y modelo de SQL Server, y rendimiento (reporte de 1,000 empleados en menos de 2 segundos).

## Supuestos de diseño

1. La especificación de la API pide una base de datos en texto plano: es el proveedor predeterminado (JSON). SQL Server con EF Core se ofrece como proveedor intercambiable para cumplir el requisito de base de datos de la prueba.
2. `PrimerNombre` es obligatorio también para empleados por horas (su omisión en el documento se trató como error tipográfico).
3. El "número de seguro social" se reemplaza por la cédula dominicana, formato `###-#######-#` (solo formato, sin dígito verificador). El rol Usuario la ve enmascarada; Admin, completa.
4. El pago se calcula al consultar con los datos actuales; las nóminas guardadas congelan el resultado de una semana.
5. Los roles se llaman `Admin` y `Usuario`, como en la prueba.
6. Nuevo tipo de empleado = nueva subclase de `Empleado` + su creador + su validador + un valor nuevo en `TipoEmpleado`. Ambos proveedores de persistencia descubren las subclases por reflexión. Solo se agrega código; la lógica existente no cambia.
7. Código en español para coincidir con los documentos de evaluación; el inglés se mantiene solo donde lo exigen el framework o sus convenciones (`*Controller`, `*Async`, `*Exception`, `appsettings`, `Migrations`).
8. Montos con máximo 2 decimales, horas con 3 y tarifa de comisión con 6, igual que las columnas de SQL Server.
