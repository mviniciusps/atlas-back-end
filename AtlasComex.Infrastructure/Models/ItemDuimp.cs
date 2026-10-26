using System;
using System.Collections.Generic;

namespace AtlasComex.Infrastructure.Models;

public partial class ItemDuimp
{
    public long IdItemDuimp { get; set; }

    public long IdDuimp { get; set; }

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

    public DateTime DataCriacao { get; set; }

    public DateTime DataAtualizacao { get; set; }

    public virtual Duimp IdDuimpNavigation { get; set; } = null!;
}
