using Web_Form.Models;

namespace Web_Form.ViewModels
{
    public class TblQuestionOptionViewModel
    {
        public int OptionId { get; set; }

        public int QuestionId { get; set; }

        public string OptionText { get; set; } = null!;

        public virtual TblQuestion Question { get; set; } = null!;
    }
}
