using AtlasComex.Infrastructure.Models;
using System.ComponentModel.DataAnnotations;

namespace atlas_back_end.Dtos.Duimp;

//O DTO define os dados que a tela pode enviar e receber. Ele não expõe chaves e status internos do banco; o backend mantém esses valores sob seu controle.
public sealed class DuimpRascunhoDto
{

    [RegularExpression("^[FJ]$", ErrorMessage = "O tipo de pessoa deve ser F ou J.")]
    public string? TipoPessoaImportador { get; set; }

    [MaxLength(14)]
    public string? DocumentoImportador { get; set; }

    [MaxLength(200)]
    public string? NomeImportador { get; set; }
    public string? LogradouroImportador { get; set; }
    public string? NumeroEnderecoImportador { get; set; }
    public string? ComplementoEnderecoImportador { get; set; }
    public string? BairroImportador { get; set; }
    public string? CodigoMunicipioImportador { get; set; }
    public string? MunicipioImportador { get; set; }
    public string? UfImportador { get; set; }
    public string? CepImportador { get; set; }
    public string? PaisImportador { get; set; }
    public string? InformacoesComplementaresGeradas { get; set; }
    public string? InformacoesComplementaresAdicionais { get; set; }

    public CargaDuimpDto? Carga { get; set; }

    public List<ItemDuimpDto> Itens { get; set; } = [];
}

public sealed class CargaDuimpDto
{
    [RegularExpression("^(CE|RUC)$")]
    public string? TipoIdentificacaoCarga { get; set; }

    [MaxLength(32)]
    public string? IdentificacaoCarga { get; set; }

    [MaxLength(150)]
    public string? LocalEmbarque { get; set; }

    public DateOnly? DataConhecimento { get; set; }

    [RegularExpression("^[A-Za-z]{2}$")]
    public string? PaisProcedencia { get; set; }

    public decimal? PesoBruto { get; set; }
    public decimal? PesoLiquido { get; set; }

    [MaxLength(3)]
    public string? CodigoMoedaFrete { get; set; }

    public decimal? ValorFreteMoeda { get; set; }
    public decimal? ValorFreteReais { get; set; }
    public decimal? ValorAfrmmTum { get; set; }
    public bool? AfrmmTumPago { get; set; }

    [MaxLength(3)]
    public string? CodigoMoedaSeguro { get; set; }

    public decimal? ValorSeguroMoeda { get; set; }
    public decimal? ValorSeguroReais { get; set; }

    public List<string> UnidadesDespacho { get; set; } = [];
    public List<ComponenteFreteDuimpDto> ComponentesFrete { get; set; } = [];
    public List<EmbalagemDuimpDto> Embalagens { get; set; } = [];
}

public sealed class ComponenteFreteDuimpDto
{
    public string? Pagamento { get; set; }
    public string? Tipo { get; set; }
    public string? Moeda { get; set; }
    public decimal? ValorMoeda { get; set; }
}

public sealed class EmbalagemDuimpDto
{
    public string? Item { get; set; }
    public string? Tipo { get; set; }
    public decimal? Quantidade { get; set; }
}

public sealed class ItemDuimpDto
{
    [Range(1, int.MaxValue)]
    public int NumeroItem { get; set; }
    public int? NumeroAdicao { get; set; }
    public string? CaracterizacaoImportacao { get; set; }
    public string? NomeProduto { get; set; }
    public string? VersaoProduto { get; set; }
    public string? Ncm { get; set; }
    public string? PaisOrigem { get; set; }
    public string? CodigoFabricante { get; set; }
    public string? NomeFabricante { get; set; }
    public string? VersaoFabricante { get; set; }
    public string? EnderecoFabricante { get; set; }
    public bool? ExportadorEhFabricante { get; set; }
    public string? PaisExportador { get; set; }
    public string? CodigoExportador { get; set; }
    public string? NomeExportador { get; set; }
    public string? VersaoExportador { get; set; }
    public string? EnderecoExportador { get; set; }
    public string? UfConsumo { get; set; }
    public string? CodigoMunicipioConsumo { get; set; }
    public string? MunicipioConsumo { get; set; }
    public string? Aplicacao { get; set; }
    public string? CondicaoMercadoria { get; set; }
    public string? UnidadeMedidaEstatistica { get; set; }
    public decimal? QuantidadeMedidaEstatistica { get; set; }
    public decimal? PesoLiquido { get; set; }
    public string? UnidadeComercializada { get; set; }
    public decimal? QuantidadeUnidadeComercializada { get; set; }
    public string? MoedaNegociada { get; set; }
    public decimal? ValorUnitarioCondicaoVenda { get; set; }
    public string? NumeroSerie { get; set; }
    public string? DetalhamentoProduto { get; set; }
    public string? MetodoValoracao { get; set; }
    public string? Incoterm { get; set; }
    public string? CoberturaCambial { get; set; }
}