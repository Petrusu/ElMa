using System;
using System.Collections.Generic;

namespace ElMaAPI.Models;

public partial class Author
{
    public int AuthorsId { get; set; }

    public string? Authorsname { get; set; }

    public virtual ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
}
