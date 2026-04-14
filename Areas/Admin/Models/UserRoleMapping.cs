using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMMS
{
    public partial class UserRoleMapping : EntitiesBase
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id { get; set; }
        public long UserId { get; set; }
        public long RoleId { get; set; }

        [NotMapped] public string RoleName { get; set; } = null;
        [NotMapped] public string UserName { get; set; } = null;
        [NotMapped] public long[] SelectedRoleId { get; set; } = null;
        [NotMapped] public long[] SeelectedUserId { get; set; } = null;
        [NotMapped] public List<SelectListItem> Users { get; set; }
        [NotMapped] public List<SelectListItem> Roles { get; set; }
        [NotMapped] public List<Menu> Menus { get; set; }

    }
    
   

}