using System.ComponentModel.DataAnnotations.Schema;

namespace PMMS;



public partial class LOV : EntitiesBase
{
    [NotMapped] public override long Id { get; set; }
	[NotMapped] public  long CompanyId { get; set; }
    public string LOV_Column { get; set; } = null!;

	public string LOV_Code { get; set; } = null!;

	public string LOV_Desc { get; set; } = null!;	
	public string Display_Text { get; set; } = null!;	
	public long DisplayOrder { get; set; }
    [NotMapped] public long MaxDisplay_Seq_No { get; set; }
    [NotMapped] public string Action { get; set; }
}
