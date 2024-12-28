
using Web_Form.Models;

namespace Web_Form.ViewModels
{
    public class FullFormViewModel
    {
        public TblForm?  TblForm { get; set; }
        public TblQuestion? TblQuestion { get; set; }
        public TblQuestionOption? tblQuestionOption { get; set; }
        public TblLike? tblLike { get; set; }
        public List<TblQuestion>? TblQuestionsList { get; set; }=new List<TblQuestion>();
        public List<TblQuestionOption>? tblQuestionOptionList { get; set; }=new List<TblQuestionOption>();
        public List<TblKeywordMaster>? QuestionType { get; set; }
        public List<TblLike>? tblLikes { get; set; }
        public TblResponse? tblResponse { get; set; }
        public TblComment tblComment { get; set; }
        public List<TblComment>? tblCommentList { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public int LikeCount { get; set; }

    }
}
