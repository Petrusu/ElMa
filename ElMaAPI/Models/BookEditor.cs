using System;
using System.Collections.Generic;

namespace ElMaAPI.Models;

public partial class BookEditor
{
    public int BookId { get; set; }

    public int EditorsId { get; set; }

    public string? Note { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual Editor Editors { get; set; } = null!;
}
