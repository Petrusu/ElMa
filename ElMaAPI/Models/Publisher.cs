using System;
using System.Collections.Generic;

namespace ElMaAPI.Models;

public partial class Publisher
{
    public int PublishersId { get; set; }

    public string Publishersname { get; set; } = null!;

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
