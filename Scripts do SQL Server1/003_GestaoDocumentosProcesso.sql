IF COL_LENGTH(N'dbo.DocumentoProcesso', N'StatusConferencia') IS NULL
BEGIN
	ALTER TABLE dbo.DocumentoProcesso
	ADD StatusConferencia NVARCHAR(30) NOT NULL
		CONSTRAINT DF_DocumentoProcesso_StatusConferencia
		DEFAULT (N'Aguardando conferência') WITH VALUES;
END;
GO

IF NOT EXISTS(
	SELECT 1
	FROM sys.check_constraints
	WHERE name = N'CK_DocumentoProcesso_StatusConferencia'
)
BEGIN
	ALTER TABLE dbo.DocumentoProcesso
	ADD CONSTRAINT CK_DocumentoProcesso_StatusConferencia
	CHECK (
		StatusConferencia IN (
			N'Aguardando conferência',
			N'Conferido',
			N'Não conferido'
		)
	);
END;

SELECT IdDocumento, TipoDocumento, StatusConferencia
FROM dbo.DocumentoProcesso