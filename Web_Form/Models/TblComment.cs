using System;
using System.Collections.Generic;

namespace Web_Form.Models;

public partial class TblComment
{
    public int Id { get; set; }

    public int FormId { get; set; }

    public string UserId { get; set; } = null!;

    public string Comment { get; set; } = null!;
    public DateTime Commented_On { get; set; }

    public virtual TblForm? Form { get; set; }
}
