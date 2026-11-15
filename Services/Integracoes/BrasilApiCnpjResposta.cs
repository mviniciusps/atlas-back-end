using System.Text.Json.Serialization;

namespace atlas_back_end.Services.Integracoes;

public sealed class BrasilApiCnpjResposta
{
    [JsonPropertyName("cnpj")]
    public string? Cnpj { get; set; }

    [JsonPropertyName("razao_social")]
    public string? RazaoSocial { get; set; }

    [JsonPropertyName("logradouro")]
    public string? Logradouro { get; set; }

    [JsonPropertyName("numero")]
    public string? Numero { get; set; }

    [JsonPropertyName("complemento")]
    public string? Complemento { get; set; }

    [JsonPropertyName("bairro")]
    public string? Bairro { get; set; }

    [JsonPropertyName("municipio")]
    public string? Municipio { get; set; }

    [JsonPropertyName("uf")]
    public string? Uf { get; set; }

    [JsonPropertyName("cep")]
    public string? Cep { get; set; }
}
