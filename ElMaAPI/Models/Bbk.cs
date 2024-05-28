using System;
using System.Collections.Generic;

namespace ElMaAPI.Models;

public partial class Bbk
{
    public int BbkId { get; set; }

    public string? BbkCode { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
