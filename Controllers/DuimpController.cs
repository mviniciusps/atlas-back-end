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

        var carga = await _context.CargaDuimps
            .AsNoTracking()
            .AsSplitQuery()
            .Include(item => item.UnidadeDespachos)
            .Include(item => item.ComponenteFreteDuimps)
            .Include(item => item.EmbalagemDuimps)
            .FirstOrDefaultAsync(item => item.IdDuimp == duimp.IdDuimp, cancellationToken);

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

            Carga = carga is null ? null : MapearCargaParaDto(carga),
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

        if (dados.Carga is not null)
        {
            if (dados.Carga.UnidadesDespacho is null ||
                dados.Carga.ComponentesFrete is null ||
                dados.Carga.Embalagens is null)
            {
                return BadRequest("As listas da carga não podem ser nulas.");
            }

            if (dados.Carga.TipoIdentificacaoCarga == "CE" &&
                (dados.Carga.IdentificacaoCarga?.Length ?? 0) > 15)
            {
                return BadRequest("A identificação da carga do tipo CE deve ter até 15 caracteres.");
            }
        }

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

        if (dados.Carga is not null)
        {
            await AtualizarCargaAsync(duimp.IdDuimp, dados.Carga, cancellationToken);
        }

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

    private static CargaDuimpDto MapearCargaParaDto(CargaDuimp carga)
    {
        return new CargaDuimpDto
        {
            TipoIdentificacaoCarga = carga.TipoIdentificacaoCarga,
            IdentificacaoCarga = carga.IdentificacaoCarga,
            LocalEmbarque = carga.LocalEmbarque,
            DataConhecimento = carga.DataConhecimento,
            PaisProcedencia = carga.PaisProcedencia,
            PesoBruto = carga.PesoBruto,
            PesoLiquido = carga.PesoLiquido,
            CodigoMoedaFrete = carga.CodigoMoedaFrete,
            ValorFreteMoeda = carga.ValorFreteMoeda,
            ValorFreteReais = carga.ValorFreteReais,
            ValorAfrmmTum = carga.ValorAfrmmTum,
            AfrmmTumPago = carga.AfrmmTumPago,
            CodigoMoedaSeguro = carga.CodigoMoedaSeguro,
            ValorSeguroMoeda = carga.ValorSeguroMoeda,
            ValorSeguroReais = carga.ValorSeguroReais,
            UnidadesDespacho = carga.UnidadeDespachos
                .Select(unidade => unidade.CodigoUnidadeDespacho)
                .ToList(),
            ComponentesFrete = carga.ComponenteFreteDuimps
                .Select(componente => new ComponenteFreteDuimpDto
                {
                    Pagamento = componente.Pagamento,
                    Tipo = componente.Tipo,
                    Moeda = componente.Moeda,
                    ValorMoeda = componente.ValorMoeda
                })
                .ToList(),
            Embalagens = carga.EmbalagemDuimps
                .Select(embalagem => new EmbalagemDuimpDto
                {
                    Item = embalagem.Item,
                    Tipo = embalagem.Tipo,
                    Quantidade = embalagem.Quantidade
                })
                .ToList()
        };
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

    private async Task AtualizarCargaAsync(
        long idDuimp,
        CargaDuimpDto dados,
        CancellationToken cancellationToken)
    {
        var carga = await _context.CargaDuimps
            .FirstOrDefaultAsync(item => item.IdDuimp == idDuimp, cancellationToken);

        if (carga is null)
        {
            carga = new CargaDuimp
            {
                IdDuimp = idDuimp
            };

            _context.CargaDuimps.Add(carga);
        }

        carga.TipoIdentificacaoCarga = dados.TipoIdentificacaoCarga;
        carga.IdentificacaoCarga = dados.IdentificacaoCarga;
        carga.LocalEmbarque = dados.LocalEmbarque;
        carga.DataConhecimento = dados.DataConhecimento;
        carga.PaisProcedencia = dados.PaisProcedencia;
        carga.PesoBruto = dados.PesoBruto;
        carga.PesoLiquido = dados.PesoLiquido;
        carga.CodigoMoedaFrete = dados.CodigoMoedaFrete;
        carga.ValorFreteMoeda = dados.ValorFreteMoeda;
        carga.ValorFreteReais = dados.ValorFreteReais;
        carga.ValorAfrmmTum = dados.ValorAfrmmTum;
        carga.AfrmmTumPago = dados.AfrmmTumPago;
        carga.CodigoMoedaSeguro = dados.CodigoMoedaSeguro;
        carga.ValorSeguroMoeda = dados.ValorSeguroMoeda;
        carga.ValorSeguroReais = dados.ValorSeguroReais;
        carga.DataAtualizacao = DateTime.UtcNow;

        // Persiste a carga antes das listas que dependem dela.
        await _context.SaveChangesAsync(cancellationToken);

        var unidadesExistentes = await _context.UnidadeDespachos
            .Where(item => item.IdDuimp == idDuimp)
            .ToListAsync(cancellationToken);

        var componentesExistentes = await _context.ComponenteFreteDuimps
            .Where(item => item.IdDuimp == idDuimp)
            .ToListAsync(cancellationToken);

        var embalagensExistentes = await _context.EmbalagemDuimps
            .Where(item => item.IdDuimp == idDuimp)
            .ToListAsync(cancellationToken);

        _context.UnidadeDespachos.RemoveRange(unidadesExistentes);
        _context.ComponenteFreteDuimps.RemoveRange(componentesExistentes);
        _context.EmbalagemDuimps.RemoveRange(embalagensExistentes);

        // As listas recebidas substituem as listas anteriores nesta atualização.
        await _context.SaveChangesAsync(cancellationToken);

        var codigosUnidade = dados.UnidadesDespacho
            .Where(codigo => !string.IsNullOrWhiteSpace(codigo))
            .Select(codigo => codigo.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(codigo => new UnidadeDespacho
            {
                IdDuimp = idDuimp,
                CodigoUnidadeDespacho = codigo
            })
            .ToList();

        _context.UnidadeDespachos.AddRange(codigosUnidade);

        var componentesNovos = dados.ComponentesFrete
            .Select(componente => new ComponenteFreteDuimp
            {
                IdDuimp = idDuimp,
                Pagamento = componente.Pagamento,
                Tipo = componente.Tipo,
                Moeda = componente.Moeda,
                ValorMoeda = componente.ValorMoeda
            })
            .ToList();

        _context.ComponenteFreteDuimps.AddRange(componentesNovos);

        var embalagensNovas = dados.Embalagens
            .Select(embalagem => new EmbalagemDuimp
            {
                IdDuimp = idDuimp,
                Item = embalagem.Item,
                Tipo = embalagem.Tipo,
                Quantidade = embalagem.Quantidade
            })
            .ToList();

        _context.EmbalagemDuimps.AddRange(embalagensNovas);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
