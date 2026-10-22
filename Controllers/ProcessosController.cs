using atlas_back_end.Dtos.Processos;
using AtlasComex.Infrastructure.Models;
using AtlasComex.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace atlas_back_end.Controllers;

[ApiController]
[Route("api/processos")]
public class ProcessosController : ControllerBase
{
    private readonly AtlasComexContext _context;

    public ProcessosController(AtlasComexContext context)
    {
        _context = context;
    }

    // Lista os processos; o filtro permite carregar os processos de um importador.
    [HttpGet]
    public async Task<ActionResult<List<ProcessoListaDto>>> Listar(
        [FromQuery] int? idImportador,
        CancellationToken cancellationToken)
    {
        var consulta = _context.Processos
            .AsNoTracking();

        if (idImportador.HasValue)
        {
            consulta = consulta.Where(processo =>
                processo.IdEstabelecimentoNavigation.IdImportador
                == idImportador.Value);
        }

        var processos = await consulta
            .OrderBy(processo => processo.PrevisaoChegada)
            .ThenBy(processo => processo.NumeroProcesso)
            .Select(processo => new ProcessoListaDto
            {
                IdProcesso = processo.IdProcesso,
                IdImportador = processo.IdEstabelecimentoNavigation.IdImportador,
                RazaoSocialImportador =
                    processo.IdEstabelecimentoNavigation
                        .IdImportadorNavigation.RazaoSocial,
                IdEstabelecimento =
                    processo.IdEstabelecimentoNavigation.IdEstabelecimento,
                Cnpj = processo.IdEstabelecimentoNavigation.Cnpj,
                NumeroProcesso = processo.NumeroProcesso,
                ReferenciaCliente = processo.ReferenciaCliente,
                PrevisaoChegada = processo.PrevisaoChegada,
                Chegada = processo.Chegada,
                Exportador = processo.Exportador,
                Observacoes = processo.Observacoes,
                ConhecimentoMaster = processo.ConhecimentoMaster,
                ConhecimentoHouse = processo.ConhecimentoHouse,
                Modal = processo.Modal,
                CeMercanteRuc = processo.CeMercanteRuc,
                Duimp = processo.Duimp,
                DataRegistro = processo.DataRegistro,
                Desembaraco = processo.Desembaraco,
                RecintoAlfandegado = processo.RecintoAlfandegado,
                Situacao = processo.Situacao
            })
            .ToListAsync(cancellationToken);

        return Ok(processos);
    }

    // Cria um processo para um estabelecimento ativo do importador.
    [HttpPost]
    public async Task<IActionResult> Criar(
        [FromBody] ProcessoCadastroDto dados,
        CancellationToken cancellationToken)
    {
        // Confirma que o estabelecimento informado existe e está ativo.
        var estabelecimento = await _context.EstabelecimentoImportadors
            .AsNoTracking()
            .Where(item =>
                item.IdEstabelecimento == dados.IdEstabelecimento
                && item.Ativo)
            .Select(item => new
            {
                item.IdEstabelecimento,
                item.IdImportador
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (estabelecimento is null)
        {
            return NotFound("Estabelecimento ativo não encontrado.");
        }

        var numeroProcesso = dados.NumeroProcesso.Trim();

        // Evita repetir o número dentro do mesmo estabelecimento.
        var processoJaExiste = await _context.Processos
            .AnyAsync(
                processo =>
                    processo.IdEstabelecimento == dados.IdEstabelecimento
                    && processo.NumeroProcesso == numeroProcesso,
                cancellationToken);

        if (processoJaExiste)
        {
            return Conflict(
                "Já existe um processo com esse número para o estabelecimento.");
        }

        // Os campos que dependem de integrações externas ficam sem preenchimento aqui.
        var processoNovo = new Processo
        {
            IdEstabelecimento = estabelecimento.IdEstabelecimento,
            NumeroProcesso = numeroProcesso,
            ReferenciaCliente = dados.ReferenciaCliente?.Trim(),
            PrevisaoChegada = dados.PrevisaoChegada,
            Exportador = dados.Exportador?.Trim(),
            Observacoes = dados.Observacoes?.Trim(),
            ConhecimentoMaster = dados.ConhecimentoMaster?.Trim(),
            ConhecimentoHouse = dados.ConhecimentoHouse?.Trim(),
            Modal = dados.Modal?.Trim(),
            Desembaraco = dados.Desembaraco,
            RecintoAlfandegado = dados.RecintoAlfandegado?.Trim(),
            Situacao = dados.Situacao.Trim()
        };

        _context.Processos.Add(processoNovo);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(Listar),
            new { idImportador = estabelecimento.IdImportador },
            new { idProcesso = processoNovo.IdProcesso });
    }

    [HttpPut("{idProcesso:long}")]
    public async Task<IActionResult> Atualizar(
        long idProcesso,
        [FromBody] ProcessoAtualizacaoDto dados,
        CancellationToken cancellationToken)
    {
        var processo = await _context.Processos.FirstOrDefaultAsync(
            item => item.IdProcesso == idProcesso,
            cancellationToken);

        if(processo is null){
            return NotFound("Processo não encontrado.");
        }

        //Atribuição de processos editaveis, automaticos ficam intactos
        processo?.ReferenciaCliente = dados.ReferenciaCliente?.Trim();
        processo?.PrevisaoChegada = dados.PrevisaoChegada;
        processo?.Exportador = dados.Exportador?.Trim();
        processo?.Observacoes = dados.Observacoes?.Trim();
        processo?.ConhecimentoMaster = dados.ConhecimentoMaster?.Trim();
        processo?.ConhecimentoHouse = dados.ConhecimentoHouse?.Trim();
        processo?.Modal = dados.Modal?.Trim();
        processo?.Desembaraco = dados.Desembaraco;
        processo?.RecintoAlfandegado = dados.RecintoAlfandegado?.Trim();
        processo?.Situacao = dados.Situacao.Trim();

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
