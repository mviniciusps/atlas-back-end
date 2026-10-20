using atlas_back_end.Dtos.Processos;
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
}
