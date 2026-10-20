namespace atlas_back_end.Dtos.Processos;

public sealed class ProcessoListaDto
{
    public long IdProcesso { get; set; }
    public int IdImportador { get; set; }
    public string RazaoSocialImportador { get; set; } = string.Empty;
    public int IdEstabelecimento { get; set; }
    public string Cnpj { get; set; } = string.Empty;

    public string NumeroProcesso { get; set; } = string.Empty;
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
}