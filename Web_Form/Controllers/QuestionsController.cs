using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Web_Form.Data;
using Web_Form.Models;
using DbContext = Web_Form.Data.DbContext;

namespace Web_Form.Controllers
{
    public class QuestionsController : Controller
    {
        private readonly DbContext _context;

        public QuestionsController(DbContext context)
        {
            _context = context;
        }

        // GET: Questions
        public async Task<IActionResult> Index()
        {
            var dbContext = _context.TblQuestions.Include(t => t.Form).Include(t => t.QuestionTypeNavigation);
            return View(await dbContext.ToListAsync());
        }

        // GET: Questions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblQuestion = await _context.TblQuestions
                .Include(t => t.Form)
                .Include(t => t.QuestionTypeNavigation)
                .FirstOrDefaultAsync(m => m.QuestionId == id);
            if (tblQuestion == null)
            {
                return NotFound();
            }

            return View(tblQuestion);
        }

        // GET: Questions/Create
        public IActionResult Create()
        {
            ViewData["FormId"] = new SelectList(_context.TblForms, "FormId", "FormId");
            ViewData["QuestionType"] = new SelectList(_context.TblKeywordMasters, "KeywordId", "KeywordId");
            return View();
        }

        // POST: Questions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("QuestionId,FormId,Question,QuestionType,InlineImage,IsRequired,Description,Serial,IsSuffled,Status,Createdby,CreatedOn,UpdatedBy,UpdatedOn")] TblQuestion tblQuestion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tblQuestion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FormId"] = new SelectList(_context.TblForms, "FormId", "FormId", tblQuestion.FormId);
            ViewData["QuestionType"] = new SelectList(_context.TblKeywordMasters, "KeywordId", "KeywordId", tblQuestion.QuestionType);
            return View(tblQuestion);
        }

        // GET: Questions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblQuestion = await _context.TblQuestions.FindAsync(id);
            if (tblQuestion == null)
            {
                return NotFound();
            }
            ViewData["FormId"] = new SelectList(_context.TblForms, "FormId", "FormId", tblQuestion.FormId);
            ViewData["QuestionType"] = new SelectList(_context.TblKeywordMasters, "KeywordId", "KeywordId", tblQuestion.QuestionType);
            return View(tblQuestion);
        }

        // POST: Questions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("QuestionId,FormId,Question,QuestionType,InlineImage,IsRequired,Description,Serial,IsSuffled,Status,Createdby,CreatedOn,UpdatedBy,UpdatedOn")] TblQuestion tblQuestion)
        {
            if (id != tblQuestion.QuestionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tblQuestion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TblQuestionExists(tblQuestion.QuestionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["FormId"] = new SelectList(_context.TblForms, "FormId", "FormId", tblQuestion.FormId);
            ViewData["QuestionType"] = new SelectList(_context.TblKeywordMasters, "KeywordId", "KeywordId", tblQuestion.QuestionType);
            return View(tblQuestion);
        }

        // GET: Questions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblQuestion = await _context.TblQuestions
                .Include(t => t.Form)
                .Include(t => t.QuestionTypeNavigation)
                .FirstOrDefaultAsync(m => m.QuestionId == id);
            if (tblQuestion == null)
            {
                return NotFound();
            }

            return View(tblQuestion);
        }

        // POST: Questions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tblQuestion = await _context.TblQuestions.FindAsync(id);
            if (tblQuestion != null)
            {
                _context.TblQuestions.Remove(tblQuestion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TblQuestionExists(int id)
        {
            return _context.TblQuestions.Any(e => e.QuestionId == id);
        }
    }
}
