using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Web_Form.Data;
using Web_Form.Interfaces;
using Web_Form.Models;
using Web_Form.ViewModels;
using DbContext = Web_Form.Data.DbContext;

namespace Web_Form.Controllers
{
    public class FormsController : Controller
    {
        private readonly DbContext _context;
        private readonly IFormService formService;
        private readonly ILogger<FormsController> logger;

        public FormsController(DbContext context ,IFormService formService, ILogger<FormsController> logger)
        {
            _context = context;
            this.formService = formService;
            this.logger = logger;
        }

        // GET: Forms
        public async Task<IActionResult> Index()
        {
            return View(await _context.TblForms.ToListAsync());
        }

        // GET: Forms/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblForm = await _context.TblForms
                .FirstOrDefaultAsync(m => m.FormId == id);
            if (tblForm == null)
            {
                return NotFound();
            }

            return View(tblForm);
        }

        // GET: Forms/Create
        public IActionResult Create()
        {
            var fullForm = new FullFormViewModel
             {
               //tblKeywordMaster = _context.TblKeywordMasters.ToList()

            };
            
            return View(fullForm);
        }

        // POST: Forms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create(FullFormViewModel fullFormViewModel)
        {
            if(ModelState.IsValid)
            {

                if (!string.IsNullOrWhiteSpace(fullFormViewModel.TblQuestion?.Question))
                {
                    var questionsList = HttpContext.Session.Get<List<TblQuestion>>("TblQuestionsList") ?? new List<TblQuestion>();

                    if (!string.IsNullOrWhiteSpace(fullFormViewModel.TblQuestion?.Question))
                    {
                        fullFormViewModel.TblQuestion.tblQuestionOptionlList.Add(fullFormViewModel.tblQuestionOption);
                        questionsList.Add(fullFormViewModel.TblQuestion);
                    }
                   
                    _context.Add(fullFormViewModel);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index"); 
                }
            }
           return View();
        }

        // GET: Forms/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblForm = await _context.TblForms.FindAsync(id);
            if (tblForm == null)
            {
                return NotFound();
            }
            return View(tblForm);
        }

        // POST: Forms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FormId,Title,HeaderPhoto,IsFavourite,FormStatus,BackgroundColor,Email,Name,Status,LastOpened,Createdby,CreatedOn,UpdatedBy,UpdatedOn")] TblForm tblForm)
        {
            if (id != tblForm.FormId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tblForm);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TblFormExists(tblForm.FormId))
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
            return View(tblForm);
        }

        // GET: Forms/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblForm = await _context.TblForms
                .FirstOrDefaultAsync(m => m.FormId == id);
            if (tblForm == null)
            {
                return NotFound();
            }

            return View(tblForm);
        }

        // POST: Forms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var tblForm = await _context.TblForms.FindAsync(id);
            if (tblForm != null)
            {
                _context.TblForms.Remove(tblForm);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TblFormExists(int id)
        {
            return _context.TblForms.Any(e => e.FormId == id);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuestion(FullFormViewModel fullFormViewModel, bool clearSession = false)
        {
            

            if (!string.IsNullOrWhiteSpace(fullFormViewModel.TblQuestion?.Question))
            {
                var questionsList = HttpContext.Session.Get<List<TblQuestion>>("TblQuestionsList") ?? new List<TblQuestion>();
                var optionlist = HttpContext.Session.Get<List<TblQuestionOption>>("TblOptionList") ?? new List<TblQuestionOption>();

                if (!string.IsNullOrWhiteSpace(fullFormViewModel.TblQuestion?.Question))
                {
                    optionlist.Add(fullFormViewModel.tblQuestionOption);
                    questionsList.Add(fullFormViewModel.TblQuestion);
                }

                HttpContext.Session.Set("TblQuestionsList", questionsList);
                fullFormViewModel.TblQuestionsList = questionsList;
                HttpContext.Session.Set("TblOptionList", optionlist);
                fullFormViewModel.TblQuestion.tblQuestionOptionlList = optionlist;
            }

            return PartialView($"AddQuestion", fullFormViewModel);
        }
        [HttpPost]
        public IActionResult ClearSession()
        {
            HttpContext.Session.Clear(); // Clear all session data
            return Ok(new { message = "Session cleared successfully." });
        }
        [HttpPost]
        public IActionResult DeleteQuestion(int questionIndex)
        {
           
            var questionsList = HttpContext.Session.Get<List<TblQuestion>>("TblQuestionsList") ?? new List<TblQuestion>();

            if (questionIndex < 0 || questionIndex >= questionsList.Count)
            {
                return BadRequest("Invalid question index.");
            }

           
            questionsList.RemoveAt(questionIndex);

            HttpContext.Session.Set("TblQuestionsList", questionsList);

            var model = new FullFormViewModel { TblQuestionsList = questionsList };

         
            return PartialView("AddQuestion", model); 
        }







    }
}
