using System;
using System.Collections.Generic;

namespace Web_Form.Models;

public partial class TblPrivateUser
{
    public int Id { get; set; }

    public string UserId { get; set; }
    public int FormId { get; set; }
}
