using Web_Form.Models;

namespace Web_Form.ViewModels
{
    public class FullFormViewModel
    {
        public TblFormViewModel  TblFormViewModel { get; set; }
        public List<TblQuestionOptionViewModel> TblQuestionsViewModel { get; set; }
        public List<TblQuestionOptionViewModel> QuestionOptionViewModels { get; set; }
    }
}
