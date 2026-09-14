using System.ComponentModel.DataAnnotations;

namespace Diplom.Models
{

    public class TextFile
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }



}
