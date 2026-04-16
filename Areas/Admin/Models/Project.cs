using System.ComponentModel.DataAnnotations;

namespace PMMS.Models
{
    public class Project : EntitiesBase
    {
        
        public override long Id { get; set; }
        public  long ClientId { get; set; }
        public string? ProjectTitle { get; set; }
        public string? Service_Ids { get; set; }
        public string? Description { get; set; }
        public decimal? Budget { get; set; }
        public string? Timeline { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }
        public string? Status_TEXT { get; set; }
        public string? Priority { get; set; }
        public string? Priority_TEXT { get; set; }
        public string? Client_Name { get; set; }
        public string? Service_Names { get; set; }
        public List<ProjectFeatureList> ProjectFeatureList { get; set; }


    }
    public class ProjectFeatureList : EntitiesBase
    {

        public override long Id { get; set; }
        public  long ProjectId { get; set; }
        public  long ServiceId { get; set; }
        public  long ChecklistId { get; set; }
        public string? Project_Feature_Name { get; set; }        
        public string? Service_Name { get; set; }        
        public bool? IsIncluded { get; set; }      
        public string? Notes { get; set; }      

    }
}
