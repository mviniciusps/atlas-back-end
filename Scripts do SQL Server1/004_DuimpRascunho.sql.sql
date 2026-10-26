USE AtlasComex;
GO

-- Uma DUIMP vinculada a cada processo
IF OBJECT_ID(N'dbo.Duimp', N'U')IS NULL
BEGIN
	CREATE TABLE dbo.Duimp
	(
		IdDuimp BIGINT IDENTITY(1,1) NOT NULL,
		IdProcesso BIGINT NOT NULL,

		TipoPessoaImportador CHAR(1) NULL,
		DocumentoImportador VARCHAR(14) NULL,
		NomeImportador CHAR(1) NULL,

		LogradouroImportador NVARCHAR(200) NULL,
		NumeroEnderecoImportador NVARCHAR(30) NULL,
		ComplementoEnderecoImportador NVARCHAR(100) NULL,
		BairroImportador NVARCHAR(100) NULL,
		CodigoMunicipioImportador VARCHAR(10) NULL,
		MunicipioImportador NVARCHAR(100) NULL,
		UfImportador CHAR(2) NULL,
		CepImportador VARCHAR(8) NULL,
		PaisImportador VARCHAR(10) NULL,

		InformacoesComplementaresGeradas NVARCHAR(MAX) NULL,
		InformacoesComplementaresAdicionais NVARCHAR(MAX) NULL,

		  StatusRascunho NVARCHAR(30) NOT NULL
            CONSTRAINT DF_Duimp_StatusRascunho DEFAULT (N'Rascunho'),

        StatusIntegracaoPortal NVARCHAR(30) NOT NULL
            CONSTRAINT DF_Duimp_StatusIntegracaoPortal DEFAULT (N'NaoEnviada'),

        DataCriacao DATETIME2(0) NOT NULL
            CONSTRAINT DF_Duimp_DataCriacao DEFAULT (SYSUTCDATETIME()),

        DataAtualizacao DATETIME2(0) NOT NULL
            CONSTRAINT DF_Duimp_DataAtualizacao DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_Duimp PRIMARY KEY (IdDuimp),

        CONSTRAINT UQ_Duimp_IdProcesso UNIQUE (IdProcesso),

        CONSTRAINT FK_Duimp_Processo
            FOREIGN KEY (IdProcesso)
            REFERENCES dbo.Processo (IdProcesso),

        CONSTRAINT CK_Duimp_TipoPessoaImportador
            CHECK (
                TipoPessoaImportador IS NULL
                OR TipoPessoaImportador IN ('F', 'J')
            )
    );
END;
GO

-- Cada DUIMP pode conter vários itens de mercadoria.
IF OBJECT_ID(N'dbo.ItemDuimp', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ItemDuimp
    (
        IdItemDuimp BIGINT IDENTITY(1,1) NOT NULL,
        IdDuimp BIGINT NOT NULL,
        NumeroItem INT NOT NULL,
        NumeroAdicao INT NULL,

        CaracterizacaoImportacao NVARCHAR(60) NULL,

        NomeProduto NVARCHAR(200) NULL,
        VersaoProduto NVARCHAR(50) NULL,
        Ncm VARCHAR(8) NULL,

        PaisOrigem VARCHAR(10) NULL,
        CodigoFabricante NVARCHAR(100) NULL,
        NomeFabricante NVARCHAR(200) NULL,
        VersaoFabricante NVARCHAR(50) NULL,
        EnderecoFabricante NVARCHAR(500) NULL,

        ExportadorEhFabricante BIT NULL,
        PaisExportador VARCHAR(10) NULL,
        CodigoExportador NVARCHAR(100) NULL,
        NomeExportador NVARCHAR(200) NULL,
        VersaoExportador NVARCHAR(50) NULL,
        EnderecoExportador NVARCHAR(500) NULL,

        UfConsumo CHAR(2) NULL,
        CodigoMunicipioConsumo VARCHAR(10) NULL,
        MunicipioConsumo NVARCHAR(100) NULL,

        Aplicacao NVARCHAR(100) NULL,
        CondicaoMercadoria NVARCHAR(20) NULL,
        UnidadeMedidaEstatistica NVARCHAR(30) NULL,
        QuantidadeMedidaEstatistica DECIMAL(18,6) NULL,
        PesoLiquido DECIMAL(18,6) NULL,
        UnidadeComercializada NVARCHAR(30) NULL,
        QuantidadeUnidadeComercializada DECIMAL(18,6) NULL,
        MoedaNegociada CHAR(3) NULL,
        ValorUnitarioCondicaoVenda DECIMAL(19,6) NULL,

        NumeroSerie NVARCHAR(150) NULL,
        DetalhamentoProduto NVARCHAR(MAX) NULL,

        MetodoValoracao NVARCHAR(100) NULL,
        Incoterm CHAR(3) NULL,
        CoberturaCambial NVARCHAR(50) NULL,

        DataCriacao DATETIME2(0) NOT NULL
            CONSTRAINT DF_ItemDuimp_DataCriacao DEFAULT (SYSUTCDATETIME()),

        DataAtualizacao DATETIME2(0) NOT NULL
            CONSTRAINT DF_ItemDuimp_DataAtualizacao DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_ItemDuimp PRIMARY KEY (IdItemDuimp),

        CONSTRAINT FK_ItemDuimp_Duimp
            FOREIGN KEY (IdDuimp)
            REFERENCES dbo.Duimp (IdDuimp),

        CONSTRAINT UQ_ItemDuimp_Duimp_NumeroItem
            UNIQUE (IdDuimp, NumeroItem),

        CONSTRAINT CK_ItemDuimp_NumeroItem
            CHECK (NumeroItem > 0)
    );
END;
GO