using System.ComponentModel.DataAnnotations;

namespace PMMS.Models
{
    public class Client : EntitiesBase
    {
        
        public override long Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? CompanyName { get; set; }
        public string? Website { get; set; }
        
    }
}
