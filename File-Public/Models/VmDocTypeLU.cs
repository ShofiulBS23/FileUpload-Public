using System.ComponentModel.DataAnnotations;

namespace File_Public.Models
{
    public class VmDocTypeLU
    {
        public int DocTypeID { get; set; }

        [Required]
        [StringLength(20)]
        public string DocType { get; set; }
    }
}
