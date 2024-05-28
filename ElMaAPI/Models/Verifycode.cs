using System;
using System.Collections.Generic;

namespace ElMaAPI.Models;

public partial class Verifycode
{
    public int CodeId { get; set; }

    public string? Code { get; set; }
}
