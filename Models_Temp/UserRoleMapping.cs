using System;
using System.Collections.Generic;

namespace PMMS.Models_Temp;

public partial class UserRoleMapping
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int RoleId { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? LastModifiedBy { get; set; }

    public DateTime? LastModifiedDate { get; set; }
}
