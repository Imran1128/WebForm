using System;
using System.Collections.Generic;

namespace Web_Form.Models;

public partial class TblLike
{
    public int Id { get; set; }

    public int FormId { get; set; }

    public string UserId { get; set; }

    public virtual TblForm? Form { get; set; } = null!;
}
    
