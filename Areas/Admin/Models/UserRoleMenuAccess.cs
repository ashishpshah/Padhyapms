using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMMS;

public partial class UserRoleMenuAccess : EntitiesBase
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [NotMapped] public override long Id { get; set; }

    public long UserId { get; set; }

    public long RoleId { get; set; }

    public long MenuId { get; set; }

    public bool IsRead { get; set; }

    public bool IsCreate { get; set; }

    public bool IsUpdate { get; set; }

    public bool IsDelete { get; set; }


    [NotMapped] public string RoleName { get; set; } = null;
    [NotMapped] public string UserName { get; set; } = null;
    [NotMapped] public string MenuName { get; set; } = null;
    [NotMapped] public string Area { get; set; } = null;
    [NotMapped] public string Controller { get; set; } = null;
    [NotMapped] public string Url { get; set; } = null;
    [NotMapped] public long ParentMenuId { get; set; }
    [NotMapped] public string ParentMenuName { get; set; } = null;
    [NotMapped] public int? DisplayOrder { get; set; } = null;
    [NotMapped] public string Icon { get; set; }
}
