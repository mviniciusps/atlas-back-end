USE [AtlasComex];
GO

IF COL_LENGTH('dbo.EstabelecimentoImportador', 'TipoLogradouro') IS NULL
    ALTER TABLE dbo.EstabelecimentoImportador ADD TipoLogradouro nvarchar(50) NULL;

IF COL_LENGTH('dbo.EstabelecimentoImportador', 'Logradouro') IS NULL
    ALTER TABLE dbo.EstabelecimentoImportador ADD Logradouro nvarchar(200) NULL;

IF COL_LENGTH('dbo.EstabelecimentoImportador', 'NumeroEndereco') IS NULL
    ALTER TABLE dbo.EstabelecimentoImportador ADD NumeroEndereco nvarchar(30) NULL;

IF COL_LENGTH('dbo.EstabelecimentoImportador', 'ComplementoEndereco') IS NULL
    ALTER TABLE dbo.EstabelecimentoImportador ADD ComplementoEndereco nvarchar(200) NULL;

IF COL_LENGTH('dbo.EstabelecimentoImportador', 'Bairro') IS NULL
    ALTER TABLE dbo.EstabelecimentoImportador ADD Bairro nvarchar(100) NULL;

IF COL_LENGTH('dbo.EstabelecimentoImportador', 'CodigoMunicipioIbge') IS NULL
    ALTER TABLE dbo.EstabelecimentoImportador ADD CodigoMunicipioIbge int NULL;

IF COL_LENGTH('dbo.EstabelecimentoImportador', 'CodigoMunicipioReceita') IS NULL
    ALTER TABLE dbo.EstabelecimentoImportador ADD CodigoMunicipioReceita int NULL;

IF COL_LENGTH('dbo.EstabelecimentoImportador', 'Municipio') IS NULL
    ALTER TABLE dbo.EstabelecimentoImportador ADD Municipio nvarchar(100) NULL;

IF COL_LENGTH('dbo.EstabelecimentoImportador', 'Uf') IS NULL
    ALTER TABLE dbo.EstabelecimentoImportador ADD Uf char(2) NULL;

IF COL_LENGTH('dbo.EstabelecimentoImportador', 'Cep') IS NULL
    ALTER TABLE dbo.EstabelecimentoImportador ADD Cep varchar(8) NULL;