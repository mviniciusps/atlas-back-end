namespace atlas_back_end.Dtos.Importadores;

public sealed class ImportadorListaDto
{
    public int IdImportador { get; set; }
    public string RazaoSocial { get; set; } = string.Empty;
    public string? NomeFantasia { get; set; }
    public List<EstabelecimentoListaDto> Estabelecimentos { get; set; } = [];
}

public sealed class EstabelecimentoListaDto
{
    public int IdEstabelecimento { get; set; }
    public string Cnpj { get; set; } = string.Empty;
    public string? NomeEstabelecimento { get; set; }
}
