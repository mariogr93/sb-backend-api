-- Reporte semanal de nómina calculado en SQL: mismo resultado que GET /api/reportes-nomina/semanal.
-- Solo empleados activos; cada concepto se redondea a 2 decimales y el pago es la suma de los conceptos.
-- Opcional: asigne @DepartamentoId para filtrar un departamento.
USE [SistemaNomina];
GO

DECLARE @DepartamentoId uniqueidentifier = NULL;
DECLARE @HORAS_SEMANALES_ESTANDAR decimal(6, 3) = 40;
DECLARE @MULTIPLICADOR_HORAS_EXTRA decimal(4, 2) = 1.5;
DECLARE @TASA_BONO_SALARIO decimal(4, 2) = 0.10;
DECLARE @DECIMALES_MONEDA int = 2;

WITH Conceptos AS (
    SELECT
        empleado.Id,
        empleado.PrimerNombre + N' ' + empleado.ApellidoPaterno AS NombreCompleto,
        empleado.TipoEmpleado,
        departamento.Nombre AS Departamento,
        -- Asalariado: salario semanal. Asalariado por comisión: salario base.
        COALESCE(empleado.SalarioSemanal, empleado.SalarioBase, 0) AS Salario,
        ROUND(ISNULL(empleado.SueldoPorHora
            * IIF(empleado.HorasTrabajadas > @HORAS_SEMANALES_ESTANDAR, @HORAS_SEMANALES_ESTANDAR, empleado.HorasTrabajadas), 0),
            @DECIMALES_MONEDA) AS HorasRegulares,
        ROUND(ISNULL(empleado.SueldoPorHora * @MULTIPLICADOR_HORAS_EXTRA
            * IIF(empleado.HorasTrabajadas > @HORAS_SEMANALES_ESTANDAR, empleado.HorasTrabajadas - @HORAS_SEMANALES_ESTANDAR, 0), 0),
            @DECIMALES_MONEDA) AS HorasExtra,
        ROUND(ISNULL(empleado.VentasBrutas * empleado.TarifaComision, 0), @DECIMALES_MONEDA) AS Comision,
        ROUND(ISNULL(empleado.SalarioBase * @TASA_BONO_SALARIO, 0), @DECIMALES_MONEDA) AS Bono
    FROM [Empleados] AS empleado
    INNER JOIN [Departamentos] AS departamento ON departamento.Id = empleado.DepartamentoId
    -- Usa el índice (Estado, DepartamentoId).
    WHERE empleado.Estado = N'Activo'
        AND (@DepartamentoId IS NULL OR empleado.DepartamentoId = @DepartamentoId)
),
Pagos AS (
    SELECT *, Salario + HorasRegulares + HorasExtra + Comision + Bono AS PagoSemanal
    FROM Conceptos
)
SELECT
    NombreCompleto,
    TipoEmpleado,
    Departamento,
    CAST(Salario AS decimal(18, 2)) AS Salario,
    CAST(HorasRegulares AS decimal(18, 2)) AS HorasRegulares,
    CAST(HorasExtra AS decimal(18, 2)) AS HorasExtra,
    CAST(Comision AS decimal(18, 2)) AS Comision,
    CAST(Bono AS decimal(18, 2)) AS Bono,
    CAST(PagoSemanal AS decimal(18, 2)) AS PagoSemanal
FROM Pagos
ORDER BY NombreCompleto;

-- Totales por tipo de empleado y total general (fila con TipoEmpleado = 'Total general').
WITH Pagos AS (
    SELECT
        empleado.TipoEmpleado,
        COALESCE(empleado.SalarioSemanal, empleado.SalarioBase, 0)
        + ROUND(ISNULL(empleado.SueldoPorHora
            * IIF(empleado.HorasTrabajadas > @HORAS_SEMANALES_ESTANDAR, @HORAS_SEMANALES_ESTANDAR, empleado.HorasTrabajadas), 0),
            @DECIMALES_MONEDA)
        + ROUND(ISNULL(empleado.SueldoPorHora * @MULTIPLICADOR_HORAS_EXTRA
            * IIF(empleado.HorasTrabajadas > @HORAS_SEMANALES_ESTANDAR, empleado.HorasTrabajadas - @HORAS_SEMANALES_ESTANDAR, 0), 0),
            @DECIMALES_MONEDA)
        + ROUND(ISNULL(empleado.VentasBrutas * empleado.TarifaComision, 0), @DECIMALES_MONEDA)
        + ROUND(ISNULL(empleado.SalarioBase * @TASA_BONO_SALARIO, 0), @DECIMALES_MONEDA) AS PagoSemanal
    FROM [Empleados] AS empleado
    WHERE empleado.Estado = N'Activo'
        AND (@DepartamentoId IS NULL OR empleado.DepartamentoId = @DepartamentoId)
)
SELECT
    IIF(GROUPING(TipoEmpleado) = 1, N'Total general', TipoEmpleado) AS TipoEmpleado,
    COUNT(*) AS CantidadEmpleados,
    CAST(SUM(PagoSemanal) AS decimal(18, 2)) AS MontoTotal
FROM Pagos
GROUP BY ROLLUP (TipoEmpleado)
ORDER BY GROUPING(TipoEmpleado), TipoEmpleado;
GO
