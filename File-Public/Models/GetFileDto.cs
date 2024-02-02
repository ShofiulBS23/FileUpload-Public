using System.ComponentModel.DataAnnotations;

namespace File_Public.Models
{
    public class GetFileDto
    {
        [Required]
        public string clientid { get; set; }
        public string Isin { get; set; }
        public string lang { get; set; }
        [Required]
        public string type { get; set; }
        public DateTime? DocDate { get; set; }
        public string DocName { get; set; }
        public string DocExt { get; set; }
        public bool? save { get; set; }
    }
}
