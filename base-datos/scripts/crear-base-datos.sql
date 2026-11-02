-- Crea la base de datos SistemaNomina y su esquema (migraciones de EF Core, idempotente).
-- Equivale a lo que hace la API al arrancar con Persistencia:Proveedor=SqlServer.
-- Colación sin mayúsculas ni acentos: las búsquedas se comportan igual que el proveedor JSON.
IF DB_ID(N'SistemaNomina') IS NULL
    CREATE DATABASE [SistemaNomina] COLLATE Latin1_General_100_CI_AI;
GO

USE [SistemaNomina];
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101204043_Inicial'
)
BEGIN
    CREATE TABLE [Departamentos] (
        [Id] uniqueidentifier NOT NULL,
        [Nombre] nvarchar(100) NOT NULL,
        [EstaActivo] bit NOT NULL,
        CONSTRAINT [PK_Departamentos] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101204043_Inicial'
)
BEGIN
    CREATE TABLE [EntidadesGubernamentales] (
        [Id] uniqueidentifier NOT NULL,
        [Nombre] nvarchar(200) NOT NULL,
        [Categoria] nvarchar(100) NOT NULL,
        [PoderDelEstado] nvarchar(100) NOT NULL,
        [Sector] nvarchar(100) NOT NULL,
        [EstaActiva] bit NOT NULL,
        CONSTRAINT [PK_EntidadesGubernamentales] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101204043_Inicial'
)
BEGIN
    CREATE TABLE [Nominas] (
        [Id] uniqueidentifier NOT NULL,
        [FechaInicioSemana] date NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(50) NOT NULL,
        [MontoTotal] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_Nominas] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101204043_Inicial'
)
BEGIN
    CREATE TABLE [Usuarios] (
        [Id] uniqueidentifier NOT NULL,
        [NombreUsuario] nvarchar(50) NOT NULL,
        [NombreCompleto] nvarchar(100) NOT NULL,
        [HashContrasena] nvarchar(500) NOT NULL,
        [Rol] nvarchar(30) NOT NULL,
        [EstaActivo] bit NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101204043_Inicial'
)
BEGIN
    CREATE TABLE [Empleados] (
        [Id] uniqueidentifier NOT NULL,
        [PrimerNombre] nvarchar(100) NOT NULL,
        [ApellidoPaterno] nvarchar(100) NOT NULL,
        [Cedula] nvarchar(13) NOT NULL,
        [DepartamentoId] uniqueidentifier NOT NULL,
        [Estado] nvarchar(30) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaActualizacion] datetime2 NULL,
        [TipoEmpleado] nvarchar(30) NOT NULL,
        [SalarioSemanal] decimal(18,2) NULL,
        [VentasBrutas] decimal(18,2) NULL,
        [TarifaComision] decimal(7,6) NULL,
        [SalarioBase] decimal(18,2) NULL,
        [SueldoPorHora] decimal(18,2) NULL,
        [HorasTrabajadas] decimal(6,3) NULL,
        CONSTRAINT [PK_Empleados] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Empleados_Departamentos_DepartamentoId] FOREIGN KEY ([DepartamentoId]) REFERENCES [Departamentos] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101204043_Inicial'
)
BEGIN
    CREATE TABLE [DetallesNomina] (
        [Id] uniqueidentifier NOT NULL,
        [NominaId] uniqueidentifier NOT NULL,
        [EmpleadoId] uniqueidentifier NOT NULL,
        [NombreCompletoEmpleado] nvarchar(201) NOT NULL,
        [TipoEmpleado] nvarchar(30) NOT NULL,
        [NombreDepartamento] nvarchar(100) NOT NULL,
        [PagoBruto] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_DetallesNomina] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DetallesNomina_Nominas_NominaId] FOREIGN KEY ([NominaId]) REFERENCES [Nominas] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101204043_Inicial'
)
BEGIN
    CREATE TABLE [LineasDesgloseNomina] (
        [Id] int NOT NULL IDENTITY,
        [Concepto] nvarchar(100) NOT NULL,
        [Formula] nvarchar(200) NOT NULL,
        [Monto] decimal(18,2) NOT NULL,
        [DetalleNominaId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_LineasDesgloseNomina] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LineasDesgloseNomina_DetallesNomina_DetalleNominaId] FOREIGN KEY ([DetalleNominaId]) REFERENCES [DetallesNomina] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101204043_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Departamentos_Nombre] ON [Departamentos] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101204043_Inicial'
)
BEGIN
    CREATE INDEX [IX_DetallesNomina_NominaId] ON [DetallesNomina] ([NominaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101204043_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Empleados_Cedula] ON [Empleados] ([Cedula]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101204043_Inicial'
)
BEGIN
    CREATE INDEX [IX_Empleados_DepartamentoId] ON [Empleados] ([DepartamentoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101204043_Inicial'
)
BEGIN
    CREATE INDEX [IX_Empleados_Estado_DepartamentoId] ON [Empleados] ([Estado], [DepartamentoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101204043_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_EntidadesGubernamentales_Nombre] ON [EntidadesGubernamentales] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101204043_Inicial'
)
BEGIN
    CREATE INDEX [IX_LineasDesgloseNomina_DetalleNominaId] ON [LineasDesgloseNomina] ([DetalleNominaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101204043_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Nominas_FechaInicioSemana] ON [Nominas] ([FechaInicioSemana]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101204043_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_NombreUsuario] ON [Usuarios] ([NombreUsuario]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101204043_Inicial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261101204043_Inicial', N'8.0.10');
END;
GO

COMMIT;
GO

