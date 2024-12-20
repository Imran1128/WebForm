using Web_Form.Models;

namespace Web_Form.ViewModels
{
    public class FullFormViewModel
    {
        public TblForm  TblForm { get; set; }
        public TblQuestion TblQuestion { get; set; }
        public TblQuestionOption tblQuestionOption { get; set; }
        public List<TblQuestion> TblQuestionsList { get; set; }
        
    }
}
