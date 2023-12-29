using System;
using System.Collections.Generic;

namespace ElMaAPI.Models;

public partial class Favorite
{
    public int? BookId { get; set; }

    public int? UserId { get; set; }

    public virtual Book? Book { get; set; }

    public virtual User? User { get; set; }
}
