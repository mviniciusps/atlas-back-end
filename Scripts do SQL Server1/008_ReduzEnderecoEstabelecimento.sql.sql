USE [AtlasComex];
GO

IF COL_LENGTH('dbo.EstabelecimentoImportador', 'TipoLogradouro') IS NOT NULL
    ALTER TABLE dbo.EstabelecimentoImportador DROP COLUMN TipoLogradouro;
GO

IF COL_LENGTH('dbo.EstabelecimentoImportador', 'CodigoMunicipioIbge') IS NOT NULL
    ALTER TABLE dbo.EstabelecimentoImportador DROP COLUMN CodigoMunicipioIbge;
GO

IF COL_LENGTH('dbo.EstabelecimentoImportador', 'CodigoMunicipioReceita') IS NOT NULL
    ALTER TABLE dbo.EstabelecimentoImportador DROP COLUMN CodigoMunicipioReceita;
GO

DECLARE @constraintAtivo sysname;
DECLARE @sql nvarchar(400);

SELECT @constraintAtivo = dc.name
FROM sys.default_constraints AS dc
JOIN sys.columns AS c
    ON c.object_id = dc.parent_object_id
   AND c.column_id = dc.parent_column_id
WHERE dc.parent_object_id = OBJECT_ID(N'dbo.EstabelecimentoImportador')
  AND c.name = N'Ativo';

IF @constraintAtivo IS NOT NULL
BEGIN
    SET @sql =
        N'ALTER TABLE dbo.EstabelecimentoImportador DROP CONSTRAINT '
        + QUOTENAME(@constraintAtivo);

    EXEC sys.sp_executesql @sql;
END;

IF COL_LENGTH('dbo.EstabelecimentoImportador', 'Ativo') IS NOT NULL
    ALTER TABLE dbo.EstabelecimentoImportador DROP COLUMN Ativo;
GO

IF COL_LENGTH('dbo.EstabelecimentoImportador', 'NomeEstabelecimento') IS NOT NULL
    ALTER TABLE dbo.EstabelecimentoImportador DROP COLUMN NomeEstabelecimento;
GO

SELECT * FROM EstabelecimentoImportador;