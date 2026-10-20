using atlas_back_end.Dtos.Importadores;
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

    public ImportadoresController(AtlasComexContext context)
    {
        _context = context;
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
                    .Where(estabelecimento => estabelecimento.Ativo)
                    .Select(estabelecimento => new EstabelecimentoListaDto
                    {
                        IdEstabelecimento = estabelecimento.IdEstabelecimento,
                        Cnpj = estabelecimento.Cnpj,
                        NomeEstabelecimento = estabelecimento.NomeEstabelecimento
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return Ok(importadores);
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
                        .Where(char.IsDigit)
                        .ToArray())
            })
            .ToList();

        if (estabelecimentos.Any(estabelecimento => estabelecimento.Cnpj.Length != 14))
        {
            return BadRequest("Cada CNPJ deve conter 14 dígitos.");
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
            NomeFantasia = dados.NomeFantasia?.Trim()
        };

        foreach (var estabelecimento in estabelecimentos)
        {
            importador.EstabelecimentoImportadors.Add(
                new EstabelecimentoImportador
                {
                    Cnpj = estabelecimento.Cnpj,
                    NomeEstabelecimento =
                        estabelecimento.Dados.NomeEstabelecimento?.Trim()
                });
        }

        _context.Importadors.Add(importador);
        await _context.SaveChangesAsync(cancellationToken);

        return Created(
            "/api/importadores",
            new { importador.IdImportador });
    }
}
