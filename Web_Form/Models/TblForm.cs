using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Web_Form.Models;

public partial class TblForm
{
    public int FormId { get; set; }
    [Required(ErrorMessage = "Title is required.")]
    public string? Title { get; set; } = null!;

    public string? HeaderPhoto { get; set; }

    public bool? IsFavourite { get; set; }

    public byte? FormStatus { get; set; }

    public string? BackgroundColor { get; set; }

    public string? Email { get; set; }

    public string? Name { get; set; }

    public bool? Status { get; set; }

    public DateTime? LastOpened { get; set; }

    public string? Createdby { get; set; }
    public string? PrivateUser { get; set; }
    public string? tag { get; set; }
    public string? topic { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? UpdatedBy { get; set; }
    public int? SubmissionCount { get; set; } = 0;
    public string? UserId { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public int? Likes { get; set; }
    [Required(ErrorMessage = "Description is required.")]
    public string? Description { get; set; }

    public bool IsPublic { get; set; }
    public virtual ICollection<TblQuestion>? TblQuestions { get; set; } = new List<TblQuestion>();
    public virtual ICollection<TblResponse>? TblResponses { get; set; } = new List<TblResponse>();

    public virtual ICollection<TblLike>? TblLikes { get; set; } = new List<TblLike>();
    public virtual ICollection<TblComment>? TblComments { get; set; } = new List<TblComment>();
}

