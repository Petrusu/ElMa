using System;
using System.Collections.Generic;

namespace ElMaAPI.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? Username { get; set; }

    public string? Userpassword { get; set; }

    public string? Email { get; set; }

    public int Userrole { get; set; }

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual Userrole UserroleNavigation { get; set; } = null!;
}
