using System;
using System.Collections.Generic;

namespace ElMaAPI.Models;

public partial class Publicationplase
{
    public int PublicationplaseId { get; set; }

    public string Publicationplasesname { get; set; } = null!;

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
