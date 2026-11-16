using System.Net;
using atlas_back_end.Dtos.Importadores;
using atlas_back_end.Services.Integracoes;
using AtlasComex.Infrastructure.Models;
using AtlasComex.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace atlas_back_end.Controllers;

[ApiController]
[Route("api/importadores")]
public class ImportadoresController : ControllerBase
{
    private readonly AtlasComexContext _context;
    private readonly BrasilApiCnpjService _brasilApiCnpjService;
    private readonly ReceitaWsCnpjService _receitaWsCnpjService;
    private readonly ILogger<ImportadoresController> _logger;

    // O serviço é fornecido pelo contêiner de dependências registrado no Program.cs.
    public ImportadoresController(
        AtlasComexContext context,
        BrasilApiCnpjService brasilApiCnpjService,
        ReceitaWsCnpjService receitaWsCnpjService,
        ILogger<ImportadoresController> logger)
    {
        _context = context;
        _brasilApiCnpjService = brasilApiCnpjService;
        _receitaWsCnpjService = receitaWsCnpjService;
        _logger = logger;
    }

    // Lista importadores ativos e seus estabelecimentos ativos para montar as abas do frontend.
    [HttpGet]
    public async Task<ActionResult<List<ImportadorListaDto>>> Listar(
        CancellationToken cancellationToken)
    {
        var importadores = await _context.Importadors
            .AsNoTracking()
            .Where(importador => importador.Ativo)
            .OrderBy(importador => importador.RazaoSocial)
            .Select(importador => new ImportadorListaDto
            {
                IdImportador = importador.IdImportador,
                RazaoSocial = importador.RazaoSocial,
                NomeFantasia = importador.NomeFantasia,
                Estabelecimentos = importador.EstabelecimentoImportadors
                    //.Where(estabelecimento => estabelecimento.Ativo)
                    .Select(estabelecimento => new EstabelecimentoListaDto
                    {
                        IdEstabelecimento = estabelecimento.IdEstabelecimento,
                        Cnpj = estabelecimento.Cnpj
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return Ok(importadores);
    }

    [HttpGet("consultar-cnpj")]
    public async Task<ActionResult<ImportadorCnpjConsultaDto>> ConsultarCnpj(
        [FromQuery] string cnpj,
        CancellationToken cancellationToken)
    {
        var cnpjNormalizado = new string(
                cnpj.Where(char.IsAsciiLetterOrDigit).ToArray())
            .ToUpperInvariant();

        var formatoValido =
            cnpjNormalizado.Length == 14 &&
            cnpjNormalizado.Take(12).All(char.IsAsciiLetterOrDigit) &&
            cnpjNormalizado.Skip(12).All(char.IsAsciiDigit);

        if (!formatoValido)
        {
            return BadRequest(
                "Informe um CNPJ com 12 letras ou números seguidos de 2 dígitos.");
        }

        ImportadorCnpjConsultaDto? dadosImportador = null;

        // A BrasilAPI é a primeira fonte. Falhas temporárias levam à tentativa pela ReceitaWS.
        try
        {
            var respostaBrasilApi = await _brasilApiCnpjService.ConsultarAsync(
                cnpjNormalizado,
                cancellationToken);

            if (respostaBrasilApi is not null &&
                !string.IsNullOrWhiteSpace(respostaBrasilApi.RazaoSocial))
            {
                dadosImportador = new ImportadorCnpjConsultaDto
                {
                    Cnpj = cnpjNormalizado,
                    RazaoSocial = respostaBrasilApi.RazaoSocial,
                    Logradouro = respostaBrasilApi.Logradouro,
                    NumeroEndereco = respostaBrasilApi.Numero,
                    ComplementoEndereco = respostaBrasilApi.Complemento,
                    Bairro = respostaBrasilApi.Bairro,
                    Municipio = respostaBrasilApi.Municipio,
                    Uf = respostaBrasilApi.Uf,
                    Cep = respostaBrasilApi.Cep is null
                        ? null
                        : new string(respostaBrasilApi.Cep.Where(char.IsAsciiDigit).ToArray())
                };
            }
        }
        catch (ArgumentException erro)
        {
            return BadRequest(erro.Message);
        }
        catch (HttpRequestException erro)
        {
            _logger.LogWarning(
                erro,
                "Falha ao consultar o CNPJ {Cnpj} na BrasilAPI.",
                cnpjNormalizado);
        }
        catch (OperationCanceledException erro)
            when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                erro,
                "A consulta do CNPJ {Cnpj} na BrasilAPI excedeu o tempo limite.",
                cnpjNormalizado);
        }
        catch (System.Text.Json.JsonException erro)
        {
            _logger.LogWarning(
                erro,
                "A BrasilAPI retornou JSON inválido para o CNPJ {Cnpj}.",
                cnpjNormalizado);
        }

        // Tenta a ReceitaWS quando a BrasilAPI não trouxe dados utilizáveis.
        if (dadosImportador is null)
        {
            try
            {
                var respostaReceitaWs = await _receitaWsCnpjService.ConsultarAsync(
                    cnpjNormalizado,
                    cancellationToken);

                if (respostaReceitaWs is not null &&
                    string.Equals(
                        respostaReceitaWs.Status,
                        "OK",
                        StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(respostaReceitaWs.RazaoSocial))
                {
                    dadosImportador = new ImportadorCnpjConsultaDto
                    {
                        Cnpj = cnpjNormalizado,
                        RazaoSocial = respostaReceitaWs.RazaoSocial,
                        Logradouro = respostaReceitaWs.Logradouro,
                        NumeroEndereco = respostaReceitaWs.Numero,
                        ComplementoEndereco = respostaReceitaWs.Complemento,
                        Bairro = respostaReceitaWs.Bairro,
                        Municipio = respostaReceitaWs.Municipio,
                        Uf = respostaReceitaWs.Uf,
                        Cep = respostaReceitaWs.Cep is null
                            ? null
                            : new string(respostaReceitaWs.Cep.Where(char.IsAsciiDigit).ToArray())
                    };
                }
                else if (respostaReceitaWs is not null &&
                         !string.IsNullOrWhiteSpace(respostaReceitaWs.Mensagem))
                {
                    _logger.LogInformation(
                        "A ReceitaWS não retornou dados para o CNPJ {Cnpj}: {Mensagem}",
                        cnpjNormalizado,
                        respostaReceitaWs.Mensagem);
                }
            }
            catch (OperationCanceledException erro)
                when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(
                    erro,
                    "A consulta do CNPJ {Cnpj} excedeu o tempo limite na ReceitaWS.",
                    cnpjNormalizado);
                return StatusCode(
                    StatusCodes.Status504GatewayTimeout,
                    "A consulta de CNPJ excedeu o tempo limite.");
            }
            catch (HttpRequestException erro)
            {
                _logger.LogError(
                    erro,
                    "Falha ao consultar o CNPJ {Cnpj} na ReceitaWS.",
                    cnpjNormalizado);

                var limiteAtingido = erro.StatusCode == HttpStatusCode.TooManyRequests;
                var statusCode = limiteAtingido
                    ? StatusCodes.Status503ServiceUnavailable
                    : StatusCodes.Status502BadGateway;
                var mensagem = limiteAtingido
                    ? "O limite de consultas da ReceitaWS foi atingido. Aguarde e tente novamente."
                    : "Não foi possível consultar o CNPJ na BrasilAPI nem na ReceitaWS.";

                return StatusCode(statusCode, mensagem);
            }
            catch (System.Text.Json.JsonException erro)
            {
                _logger.LogError(
                    erro,
                    "A ReceitaWS retornou JSON inválido para o CNPJ {Cnpj}.",
                    cnpjNormalizado);
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    "A ReceitaWS retornou uma resposta inválida.");
            }
        }

        if (dadosImportador is null)
        {
            return NotFound(
                "O CNPJ não foi encontrado ou não está disponível nas bases consultadas.");
        }

        return Ok(dadosImportador);
    }

    // Cria o importador e seus CNPJs em uma única operação.
    [HttpPost]
    public async Task<IActionResult> Criar(
        [FromBody] ImportadorCadastroDto dados,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dados.RazaoSocial))
        {
            return BadRequest("A razão social do importador é obrigatória.");
        }

        var estabelecimentos = dados.Estabelecimentos
            .Select(estabelecimento => new
            {
                Dados = estabelecimento,
                Cnpj = new string(
                    estabelecimento.Cnpj
                        .Where(char.IsAsciiLetterOrDigit)
                        .Select(char.ToUpperInvariant)
                        .ToArray())
            })
            .ToList();

        if (estabelecimentos.Any(estabelecimento =>
                estabelecimento.Cnpj.Length != 14 ||
                !estabelecimento.Cnpj.Take(12).All(char.IsAsciiLetterOrDigit) ||
                !estabelecimento.Cnpj.Skip(12).All(char.IsAsciiDigit)))
        {
            return BadRequest(
                "Cada CNPJ deve ter 12 letras ou números, seguidos de 2 dígitos.");
        }

        var cnpjs = estabelecimentos
            .Select(estabelecimento => estabelecimento.Cnpj)
            .ToList();

        if (cnpjs.Distinct().Count() != cnpjs.Count)
        {
            return BadRequest("CNPJs duplicados não são permitidos.");
        }

        var cnpjJaCadastrado = await _context.EstabelecimentoImportadors
            .AnyAsync(
                estabelecimento => cnpjs.Contains(estabelecimento.Cnpj),
                cancellationToken);

        if (cnpjJaCadastrado)
        {
            return Conflict("Um ou mais CNPJs já estão cadastrados.");
        }

        var importador = new Importador
        {
            RazaoSocial = dados.RazaoSocial.Trim(),
            NomeFantasia = dados.NomeFantasia?.Trim(),
            Ativo = true
        };

        foreach (var estabelecimento in estabelecimentos)
        {
            importador.EstabelecimentoImportadors.Add(
                new EstabelecimentoImportador
                {
                    Cnpj = estabelecimento.Cnpj,
                    Logradouro = estabelecimento.Dados.Logradouro?.Trim(),
                    NumeroEndereco = estabelecimento.Dados.NumeroEndereco?.Trim(),
                    ComplementoEndereco = estabelecimento.Dados.ComplementoEndereco?.Trim(),
                    Bairro = estabelecimento.Dados.Bairro?.Trim(),
                    Municipio = estabelecimento.Dados.Municipio?.Trim(),
                    Uf = estabelecimento.Dados.Uf?.Trim().ToUpperInvariant(),
                    Cep = estabelecimento.Dados.Cep?.Trim()
                });
        }

        _context.Importadors.Add(importador);
        await _context.SaveChangesAsync(cancellationToken);

        return Created(
            "/api/importadores",
            new { importador.IdImportador });
    }
}
