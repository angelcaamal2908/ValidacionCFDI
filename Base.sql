-- Crear la base de datos
CREATE DATABASE CFDIReg;


-- Usar la base de datos
USE CFDIReg;

-- Crear la tabla Cfdis
CREATE TABLE Cfdis (
    Id INT PRIMARY KEY IDENTITY(1,1),
    RfcEmisor NVARCHAR(20) NOT NULL,
    RfcReceptor NVARCHAR(20) NOT NULL,
    FolioFiscal NVARCHAR(36) NOT NULL UNIQUE,
    FechaEmision DATETIME NOT NULL,
    Total DECIMAL(18,2) NOT NULL,
    Estatus NVARCHAR(20) NOT NULL
);

-- Crear la tabla LogValidacion
CREATE TABLE LogValidacion (
    Id INT PRIMARY KEY IDENTITY(1,1),
    RfcEmisor NVARCHAR(20),
    RfcReceptor NVARCHAR(20),
    FolioFiscal NVARCHAR(36),
    Total DECIMAL(18,2),
    Estatus NVARCHAR(20),
    ResponseData NVARCHAR(MAX),
    ErrorMessage NVARCHAR(MAX)
);