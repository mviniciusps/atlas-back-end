using atlas_back_end.Dtos.Documentos;
using AtlasComex.Infrastructure.Models;
using AtlasComex.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace atlas_back_end.Controllers;

[ApiController]
[Route("api/processos/{idProcesso:long}/documentos")]
public class DocumentosController : ControllerBase
{
    private const long TamanhoMaximoArquivo = 25 * 1024 * 1024;

    private static readonly HashSet<string> ExtensoesPermitidas =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf",
            ".xml",
            ".xlsx",
            ".xls",
            ".jpg",
            ".jpeg",
            ".png",
            ".tif",
            ".tiff",
        };

    private readonly AtlasComexContext _context;
    private readonly IWebHostEnvironment _ambiente;

    public DocumentosController(
        AtlasComexContext context,
        IWebHostEnvironment ambiente)
    {
        _context = context;
        _ambiente = ambiente;
    }

    [HttpGet]
    public async Task<ActionResult<List<DocumentoProcessoDto>>> Listar(
        long idProcesso,
        CancellationToken cancellationToken)
    {
        var processoExiste = await _context.Processos
            .AnyAsync(
                processo => processo.IdProcesso == idProcesso,
                cancellationToken);

        if (!processoExiste)
        {
            return NotFound("Processo não encontrado.");
        }

        var documentos = await _context.DocumentoProcessos
            .AsNoTracking()
            .Where(documento => documento.IdProcesso == idProcesso)
            .OrderByDescending(documento => documento.DataUpload)
            .Select(documento => new DocumentoProcessoDto
            {
                IdDocumento = documento.IdDocumento,
                IdProcesso = documento.IdProcesso,
                TipoDocumento = documento.TipoDocumento,
                NomeArquivo = documento.NomeArquivo,
                ContentType = documento.ContentType,
                TamanhoBytes = documento.TamanhoBytes,
                IncluirNaDuimp = documento.IncluirNaDuimp,
                DataUpload = documento.DataUpload,
            })
            .ToListAsync(cancellationToken);

        return Ok(documentos);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(26 * 1024 * 1024)]
    public async Task<ActionResult<DocumentoProcessoDto>> Enviar(
        long idProcesso,
        [FromForm] DocumentoProcessoUploadDto dados,
        CancellationToken cancellationToken)
    {
        var arquivo = dados.Arquivo;

        if (arquivo is null || arquivo.Length == 0)
        {
            return BadRequest("Selecione um arquivo.");
        }

        if (arquivo.Length > TamanhoMaximoArquivo)
        {
            return BadRequest("O arquivo não pode ultrapassar 25 MB.");
        }

        if (string.IsNullOrWhiteSpace(dados.TipoDocumento))
        {
            return BadRequest("Informe o tipo do documento.");
        }

        var processoExiste = await _context.Processos
            .AnyAsync(
                processo => processo.IdProcesso == idProcesso,
                cancellationToken);

        if (!processoExiste)
        {
            return NotFound("Processo não encontrado.");
        }

        var nomeOriginal = Path.GetFileName(
            arquivo.FileName.Replace('\\', '/'));

        if (string.IsNullOrWhiteSpace(nomeOriginal) || nomeOriginal.Length > 260)
        {
            return BadRequest("O nome do arquivo deve ter entre 1 e 260 caracteres.");
        }

        var extensao = Path.GetExtension(nomeOriginal);

        if (!ExtensoesPermitidas.Contains(extensao))
        {
            return BadRequest("Tipo de arquivo não permitido.");
        }

        var contentType = string.IsNullOrWhiteSpace(arquivo.ContentType)
            ? null
            : arquivo.ContentType.Trim();

        if (contentType?.Length > 150)
        {
            return BadRequest("O tipo de conteúdo do arquivo é inválido.");
        }

        var nomeArmazenado = $"{Guid.NewGuid():N}{extensao.ToLowerInvariant()}";
        var diretorio = Path.Combine(
            _ambiente.ContentRootPath,
            "storage",
            "documentos",
            idProcesso.ToString());

        Directory.CreateDirectory(diretorio);

        var caminhoFisico = Path.Combine(diretorio, nomeArmazenado);
        var caminhoRelativo =
            $"storage/documentos/{idProcesso}/{nomeArmazenado}";

        var documento = new DocumentoProcesso
        {
            IdProcesso = idProcesso,
            TipoDocumento = dados.TipoDocumento.Trim(),
            NomeArquivo = nomeOriginal,
            ContentType = contentType,
            TamanhoBytes = arquivo.Length,
            CaminhoArmazenamento = caminhoRelativo,
            IncluirNaDuimp = dados.IncluirNaDuimp,
        };

        try
        {
            await using (var stream = new FileStream(
                caminhoFisico,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true))
            {
                await arquivo.CopyToAsync(stream, cancellationToken);
            }

            _context.DocumentoProcessos.Add(documento);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            if (System.IO.File.Exists(caminhoFisico))
            {
                System.IO.File.Delete(caminhoFisico);
            }

            throw;
        }

        var resposta = ConverterDocumento(documento);

        return CreatedAtAction(
            nameof(Listar),
            new { idProcesso },
            resposta);
    }

    [HttpPut("{idDocumento:int}/inclusao-duimp")]
    public async Task<IActionResult> AtualizarInclusaoDuimp(
        long idProcesso,
        int idDocumento,
        [FromBody] DocumentoInclusaoDuimpDto dados,
        CancellationToken cancellationToken)
    {
        var documento = await _context.DocumentoProcessos
            .SingleOrDefaultAsync(
                item => item.IdDocumento == idDocumento
                    && item.IdProcesso == idProcesso,
                cancellationToken);

        if (documento is null)
        {
            return NotFound("Documento não encontrado neste processo.");
        }

        documento.IncluirNaDuimp = dados.IncluirNaDuimp;
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static DocumentoProcessoDto ConverterDocumento(
        DocumentoProcesso documento)
    {
        return new DocumentoProcessoDto
        {
            IdDocumento = documento.IdDocumento,
            IdProcesso = documento.IdProcesso,
            TipoDocumento = documento.TipoDocumento,
            NomeArquivo = documento.NomeArquivo,
            ContentType = documento.ContentType,
            TamanhoBytes = documento.TamanhoBytes,
            IncluirNaDuimp = documento.IncluirNaDuimp,
            DataUpload = documento.DataUpload,
        };
    }
}
