using System.ComponentModel.DataAnnotations.Schema;
using Web_Form.Models;

namespace Web_Form.ViewModels
{
    public class QuestionViewModel
    {
        public int QuestionId { get; set; }

        public int FormId { get; set; }


        public string Question { get; set; }

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

        public virtual TblForm? Form { get; set; }

        //public virtual TblKeywordMaster? QuestionTypeNavigation { get; set; }
        public virtual ICollection<QuestionOptionViewModel>? TblQuestionOptions { get; set; } = new List<QuestionOptionViewModel>();
       public List<QuestionOptionViewModel>? tblQuestionOptionlList { get; set; } = new List<QuestionOptionViewModel>();
    }
}
