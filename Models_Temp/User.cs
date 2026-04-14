using System;
using System.Collections.Generic;

namespace PMMS.Models_Temp;

public partial class User
{
    public int Id { get; set; }

    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int? NoOfWrongPasswordAttempts { get; set; }

    public DateTime? NextChangePasswordDate { get; set; }

    public string? Email { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? LastModifiedBy { get; set; }

    public DateTime? LastModifiedDate { get; set; }
}
