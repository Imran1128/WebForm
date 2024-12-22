using Web_Form.Models;

namespace Web_Form.ViewModels
{
    public class FullFormViewModel
    {
        public FullFormViewModel()
        {
            TblQuestionsList = new List<TblQuestion>();
        }
        public TblForm  TblForm { get; set; }
        public TblQuestion TblQuestion { get; set; }
        public TblQuestionOption tblQuestionOption { get; set; }
        public List<TblQuestion> TblQuestionsList { get; set; }
        public List< TblKeywordMaster> tblKeywordMaster { get; set; }

    }
}
