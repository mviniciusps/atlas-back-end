using System;
using System.Collections.Generic;

namespace AtlasComex.Infrastructure.Models;

public partial class Processo
{
    public long IdProcesso { get; set; }

    public int IdEstabelecimento { get; set; }

    public string NumeroProcesso { get; set; } = null!;

    public string? ReferenciaCliente { get; set; }

    public DateOnly? PrevisaoChegada { get; set; }

    public DateOnly? Chegada { get; set; }

    public string? Exportador { get; set; }

    public string? Observacoes { get; set; }

    public string? ConhecimentoMaster { get; set; }

    public string? ConhecimentoHouse { get; set; }

    public string? Modal { get; set; }

    public string? CeMercanteRuc { get; set; }

    public string? Duimp { get; set; }

    public DateOnly? DataRegistro { get; set; }

    public DateOnly? Desembaraco { get; set; }

    public string? RecintoAlfandegado { get; set; }

    public string? Situacao { get; set; }

    public DateTime DataCriacao { get; set; }

    public virtual ICollection<DocumentoProcesso> DocumentoProcessos { get; set; } = new List<DocumentoProcesso>();

    public virtual Duimp? DuimpNavigation { get; set; }

    public virtual EstabelecimentoImportador IdEstabelecimentoNavigation { get; set; } = null!;
}
