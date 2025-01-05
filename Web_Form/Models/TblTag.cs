using System;
using System.Collections.Generic;

namespace Web_Form.Models;

public partial class TblTag
{
    public int Id { get; set; }

    public int? FormId { get; set; }

    public string? Tag { get; set; }
}
