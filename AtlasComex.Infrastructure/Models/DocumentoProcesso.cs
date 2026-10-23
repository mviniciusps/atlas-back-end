namespace AtlasComex.Infrastructure.Models;

public partial class DocumentoProcesso
{
    public int IdDocumento { get; set; }

    // SQL Server BIGINT corresponde ao tipo long no C#.
    public long IdProcesso { get; set; }

    public string TipoDocumento { get; set; } = null!;

    public string NomeArquivo { get; set; } = null!;

    public string? ContentType { get; set; }

    public long TamanhoBytes { get; set; }

    public string CaminhoArmazenamento { get; set; } = null!;

    public bool IncluirNaDuimp { get; set; }

    public DateTime DataUpload { get; set; }

    public virtual Processo IdProcessoNavigation { get; set; } = null!;
}
