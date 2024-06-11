using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvnIdentityManager;

/// <summary>
/// Application Role class that includes claims
/// </summary>
public class IMApplicationRole : IdentityRole
{
    public IMApplicationRole() { }

    public IMApplicationRole(string roleName) : base(roleName) { }

    public virtual ICollection<IdentityRoleClaim<string>>? Claims { get; set; }
}
