using System.ComponentModel.DataAnnotations;

namespace atlas_back_end.Dtos.Documentos
{
    public class DocumentoStatusConferenciaDto
    {
        [Required]
        [StringLength(30)]
        public string StatusConferencia { get; set; } = string.Empty;
    }
}
