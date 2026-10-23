using System.ComponentModel.DataAnnotations;

namespace atlas_back_end.Dtos.Documentos
{
    public class DocumentoProcessoUploadDto
    {
        [Required]
        public Microsoft.AspNetCore.Http.IFormFile? Arquivo { get; set; }

        [Required]
        [StringLength(80)]
        public string TipoDocumento { get; set; } = string.Empty;
        public bool IncluirNaDuimp { get; set; }
    }
}
