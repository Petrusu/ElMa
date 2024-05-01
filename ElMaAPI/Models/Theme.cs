using System;
using System.Collections.Generic;

namespace ElMaAPI.Models;

public partial class Theme
{
    public int ThemesId { get; set; }

    public string? Themesname { get; set; }

    public virtual ICollection<BookTheme> BookThemes { get; set; } = new List<BookTheme>();
}
