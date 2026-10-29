using System;
using System.Collections.Generic;

namespace AtlasComex.Infrastructure.Models;

public partial class Duimp
{
    public long IdDuimp { get; set; }

    public long IdProcesso { get; set; }

    public string? TipoPessoaImportador { get; set; }

    public string? DocumentoImportador { get; set; }

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

    public string StatusRascunho { get; set; } = null!;

    public string StatusIntegracaoPortal { get; set; } = null!;

    public DateTime DataCriacao { get; set; }

    public DateTime DataAtualizacao { get; set; }

    public virtual CargaDuimp? CargaDuimp { get; set; }

    public virtual Processo IdProcessoNavigation { get; set; } = null!;

    public virtual ICollection<ItemDuimp> ItemDuimps { get; set; } = new List<ItemDuimp>();
}
