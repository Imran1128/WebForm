using System.ComponentModel.DataAnnotations;
using Web_Form.Models;

namespace Web_Form.ViewModels
{
    public class EditFormViewModel
    {
        public int FormId { get; set; }
        [Required(ErrorMessage = "Title is required.")]
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
        public int? SubmissionCount { get; set; } = 0;
        public string? UserId { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? Likes { get; set; }
        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; }


        public virtual List<QuestionViewModel>? TblQuestions { get; set; } 
        public virtual ICollection<TblLike>? TblLikes { get; set; } = new List<TblLike>();
        public virtual ICollection<TblComment>? TblComments { get; set; } = new List<TblComment>();
    }
}
