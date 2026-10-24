using System.ComponentModel.DataAnnotations;

namespace atlas_back_end.Dtos.Documentos
{
    public class DocumentoTipoAtualizacaoDto
    {
        [Required]
        [StringLength(80)]
        public string TipoDocumento { get; set; } = string.Empty;
    }
}
