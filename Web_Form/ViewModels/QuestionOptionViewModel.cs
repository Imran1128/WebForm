using Web_Form.Models;

namespace Web_Form.ViewModels
{
    public class QuestionOptionViewModel
    {
        public int OptionId { get; set; }

        public int QuestionId { get; set; }


        public string? OptionText { get; set; } = null!;

        public virtual QuestionViewModel? Question { get; set; } = null!;
    }
}
