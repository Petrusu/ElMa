using System;
using System.Collections.Generic;

namespace ElMaAPI.Models;

public partial class Book
{
    public int BookId { get; set; }

    public string Title { get; set; } = null!;

    public string? SeriesName { get; set; }

    public string? Annotation { get; set; }

    public int Publisher { get; set; }

    public int PlaceOfPublication { get; set; }

    public int YearOfPublication { get; set; }

    public int? BbkCode { get; set; }

    public string? Image { get; set; }

    public virtual Bbk? BbkCodeNavigation { get; set; }

    public virtual ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();

    public virtual ICollection<BookEditor> BookEditors { get; set; } = new List<BookEditor>();

    public virtual ICollection<BookTheme> BookThemes { get; set; } = new List<BookTheme>();

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual Publicationplase PlaceOfPublicationNavigation { get; set; } = null!;

    public virtual Publisher PublisherNavigation { get; set; } = null!;
}
