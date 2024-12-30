using System;
using System.Collections.Generic;

namespace Web_Form.Models;

public partial class TblFormSubmissionByUser
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public int FormId { get; set; }
}
