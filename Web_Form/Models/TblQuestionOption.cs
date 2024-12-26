using System;
using System.Collections.Generic;

namespace Web_Form.Models;

public partial class TblQuestionOption
{
    public int OptionId { get; set; }

    public int QuestionId { get; set; }

    public string? OptionText { get; set; } = null!;

    public virtual TblQuestion? Question { get; set; } = null!;
}
