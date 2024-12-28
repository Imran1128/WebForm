using System;
using System.Collections.Generic;

namespace Web_Form.Models;

public partial class TblKeywordMaster
{
    public int KeywordId { get; set; }

    public string? KeywordName { get; set; }

    public string? KeywordType { get; set; }

    public bool? Status { get; set; }
}
