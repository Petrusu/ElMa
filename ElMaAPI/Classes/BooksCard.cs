using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;

namespace ElMaDesktop.Classes;

public class BooksCard
{
    public int BookId { get; set; }

    public string Title { get; set; } = null!;

    public string? SeriesName { get; set; }

    public string? Annotation { get; set; }

    public string Publisher { get; set; }

    public string? PlaceOfPublication { get; set; }

    public string YearOfPublication { get; set; }
    public string BBK { get; set; }
    public string? Author { get; set; }
    public string? Editor { get; set; }
    public List<int> ThemeIds { get; set; } = new List<int>();
    public List<string> ThemesName { get; set; }
    public byte[]? Image { get; set; }
    public byte[] ImageBook;
    public List<string> Authors { get; set; } = new List<string>();
    public List<string> Editors { get; set; } = new List<string>();
}

public class Author
{
    public string Authorsname { get; set; } = null!;
}

public class Editor
{
    public string Editorname { get; set; } = null!;
}
