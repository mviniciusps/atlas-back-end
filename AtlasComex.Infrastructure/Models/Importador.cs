using System;
using System.Collections.Generic;

namespace AtlasComex.Infrastructure.Models;

public partial class Importador
{
    public int IdImportador { get; set; }

    public string RazaoSocial { get; set; } = null!;

    public string? NomeFantasia { get; set; }

    public bool Ativo { get; set; }

    public DateTime DataCriacao { get; set; }

    public virtual ICollection<EstabelecimentoImportador> EstabelecimentoImportadors { get; set; } = new List<EstabelecimentoImportador>();
}
