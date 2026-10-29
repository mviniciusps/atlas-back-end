using System;
using System.Collections.Generic;

namespace AtlasComex.Infrastructure.Models;

public partial class EmbalagemDuimp
{
    public long IdEmbalagemDuimp { get; set; }

    public long IdDuimp { get; set; }

    public string? Item { get; set; }

    public string? Tipo { get; set; }

    public decimal? Quantidade { get; set; }

    public virtual CargaDuimp IdDuimpNavigation { get; set; } = null!;
}
