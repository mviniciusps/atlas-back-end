namespace atlas_back_end.Dtos.Documentos
{
    public class DocumentoProcessoDto
    {
        public int IdDocumento { get; set; }
        public long IdProcesso { get; set; }
        public string TipoDocumento { get; set; } = string.Empty;
        public string NomeArquivo { get; set; } = string.Empty;
        public string? ContentType { get; set; }
        public long TamanhoBytes { get; set; }
        public bool IncluirNaDuimp { get; set; }
        public DateTime DataUpload { get; set; }
    }
}
