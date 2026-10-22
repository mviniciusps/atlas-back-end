using System.ComponentModel.DataAnnotations;

namespace atlas_back_end.Dtos.Processos;

public sealed class ProcessoCadastroDto
{
    [Range(1, int.MaxValue)]
    public int IdEstabelecimento { get; set; }

    [Required]
    [StringLength(30)]
    public string NumeroProcesso { get; set; } = string.Empty;

    [StringLength(80)]
    public string? ReferenciaCliente { get; set; }

    public DateOnly? PrevisaoChegada { get; set; }

    [StringLength(200)]
    public string? Exportador { get; set; }

    [StringLength(1000)]
    public string? Observacoes { get; set; }

    [StringLength(50)]
    public string? ConhecimentoMaster { get; set; }

    [StringLength(50)]
    public string? ConhecimentoHouse { get; set; }

    [StringLength(20)]
    public string? Modal { get; set; }

    public DateOnly? Desembaraco { get; set; }

    [StringLength(150)]
    public string? RecintoAlfandegado { get; set; }

    [Required]
    [StringLength(100)]
    public string Situacao { get; set; } = string.Empty;
}
