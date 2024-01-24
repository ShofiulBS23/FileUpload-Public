using System.ComponentModel.DataAnnotations;

namespace File_Public.DbModels
{
    public class DocTypeLU
    {
        [Key]
        public int DocTypeID { get; set; }

        [Required]
        [MaxLength(20)]
        public string DocType { get; set; }

    }
}
