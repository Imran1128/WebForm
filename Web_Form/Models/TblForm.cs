using System;
using System.Collections.Generic;

namespace Web_Form.Models;

public partial class TblForm
{
    public int FormId { get; set; }

    public string Title { get; set; } = null!;

    public string? HeaderPhoto { get; set; }

    public bool? IsFavourite { get; set; }

    public byte? FormStatus { get; set; }

    public string? BackgroundColor { get; set; }

    public string? Email { get; set; }

    public string? Name { get; set; }

    public bool? Status { get; set; }

    public DateTime? LastOpened { get; set; }

    public string? Createdby { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }
    public int? Likes { get; set; } 

    public string Description { get; set; }

    
    public virtual ICollection<TblQuestion>? TblQuestions { get; set; } = new List<TblQuestion>();
    public virtual ICollection<TblLike>? TblLikes { get; set; } = new List<TblLike>();
    public virtual ICollection<TblComment>? TblComments { get; set; } = new List<TblComment>();
}

