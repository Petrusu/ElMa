using System;
using System.Collections.Generic;

namespace ElMaAPI.Models;

public partial class Editor
{
    public int EditorsId { get; set; }

    public string? Editorname { get; set; }

    public virtual ICollection<BookEditor> BookEditors { get; set; } = new List<BookEditor>();
}
