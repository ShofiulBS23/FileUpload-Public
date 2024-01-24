using System.ComponentModel.DataAnnotations;

namespace File_Public.Models
{
    public class VmDocument
    {
        public int Id { get; set; }

        [Required]
        public Guid ClientId { get; set; }

        [MaxLength(20)]
        [Required]
        public string ISIN { get; set; }

        [Required]
        [StringLength(20)]
        public string Language { get; set; }

        [Required]
        [StringLength(20)]
        public string DocType { get; set; }

        [Required]
        [StringLength(20)]
        public DateTime DocDate { get; set; }

        [Required]
        [StringLength(500)]
        public string DocName { get; set; }

        [Required]
        [StringLength(10)]
        public string DocExt { get; set; }
    }
}
