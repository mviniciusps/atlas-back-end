using System;
using System.Collections.Generic;

namespace AtlasComex.Infrastructure.Models;

public partial class UnidadeDespacho
{
    public long IdUnidadeDespachoDuimp { get; set; }

    public long IdDuimp { get; set; }

    public string CodigoUnidadeDespacho { get; set; } = null!;

    public virtual CargaDuimp IdDuimpNavigation { get; set; } = null!;
}
