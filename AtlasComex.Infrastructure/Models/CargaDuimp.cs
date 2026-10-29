using System;
using System.Collections.Generic;

namespace AtlasComex.Infrastructure.Models;

public partial class CargaDuimp
{
    public long IdDuimp { get; set; }

    public string? TipoIdentificacaoCarga { get; set; }

    public string? IdentificacaoCarga { get; set; }

    public string? LocalEmbarque { get; set; }

    public DateOnly? DataConhecimento { get; set; }

    public string? PaisProcedencia { get; set; }

    public decimal? PesoBruto { get; set; }

    public decimal? PesoLiquido { get; set; }

    public string? CodigoMoedaFrete { get; set; }

    public decimal? ValorFreteMoeda { get; set; }

    public decimal? ValorFreteReais { get; set; }

    public decimal? ValorAfrmmTum { get; set; }

    public bool? AfrmmTumPago { get; set; }

    public string? CodigoMoedaSeguro { get; set; }

    public decimal? ValorSeguroMoeda { get; set; }

    public decimal? ValorSeguroReais { get; set; }

    public DateTime DataCriacao { get; set; }

    public DateTime DataAtualizacao { get; set; }

    public virtual ICollection<ComponenteFreteDuimp> ComponenteFreteDuimps { get; set; } = new List<ComponenteFreteDuimp>();

    public virtual ICollection<EmbalagemDuimp> EmbalagemDuimps { get; set; } = new List<EmbalagemDuimp>();

    public virtual Duimp IdDuimpNavigation { get; set; } = null!;

    public virtual ICollection<UnidadeDespacho> UnidadeDespachos { get; set; } = new List<UnidadeDespacho>();
}
