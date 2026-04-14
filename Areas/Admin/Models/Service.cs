using System.ComponentModel.DataAnnotations;

namespace PMMS.Models
{
    public class Service : EntitiesBase
    {
        
        public override long Id { get; set; }
        public string? ServiceName { get; set; }
        public string? Slug { get; set; }
        public string? ShortDescription { get; set; }
        public int? DisplayOrder { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? BestFor { get; set; }
        public string? Technologies { get; set; }
        public List<ServiceChecklist> ServiceChecklist { get; set; }


    }
    public class ServiceChecklist : EntitiesBase
    {

        public override long Id { get; set; }
        public  long ServiceId { get; set; }
        public string? ItemName { get; set; }        
        public int? DisplayOrder { get; set; }      

    }
}
