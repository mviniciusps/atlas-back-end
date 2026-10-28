using atlas_back_end.Dtos.Duimp;
using AtlasComex.Infrastructure.Models;
using AtlasComex.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace atlas_back_end.Controllers;

[ApiController]
[Route("api/processos/{idProcesso:long}/duimp")]
public sealed class DuimpController : ControllerBase
{
    private readonly AtlasComexContext _context;

    public DuimpController(AtlasComexContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<DuimpRascunhoDto>> ObterPorProcesso(
        long idProcesso,
        CancellationToken cancellationToken)
    {
        if (idProcesso <= 0) return BadRequest("O identificador do processo deve ser maior que zero.");

        // Primeiro, obtenha o Duimp relacionado ao processo
        var duimp = await _context.Duimps
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.IdProcesso == idProcesso, cancellationToken);

        if (duimp == null)
            return NotFound("Duimp não encontrado para o processo informado.");

        // Agora, obtenha os itens relacionados ao Duimp
        var itens = await _context.ItemDuimps
            .AsNoTracking()
            .Where(item => item.IdDuimp == duimp.IdDuimp)
            .OrderBy(item => item.NumeroItem)
            .ToListAsync(cancellationToken);

        var resposta = new DuimpRascunhoDto
        {
            TipoPessoaImportador = duimp.TipoPessoaImportador,
            DocumentoImportador = duimp.DocumentoImportador,
            NomeImportador = duimp.NomeImportador,

            LogradouroImportador = duimp.LogradouroImportador,
            NumeroEnderecoImportador = duimp.NumeroEnderecoImportador,
            ComplementoEnderecoImportador = duimp.ComplementoEnderecoImportador,
            BairroImportador = duimp.BairroImportador,
            CodigoMunicipioImportador = duimp.CodigoMunicipioImportador,
            MunicipioImportador = duimp.MunicipioImportador,
            UfImportador = duimp.UfImportador,
            CepImportador = duimp.CepImportador,
            PaisImportador = duimp.PaisImportador,

            InformacoesComplementaresGeradas = duimp.InformacoesComplementaresGeradas,
            InformacoesComplementaresAdicionais = duimp.InformacoesComplementaresAdicionais,

            Itens = itens.Select(MapearItemParaDto).ToList()
        };

        return Ok(resposta);
    }

    [HttpPut]
    public async Task<IActionResult> SalvarRascunho(
        long idProcesso,
        [FromBody] DuimpRascunhoDto dados,
        CancellationToken cancellationToken)
    {
        if (idProcesso <= 0)
            return BadRequest("O identificador do processo deve ser maior que zero.");

        if (dados.Itens is null)
            return BadRequest("A lista de itens não pode ser nula.");

        if (dados.Itens.GroupBy(item => item.NumeroItem).Any(grupo => grupo.Count() > 1))
            return BadRequest("A lista contém números de item duplicados.");

        var processoExiste = await _context.Processos
            .AnyAsync(processo => processo.IdProcesso == idProcesso, cancellationToken);

        if (!processoExiste)
            return NotFound("Processo não encontrado.");

        await using var transacao = await _context.Database
            .BeginTransactionAsync(cancellationToken);

        var duimp = await _context.Duimps
            .FirstOrDefaultAsync(item => item.IdProcesso == idProcesso, cancellationToken);

        if (duimp is null)
        {
            duimp = new Duimp
            {
                IdProcesso = idProcesso,
                StatusRascunho = "Rascunho",
                StatusIntegracaoPortal = "NaoEnviada"
            };

            _context.Duimps.Add(duimp);
        }

        duimp.TipoPessoaImportador = dados.TipoPessoaImportador;
        duimp.DocumentoImportador = dados.DocumentoImportador;
        duimp.NomeImportador = dados.NomeImportador;
        duimp.LogradouroImportador = dados.LogradouroImportador;
        duimp.NumeroEnderecoImportador = dados.NumeroEnderecoImportador;
        duimp.ComplementoEnderecoImportador = dados.ComplementoEnderecoImportador;
        duimp.BairroImportador = dados.BairroImportador;
        duimp.CodigoMunicipioImportador = dados.CodigoMunicipioImportador;
        duimp.MunicipioImportador = dados.MunicipioImportador;
        duimp.UfImportador = dados.UfImportador;
        duimp.CepImportador = dados.CepImportador;
        duimp.PaisImportador = dados.PaisImportador;
        duimp.InformacoesComplementaresGeradas = dados.InformacoesComplementaresGeradas;
        duimp.InformacoesComplementaresAdicionais = dados.InformacoesComplementaresAdicionais;
        duimp.DataAtualizacao = DateTime.UtcNow;

        // Salva o cabeçalho para obter o IdDuimp antes de gravar os itens.
        await _context.SaveChangesAsync(cancellationToken);

        var itensExistentes = await _context.ItemDuimps
            .Where(item => item.IdDuimp == duimp.IdDuimp)
            .ToListAsync(cancellationToken);

        var itensPorNumero = itensExistentes.ToDictionary(item => item.NumeroItem);
        var numerosRecebidos = dados.Itens
            .Select(item => item.NumeroItem)
            .ToHashSet();

        var itensRemovidos = itensExistentes
            .Where(item => !numerosRecebidos.Contains(item.NumeroItem))
            .ToList();

        _context.ItemDuimps.RemoveRange(itensRemovidos);

        foreach (var itemDto in dados.Itens)
        {
            if (!itensPorNumero.TryGetValue(itemDto.NumeroItem, out var item))
            {
                item = new ItemDuimp
                {
                    IdDuimp = duimp.IdDuimp
                };

                _context.ItemDuimps.Add(item);
            }

            AtualizarItem(item, itemDto);
            item.DataAtualizacao = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        await transacao.CommitAsync(cancellationToken);

        return NoContent();
    }

    private static ItemDuimpDto MapearItemParaDto(ItemDuimp item)
    {
        return new ItemDuimpDto
        {
            NumeroItem = item.NumeroItem,
            NumeroAdicao = item.NumeroAdicao,
            CaracterizacaoImportacao = item.CaracterizacaoImportacao,
            NomeProduto = item.NomeProduto,
            VersaoProduto = item.VersaoProduto,
            Ncm = item.Ncm,
            PaisOrigem = item.PaisOrigem,
            CodigoFabricante = item.CodigoFabricante,
            NomeFabricante = item.NomeFabricante,
            VersaoFabricante = item.VersaoFabricante,
            EnderecoFabricante = item.EnderecoFabricante,
            ExportadorEhFabricante = item.ExportadorEhFabricante,
            PaisExportador = item.PaisExportador,
            CodigoExportador = item.CodigoExportador,
            NomeExportador = item.NomeExportador,
            VersaoExportador = item.VersaoExportador,
            EnderecoExportador = item.EnderecoExportador,
            UfConsumo = item.UfConsumo,
            CodigoMunicipioConsumo = item.CodigoMunicipioConsumo,
            MunicipioConsumo = item.MunicipioConsumo,
            Aplicacao = item.Aplicacao,
            CondicaoMercadoria = item.CondicaoMercadoria,
            UnidadeMedidaEstatistica = item.UnidadeMedidaEstatistica,
            QuantidadeMedidaEstatistica = item.QuantidadeMedidaEstatistica,
            PesoLiquido = item.PesoLiquido,
            UnidadeComercializada = item.UnidadeComercializada,
            QuantidadeUnidadeComercializada = item.QuantidadeUnidadeComercializada,
            MoedaNegociada = item.MoedaNegociada,
            ValorUnitarioCondicaoVenda = item.ValorUnitarioCondicaoVenda,
            NumeroSerie = item.NumeroSerie,
            DetalhamentoProduto = item.DetalhamentoProduto,
            MetodoValoracao = item.MetodoValoracao,
            Incoterm = item.Incoterm,
            CoberturaCambial = item.CoberturaCambial
        };
    }

    private static void AtualizarItem(ItemDuimp destino, ItemDuimpDto origem)
    {
        destino.NumeroItem = origem.NumeroItem;
        destino.NumeroAdicao = origem.NumeroAdicao;
        destino.CaracterizacaoImportacao = origem.CaracterizacaoImportacao;
        destino.NomeProduto = origem.NomeProduto;
        destino.VersaoProduto = origem.VersaoProduto;
        destino.Ncm = origem.Ncm;
        destino.PaisOrigem = origem.PaisOrigem;
        destino.CodigoFabricante = origem.CodigoFabricante;
        destino.NomeFabricante = origem.NomeFabricante;
        destino.VersaoFabricante = origem.VersaoFabricante;
        destino.EnderecoFabricante = origem.EnderecoFabricante;
        destino.ExportadorEhFabricante = origem.ExportadorEhFabricante;
        destino.PaisExportador = origem.PaisExportador;
        destino.CodigoExportador = origem.CodigoExportador;
        destino.NomeExportador = origem.NomeExportador;
        destino.VersaoExportador = origem.VersaoExportador;
        destino.EnderecoExportador = origem.EnderecoExportador;
        destino.UfConsumo = origem.UfConsumo;
        destino.CodigoMunicipioConsumo = origem.CodigoMunicipioConsumo;
        destino.MunicipioConsumo = origem.MunicipioConsumo;
        destino.Aplicacao = origem.Aplicacao;
        destino.CondicaoMercadoria = origem.CondicaoMercadoria;
        destino.UnidadeMedidaEstatistica = origem.UnidadeMedidaEstatistica;
        destino.QuantidadeMedidaEstatistica = origem.QuantidadeMedidaEstatistica;
        destino.PesoLiquido = origem.PesoLiquido;
        destino.UnidadeComercializada = origem.UnidadeComercializada;
        destino.QuantidadeUnidadeComercializada = origem.QuantidadeUnidadeComercializada;
        destino.MoedaNegociada = origem.MoedaNegociada;
        destino.ValorUnitarioCondicaoVenda = origem.ValorUnitarioCondicaoVenda;
        destino.NumeroSerie = origem.NumeroSerie;
        destino.DetalhamentoProduto = origem.DetalhamentoProduto;
        destino.MetodoValoracao = origem.MetodoValoracao;
        destino.Incoterm = origem.Incoterm;
        destino.CoberturaCambial = origem.CoberturaCambial;
    }
}
