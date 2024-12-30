
using System.ComponentModel.DataAnnotations;
using Web_Form.Models;

namespace Web_Form.ViewModels
{
    public class ResponseViewModel
    {
        public int Id { get; set; }

        public int FormId { get; set; }

        public int QuestionId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Question { get; set; }
        public string? CreatedBy { get; set; }
        public string? SubmittedBy { get; set; }
        public int? OptionId { get; set; }
        [Required(ErrorMessage = "Please provide an answer.")]
        public string? ResponseText { get; set; }

        public string? UserId { get; set; }
        public string UniqueId { get; set; }
        public DateTime SubmissionDate { get; set; }

        public virtual TblForm? Form { get; set; } = null!;

        public virtual TblQuestionOption? Option { get; set; }

        public virtual TblQuestion Questions { get; set; } = null!;
        public List<TblResponse> tblResponses { get; set; }

    }
}
