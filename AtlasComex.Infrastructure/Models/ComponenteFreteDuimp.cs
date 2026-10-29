using System;
using System.Collections.Generic;

namespace AtlasComex.Infrastructure.Models;

public partial class ComponenteFreteDuimp
{
    public long IdComponenteFreteDuimp { get; set; }

    public long IdDuimp { get; set; }

    public string? Pagamento { get; set; }

    public string? Tipo { get; set; }

    public string? Moeda { get; set; }

    public decimal? ValorMoeda { get; set; }

    public virtual CargaDuimp IdDuimpNavigation { get; set; } = null!;
}
