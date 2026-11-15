using System;
using System.Collections.Generic;

namespace AtlasComex.Infrastructure.Models;

public partial class EstabelecimentoImportador
{
    public int IdEstabelecimento { get; set; }

    public int IdImportador { get; set; }

    public string Cnpj { get; set; } = null!;

    public string? Logradouro { get; set; }

    public string? NumeroEndereco { get; set; }

    public string? ComplementoEndereco { get; set; }

    public string? Bairro { get; set; }

    public string? Municipio { get; set; }

    public string? Uf { get; set; }

    public string? Cep { get; set; }

    public virtual Importador IdImportadorNavigation { get; set; } = null!;

    public virtual ICollection<Processo> Processos { get; set; } = new List<Processo>();
}
