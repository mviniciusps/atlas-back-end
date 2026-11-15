using System.Net;
using System.Net.Http.Json;

namespace atlas_back_end.Services.Integracoes;

public sealed class BrasilApiCnpjService
{
    private readonly HttpClient _httpClient;

    public BrasilApiCnpjService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BrasilApiCnpjResposta?> ConsultarAsync(
        string cnpj,
        CancellationToken cancellationToken)
    {
        using var resposta = await _httpClient.GetAsync(
            cnpj,
            cancellationToken);

        if (resposta.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        resposta.EnsureSuccessStatusCode();

        return await resposta.Content
            .ReadFromJsonAsync<BrasilApiCnpjResposta>(
                cancellationToken: cancellationToken);
    }
}