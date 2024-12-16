using System;
using System.Collections.Generic;

namespace Web_Form.Models;

public partial class TblQuestion
{
    public int QuestionId { get; set; }

    public int FormId { get; set; }

    public string? Question { get; set; }

    public int? QuestionType { get; set; }

    public string? InlineImage { get; set; }

    public bool? IsRequired { get; set; }

    public string? Description { get; set; }

    public int Serial { get; set; }

    public bool? IsSuffled { get; set; }

    public bool? Status { get; set; }

    public string? Createdby { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public virtual TblForm Form { get; set; } = null!;

    public virtual TblKeywordMaster? QuestionTypeNavigation { get; set; }

    public virtual ICollection<TblQuestionOption> TblQuestionOptions { get; set; } = new List<TblQuestionOption>();
}
