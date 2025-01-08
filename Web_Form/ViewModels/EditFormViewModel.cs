using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

using Web_Form.Models;

namespace Web_Form.ViewModels
{
    public class EditFormViewModel
    {
        public int FormId { get; set; }
        [Required(ErrorMessage = "Title is required.")]
        public string? Title { get; set; } = null!;
        public TblQuestion? TblQuestion { get; set; }
        public string? HeaderPhoto { get; set; }
        public bool IsPublic { get; set; }

        public bool? IsFavourite { get; set; }

        public byte? FormStatus { get; set; }
        public List<TblKeywordMaster>? QuestionType { get; set; }
        public string? BackgroundColor { get; set; }
        public TblQuestionOption? tblQuestionOption { get; set; }
        public string? Email { get; set; }
        public List<TblQuestion>? TblQuestionsList { get; set; } = new List<TblQuestion>();
        public List<TblPrivateUser>? privateUsers { get; set; } = new List<TblPrivateUser>();
        public List<TblTag>? tbltaglist { get; set; } = new List<TblTag>();
        public IEnumerable<SelectListItem>? tblTags { get; set; }
        public TblTag? tblTag { get; set; }
        public IEnumerable<SelectListItem>? appUsers { get; set; }

        public string? Name { get; set; }

        public bool? Status { get; set; }

        public DateTime? LastOpened { get; set; }

        public string? Createdby { get; set; }

        public DateTime? CreatedOn { get; set; }

        public string? UpdatedBy { get; set; }
        public int? SubmissionCount { get; set; } = 0;
        public string? UserId { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? Likes { get; set; }
        [Required(ErrorMessage = "Description is required.")]
        public string? Description { get; set; }


        public virtual List<QuestionViewModel>? TblQuestions { get; set; } 
        public virtual ICollection<TblLike>? TblLikes { get; set; } = new List<TblLike>();
        public virtual ICollection<TblComment>? TblComments { get; set; } = new List<TblComment>();
    }
}
