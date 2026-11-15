namespace atlas_back_end.Dtos.Importadores;

public sealed class ImportadorCnpjConsultaDto
{
    public string Cnpj { get; set; } = string.Empty;
    public string RazaoSocial { get; set; } = string.Empty;
    public string? Logradouro { get; set; }
    public string? NumeroEndereco { get; set; }
    public string? ComplementoEndereco { get; set; }
    public string? Bairro { get; set; }
    public string? Municipio { get; set; }
    public string? Uf { get; set; }
    public string? Cep { get; set; }
}
