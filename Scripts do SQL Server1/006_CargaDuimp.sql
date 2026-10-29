USE AtlasComex;
GO

-- Dados principais da carga vinculada à DUIMP.
-- Modal e dados gerais do processo continuam na tabela Processo.
IF OBJECT_ID('dbo.CargaDuimp') IS NULL
BEGIN
	CREATE TABLE dbo.CargaDuimp
	(
		 IdDuimp BIGINT NOT NULL,
        TipoIdentificacaoCarga VARCHAR(3) NULL,
        IdentificacaoCarga VARCHAR(32) NULL,
        LocalEmbarque NVARCHAR(150) NULL,
        DataConhecimento DATE NULL,
        PaisProcedencia CHAR(2) NULL,
        PesoBruto DECIMAL(18,6) NULL,
        PesoLiquido DECIMAL(18,6) NULL,
        CodigoMoedaFrete CHAR(3) NULL,
        ValorFreteMoeda DECIMAL(19,6) NULL,
        ValorFreteReais DECIMAL(19,2) NULL,
        ValorAfrmmTum DECIMAL(19,2) NULL,
        AfrmmTumPago BIT NULL,
        CodigoMoedaSeguro CHAR(3) NULL,
        ValorSeguroMoeda DECIMAL(19,6) NULL,
        ValorSeguroReais DECIMAL(19,2) NULL,

        DataCriacao DATETIME2(0) NOT NULL
            CONSTRAINT DF_CargaDuimp_DataCriacao DEFAULT (SYSUTCDATETIME()),
        DataAtualizacao DATETIME2(0) NOT NULL
            CONSTRAINT DF_CargaDuimp_DataAtualizacao DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_CargaDuimp PRIMARY KEY (IdDuimp),
        CONSTRAINT FK_CargaDuimp_Duimp
            FOREIGN KEY (IdDuimp) REFERENCES dbo.Duimp (IdDuimp),
        CONSTRAINT CK_CargaDuimp_TipoIdentificacao
            CHECK (TipoIdentificacaoCarga IS NULL
                OR TipoIdentificacaoCarga IN ('CE', 'RUC'))
	);
END;
GO

-- Uma DUIMP pode ter várias unidades de despacho.
IF OBJECT_ID('dbo.UnidadeDespacho', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UnidadeDespacho
    (
          IdUnidadeDespachoDuimp BIGINT IDENTITY(1,1) NOT NULL,
        IdDuimp BIGINT NOT NULL,
        CodigoUnidadeDespacho NVARCHAR(30) NOT NULL,

        CONSTRAINT PK_UnidadeDespachoDuimp
            PRIMARY KEY (IdUnidadeDespachoDuimp),
        CONSTRAINT FK_UnidadeDespachoDuimp_CargaDuimp
            FOREIGN KEY (IdDuimp) REFERENCES dbo.CargaDuimp (IdDuimp),
        CONSTRAINT UQ_UnidadeDespachoDuimp
            UNIQUE (IdDuimp, CodigoUnidadeDespacho)
    );
END;

-- Guarda os componentes consultados no CE Mercante.
IF OBJECT_ID(N'dbo.ComponenteFreteDuimp', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ComponenteFreteDuimp
    (
        IdComponenteFreteDuimp BIGINT IDENTITY(1,1) NOT NULL,
        IdDuimp BIGINT NOT NULL,
        Pagamento NVARCHAR(50) NULL,
        Tipo NVARCHAR(100) NULL,
        Moeda CHAR(3) NULL,
        ValorMoeda DECIMAL(19,6) NULL,

        CONSTRAINT PK_ComponenteFreteDuimp
            PRIMARY KEY (IdComponenteFreteDuimp),
        CONSTRAINT FK_ComponenteFreteDuimp_CargaDuimp
            FOREIGN KEY (IdDuimp) REFERENCES dbo.CargaDuimp (IdDuimp)
    );
END;
GO

-- Uma carga pode conter várias embalagens.
IF OBJECT_ID(N'dbo.EmbalagemDuimp', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmbalagemDuimp
    (
        IdEmbalagemDuimp BIGINT IDENTITY(1,1) NOT NULL,
        IdDuimp BIGINT NOT NULL,
        Item NVARCHAR(50) NULL,
        Tipo NVARCHAR(100) NULL,
        Quantidade DECIMAL(18,6) NULL,

        CONSTRAINT PK_EmbalagemDuimp
            PRIMARY KEY (IdEmbalagemDuimp),
        CONSTRAINT FK_EmbalagemDuimp_CargaDuimp
            FOREIGN KEY (IdDuimp) REFERENCES dbo.CargaDuimp (IdDuimp)
    );
END;
GO