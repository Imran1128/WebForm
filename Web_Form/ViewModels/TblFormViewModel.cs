using Web_Form.Models;

namespace Web_Form.ViewModels
{
    public class TblFormViewModel
    {
        public int FormId { get; set; }

        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;
        public string? HeaderPhoto { get; set; }

        public bool? IsFavourite { get; set; }

        public byte? FormStatus { get; set; }

        public string? BackgroundColor { get; set; }

        public string? Email { get; set; }

        public string? Name { get; set; }

        public bool? Status { get; set; }

        public DateTime? LastOpened { get; set; } = DateTime.Now;

        public string? Createdby { get; set; }

        public DateTime? CreatedOn { get; set; } = DateTime.Now;

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; } = DateTime.Now;

        public virtual ICollection<TblQuestion> TblQuestions { get; set; } = new List<TblQuestion>();
    }
}
