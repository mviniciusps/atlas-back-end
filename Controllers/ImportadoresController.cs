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

    // O serviço é fornecido pelo contêiner de dependências registrado no Program.cs.
    public ImportadoresController(
        AtlasComexContext context,
        BrasilApiCnpjService brasilApiCnpjService)
    {
        _context = context;
        _brasilApiCnpjService = brasilApiCnpjService;
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

        try
        {
            var resposta = await _brasilApiCnpjService.ConsultarAsync(
                cnpjNormalizado,
                cancellationToken
            );

            if (resposta is null)
            {
                return NotFound("CNPJ não encontrado.");
            }

            if (string.IsNullOrWhiteSpace(resposta.RazaoSocial))
            {
                return StatusCode(
                StatusCodes.Status502BadGateway,
                "A BrasilAPI não retornou a razão social.");
            }

            var dadosImportador = new ImportadorCnpjConsultaDto
            {
                Cnpj = cnpjNormalizado,
                RazaoSocial = resposta.RazaoSocial,
                Logradouro = resposta.Logradouro,
                NumeroEndereco = resposta.Numero,
                ComplementoEndereco = resposta.Complemento,
                Bairro = resposta.Bairro,
                Municipio = resposta.Municipio,
                Uf = resposta.Uf,
                Cep = resposta.Cep is null
                    ? null
                    : new string(resposta.Cep.Where(char.IsAsciiDigit).ToArray())
            };

            return Ok(dadosImportador);
        }
        catch (ArgumentException erro)
        {
            return BadRequest(erro.Message);
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            return StatusCode(
                StatusCodes.Status504GatewayTimeout,
                "A consulta à BrasilAPI excedeu o tempo limite.");
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                "Não foi possível consultar a BrasilAPI.");
        }
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
