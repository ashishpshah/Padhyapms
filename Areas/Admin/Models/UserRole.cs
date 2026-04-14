using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMMS;

public partial class UserRole : EntitiesBase
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public override long Id { get; set; }

    public string Name { get; set; } = null!;

    public int? DisplayOrder { get; set; }

    public bool IsAdmin { get; set; }
}
