USE [AtlasComex];
GO

CREATE TABLE [dbo].[DocumentoProcesso]
(
    [IdDocumento] INT IDENTITY(1,1) NOT NULL,
    [IdProcesso] BIGINT NOT NULL,
    [TipoDocumento] NVARCHAR(80) NOT NULL,
    [NomeArquivo] NVARCHAR(260) NOT NULL,
    [ContentType] NVARCHAR(150) NULL,
    [TamanhoBytes] BIGINT NOT NULL,
    [CaminhoArmazenamento] NVARCHAR(500) NOT NULL,
    [IncluirNaDuimp] BIT NOT NULL
        CONSTRAINT [DF_DocumentoProcesso_IncluirNaDuimp] DEFAULT (0),
    [DataUpload] DATETIME2(0) NOT NULL
        CONSTRAINT [DF_DocumentoProcesso_DataUpload] DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT [PK_DocumentoProcesso]
        PRIMARY KEY ([IdDocumento]),

    CONSTRAINT [FK_DocumentoProcesso_Processo]
        FOREIGN KEY ([IdProcesso])
        REFERENCES [dbo].[Processo] ([IdProcesso])
);
GO

BEGIN TRANSACTION;
SELECT TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME = 'DocumentoProcesso';
COMMIT TRANSACTION;
