using System;
using System.Collections.Generic;

namespace ElMaAPI.Models;

public partial class Userrole
{
    public int RoleId { get; set; }

    public string? Rolename { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
