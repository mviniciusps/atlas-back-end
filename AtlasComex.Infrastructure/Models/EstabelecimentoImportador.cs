using System;
using System.Collections.Generic;

namespace AtlasComex.Infrastructure.Models;

public partial class EstabelecimentoImportador
{
    public int IdEstabelecimento { get; set; }

    public int IdImportador { get; set; }

    public string Cnpj { get; set; } = null!;

    public string? NomeEstabelecimento { get; set; }

    public bool Ativo { get; set; }

    public virtual Importador IdImportadorNavigation { get; set; } = null!;

    public virtual ICollection<Processo> Processos { get; set; } = new List<Processo>();
}
