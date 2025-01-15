using System;
using Web_Form.Data;
using Web_Form.Interfaces;
using Web_Form.Models;
using System.Linq;
using Web_Form.Repository;

using Microsoft.EntityFrameworkCore;
using DbContext = Web_Form.Data.DbContext;

namespace Web_Form.Repository
{
    public class FormRepository : BaseRepository<TblForm>, IFormService
    {
        private readonly DbContext _context;

        public FormRepository(DbContext myDbContext) : base(myDbContext)
        {
            _context = myDbContext;
        }

        public async Task<IEnumerable<TblForm>> GetFormsByUserAsync(string userId)
        {
            return await _context.TblForms
                .Where(f => f.UserId == userId)
                //.Include(f => f.TblQuestions)
                .ToListAsync();
        }

        

        public async Task AddFormAsync(TblForm form)
        {
            await _context.TblForms.AddAsync(form);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateFormAsync(TblForm form)
        {
            _context.TblForms.Update(form);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteFormAsync(int formId)
        {
            var form = await _context.TblForms.FindAsync(formId);
            if (form != null)
            {
                _context.TblForms.Remove(form);
                await _context.SaveChangesAsync();
            }
        }
    }
}