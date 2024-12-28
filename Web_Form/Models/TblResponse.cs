using System;
using System.Collections.Generic;

namespace Web_Form.Models;

public partial class TblResponse
{
    public int Id { get; set; }

    public int FormId { get; set; }

    public int QuestionId { get; set; }

    public int? OptionId { get; set; }

    public string? ResponseText { get; set; }

    public string? UserId { get; set; }

    public DateTime SubmissionDate { get; set; }

    public virtual TblForm Form { get; set; } = null!;

    public virtual TblQuestionOption? Option { get; set; }

    public virtual TblQuestion Question { get; set; } = null!;

}
