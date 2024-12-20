using System;
using Web_Form.Data;
using Web_Form.Interfaces;
using Web_Form.Models;
using System.Linq;
using Web_Form.Repository;

namespace Web_Form.Repository
{
    public class FormRepository : BaseRepository<TblForm>, IFormService
    {
        private readonly DbContext myDbContext;

        public FormRepository(DbContext myDbContext) : base(myDbContext)
        {
            this.myDbContext = myDbContext;
        }
    }
}