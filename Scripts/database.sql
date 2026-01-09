-- Script de creación de base de datos para Servicio PLC
-- SQL Server 2025

USE master;
GO

-- Crear base de datos si no existe
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ProduccionLocal')
BEGIN
    CREATE DATABASE ProduccionLocal;
    PRINT 'Base de datos ProduccionLocal creada correctamente';
END
ELSE
BEGIN
    PRINT 'Base de datos ProduccionLocal ya existe';
END
GO

USE ProduccionLocal;
GO

-- Tabla principal de lecturas PLC
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LecturasPLC')
BEGIN
    CREATE TABLE LecturasPLC (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        PlcId INT NOT NULL,
        NombrePlc NVARCHAR(50) NOT NULL,
        FechaHoraLectura DATETIME2(3) NOT NULL DEFAULT GETDATE(),
        
        -- Datos de producción
        KgHoraActual FLOAT NOT NULL,
        EspesorActual FLOAT NOT NULL,
        VelocidadLinea FLOAT NOT NULL,
        TemperaturaProceso FLOAT NOT NULL,
        ContadorProduccion INT NOT NULL,
        EstadoMaquina BIT NOT NULL,
        
        -- 👉 AGREGAR AQUÍ MÁS COLUMNAS SEGÚN TUS NECESIDADES:
        -- Presion FLOAT NULL,
        -- Humedad FLOAT NULL,
        -- AlarmasActivas INT NULL,
        -- ConsumoEnergia FLOAT NULL,
        
        -- Metadata
        FechaHoraInsercion DATETIME2(3) NOT NULL DEFAULT GETDATE(),
        
        INDEX IX_LecturasPLC_PlcId_Fecha (PlcId, FechaHoraLectura DESC),
        INDEX IX_LecturasPLC_Fecha (FechaHoraLectura DESC)
    );
    
    PRINT 'Tabla LecturasPLC creada correctamente';
END
ELSE
BEGIN
    PRINT 'Tabla LecturasPLC ya existe';
END
GO

-- Tabla de configuración de PLCs
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ConfiguracionPLCs')
BEGIN
    CREATE TABLE ConfiguracionPLCs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(50) NOT NULL UNIQUE,
        Ip NVARCHAR(15) NOT NULL,
        Puerto INT NOT NULL,
        Habilitada BIT NOT NULL DEFAULT 1,
        TipoConexion NVARCHAR(20) NOT NULL DEFAULT 'Modbus',
        Descripcion NVARCHAR(200) NULL,
        FechaCreacion DATETIME2(3) NOT NULL DEFAULT GETDATE(),
        FechaModificacion DATETIME2(3) NOT NULL DEFAULT GETDATE()
    );
    
    PRINT 'Tabla ConfiguracionPLCs creada correctamente';
END
GO

-- Tabla de eventos del sistema
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EventosSistema')
BEGIN
    CREATE TABLE EventosSistema (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        FechaHora DATETIME2(3) NOT NULL DEFAULT GETDATE(),
        TipoEvento NVARCHAR(50) NOT NULL,
        Mensaje NVARCHAR(500) NOT NULL,
        PlcId INT NULL,
        Severidad NVARCHAR(10) NOT NULL DEFAULT 'INFO',
        
        INDEX IX_EventosSistema_Fecha (FechaHora DESC),
        INDEX IX_EventosSistema_Tipo (TipoEvento),
        INDEX IX_EventosSistema_PlcId (PlcId)
    );
    
    PRINT 'Tabla EventosSistema creada correctamente';
END
GO

-- Insertar datos de ejemplo de configuración
IF NOT EXISTS (SELECT * FROM ConfiguracionPLCs)
BEGIN
    INSERT INTO ConfiguracionPLCs (Nombre, Ip, Puerto, Habilitada, TipoConexion, Descripcion)
    VALUES 
        ('PLC1', '192.168.0.10', 502, 1, 'Modbus', 'PLC Línea de Producción 1'),
        ('PLC2', '192.168.0.11', 502, 1, 'Modbus', 'PLC Línea de Producción 2'),
        ('PLC3', '192.168.0.12', 502, 0, 'Modbus', 'PLC Línea de Producción 3 - Deshabilitado');
    
    PRINT 'Datos de configuración insertados correctamente';
END
GO

-- Procedimiento almacenado para insertar lecturas (opcional, más eficiente)
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_InsertarLecturaPLC')
BEGIN
    DROP PROCEDURE sp_InsertarLecturaPLC;
END
GO

CREATE PROCEDURE sp_InsertarLecturaPLC
    @PlcId INT,
    @NombrePlc NVARCHAR(50),
    @FechaHoraLectura DATETIME2(3),
    @KgHoraActual FLOAT,
    @EspesorActual FLOAT,
    @VelocidadLinea FLOAT,
    @TemperaturaProceso FLOAT,
    @ContadorProduccion INT,
    @EstadoMaquina BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO LecturasPLC 
    (PlcId, NombrePlc, FechaHoraLectura, KgHoraActual, EspesorActual, 
     VelocidadLinea, TemperaturaProceso, ContadorProduccion, EstadoMaquina)
    VALUES 
    (@PlcId, @NombrePlc, @FechaHoraLectura, @KgHoraActual, @EspesorActual, 
     @VelocidadLinea, @TemperaturaProceso, @ContadorProduccion, @EstadoMaquina);
    
    -- 👉 AGREGAR AQUÍ MÁS PARÁMETROS SI AGREGASTE MÁS COLUMNAS
END
GO

PRINT 'Procedimiento sp_InsertarLecturaPLC creado correctamente';
GO

-- Vista para consultas rápidas de últimas lecturas
IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_UltimasLecturas')
BEGIN
    DROP VIEW vw_UltimasLecturas;
END
GO

CREATE VIEW vw_UltimasLecturas
AS
SELECT TOP 100
    PlcId,
    NombrePlc,
    FechaHoraLectura,
    KgHoraActual,
    EspesorActual,
    VelocidadLinea,
    TemperaturaProceso,
    ContadorProduccion,
    EstadoMaquina,
    FechaHoraInsercion
FROM LecturasPLC
ORDER BY FechaHoraLectura DESC;
GO

PRINT 'Vista vw_UltimasLecturas creada correctamente';
GO

-- Procedimiento para limpieza de datos antiguos (mantenimiento)
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_LimpiarDatosAntiguos')
BEGIN
    DROP PROCEDURE sp_LimpiarDatosAntiguos;
END
GO

CREATE PROCEDURE sp_LimpiarDatosAntiguos
    @DiasAntiguedad INT = 90
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @FechaLimite DATETIME2(3) = DATEADD(DAY, -@DiasAntiguedad, GETDATE());
    DECLARE @RegistrosEliminados INT;
    
    -- Eliminar lecturas antiguas
    DELETE FROM LecturasPLC 
    WHERE FechaHoraLectura < @FechaLimite;
    
    SET @RegistrosEliminados = @@ROWCOUNT;
    
    -- Eliminar eventos antiguos
    DELETE FROM EventosSistema 
    WHERE FechaHora < @FechaLimite;
    
    PRINT 'Registros eliminados: ' + CAST(@RegistrosEliminados AS NVARCHAR(10));
    
    -- Reorganizar índices
    ALTER INDEX ALL ON LecturasPLC REORGANIZE;
    ALTER INDEX ALL ON EventosSistema REORGANIZE;
    
    PRINT 'Limpieza completada correctamente';
END
GO

PRINT 'Procedimiento sp_LimpiarDatosAntiguos creado correctamente';
GO

PRINT '';
PRINT '========================================';
PRINT 'Base de datos configurada correctamente';
PRINT '========================================';
PRINT '';
PRINT 'Tablas creadas:';
PRINT '  - LecturasPLC';
PRINT '  - ConfiguracionPLCs';
PRINT '  - EventosSistema';
PRINT '';
PRINT 'Vistas creadas:';
PRINT '  - vw_UltimasLecturas';
PRINT '';
PRINT 'Procedimientos almacenados:';
PRINT '  - sp_InsertarLecturaPLC';
PRINT '  - sp_LimpiarDatosAntiguos';
PRINT '';
GO
