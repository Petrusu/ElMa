using System;
using System.Collections.Generic;

namespace ElMaAPI.Models;

public partial class BookEditor
{
    public int? BookId { get; set; }

    public int? EditorsId { get; set; }

    public virtual Book? Book { get; set; }

    public virtual Editor? Editors { get; set; }
}
