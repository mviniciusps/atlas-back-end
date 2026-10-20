using System.ComponentModel.DataAnnotations;

namespace atlas_back_end.Dtos.Importadores
{
    public sealed class ImportadorCadastroDto
    {
        [Required]
        [StringLength(200)]
        public string RazaoSocial { get; set; } = string.Empty;

        [StringLength(200)]
        public string? NomeFantasia { get; set; }

        [Required]
        [MinLength(1)]
        public List<EstabelecimentoCadastroDto> Estabelecimentos { get; set; } = [];
}

public sealed class EstabelecimentoCadastroDto
    {
        [Required]
        [StringLength(18)]
        public string Cnpj { get; set; } = string.Empty;

        [StringLength(200)]
        public string? NomeEstabelecimento { get; set; }
    }
}
