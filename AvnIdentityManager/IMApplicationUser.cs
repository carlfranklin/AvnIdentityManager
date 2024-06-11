using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvnIdentityManager;

/// <summary>
/// ApplicationUser class that includes roles and claims
/// </summary>
public class IMApplicationUser : IdentityUser
{
    public virtual ICollection<IdentityUserRole<string>>? Roles { get; set; }
    public virtual ICollection<IdentityUserClaim<string>>? Claims { get; set; }
}