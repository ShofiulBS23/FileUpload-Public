using System.ComponentModel.DataAnnotations;

namespace File_Public.Models
{
    public class VmDocGroupLU
    {
        public int DocTypeID { get; set; }

        [Required]
        [StringLength(20)]
        public string DocGroup { get; set; }
    }
}
