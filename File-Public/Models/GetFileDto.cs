using System.ComponentModel.DataAnnotations;

namespace File_Public.Models
{
    public class GetFileDto
    {
        [Required(ErrorMessage = "Client Id is required")]
        public string ClientId { get; set; }
        public string? ISIN { get; set; }
        public string? Lang { get; set; }
        [Required(ErrorMessage = "Doc group is required")]
        public string DocGroup { get; set; }
        public DateTime? DocDate { get; set; }
        public string? DocName { get; set; }
        public string? DocExt { get; set; }
        public bool? Save { get; set; }
    }
}
