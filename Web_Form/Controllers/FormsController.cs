using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;
using Web_Form.Data;
using Web_Form.Interfaces;
using Web_Form.Migrations;
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
        private readonly UserManager<ApplicationUser> userManager;

        public FormsController(DbContext context, IFormService formService, ILogger<FormsController> logger,UserManager<ApplicationUser> userManager)
        {
            _context = context;
            this.formService = formService;
            this.logger = logger;
            this.userManager = userManager;
        }

        // GET: Forms
        public async Task<IActionResult> Index()
        {
            return View(await _context.TblForms.ToListAsync());
        }

        // GET: Forms/Details/5
        public async Task<IActionResult> Details(int FormId)
        {
            
            var tblForm = await _context.TblForms
    .Include(f => f.TblQuestions)
        .ThenInclude(q => q.TblQuestionOptions)
    .Include(f => f.TblComments)
    .Include(f => f.TblLikes)
    .AsNoTracking()
    .FirstOrDefaultAsync(f => f.FormId == FormId);

            if (tblForm == null)
            {
                return NotFound("Form not found.");
            }

            if (tblForm.TblLikes == null)
            {
                throw new Exception("TblLikes collection is null.");
            }

            if (tblForm.TblComments == null)
            {
                throw new Exception("TblComments collection is null.");
            }

            if (tblForm.TblQuestions == null)
            {
                throw new Exception("TblQuestions collection is null.");
            }

            if (tblForm == null)
            {
                return NotFound();
            }

            // Ensure collections are initialized
            //tblForm.TblQuestions ??= new List<TblQuestion>();
            //tblForm.TblComments ??= new List<TblComment>();
            //tblForm.TblLikes ??= new List<TblLike>();

            // Get current user ID
            var user = await userManager.GetUserAsync(User);
            bool isLikedByCurrentUser = false;

            if (user != null)
            {
                isLikedByCurrentUser = tblForm.TblLikes.Any(l => l.UserId == user.Id);
            }

            // Populate view model
            var fullFormViewModel = new FullFormViewModel
            {
                TblForm = tblForm,
                TblQuestionsList = tblForm.TblQuestions.ToList(),
                tblQuestionOptionList = tblForm.TblQuestions.SelectMany(q => q.TblQuestionOptions).ToList(),
                tblCommentList = tblForm.TblComments.ToList(),
                LikeCount = tblForm.TblLikes.Count,
                IsLikedByCurrentUser = isLikedByCurrentUser
            };
            var hasLiked = await _context.TblLikes
       .AnyAsync(l => l.FormId == FormId && l.UserId == user.Id);
            ViewBag.HasLiked = hasLiked;
            
            return View(fullFormViewModel);
        }
      




        // GET: Forms/Create
        public IActionResult Create()
        {
            var fullForm = new FullFormViewModel
            {
                QuestionType = _context.TblKeywordMasters.Where(c => c.KeywordType == "QuestionType").ToList()
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
            try
            {
                if (ModelState.IsValid)
                {
                    if (!string.IsNullOrWhiteSpace(fullFormViewModel.TblQuestion?.Question))
                    {
                        var questionsList = HttpContext.Session.Get<List<TblQuestion>>("TblQuestionsList") ??
                                            new List<TblQuestion>();


                        if (fullFormViewModel.TblForm != null)
                        {
                            var formInsert = await _context.TblForms.AddAsync(fullFormViewModel.TblForm);
                            await _context.SaveChangesAsync();
                            foreach (var tblQuestion in questionsList)
                            {
                                if (formInsert.Entity != null) tblQuestion.FormId = formInsert.Entity.FormId;
                                var isQuestionInsert = await _context.TblQuestions.AddAsync(tblQuestion);

                                await _context.SaveChangesAsync();
                                if (tblQuestion.tblQuestionOptionlList == null) continue;

                                foreach (var tblQuestionOption in tblQuestion.tblQuestionOptionlList)
                                {
                                    tblQuestionOption.QuestionId = isQuestionInsert.Entity.QuestionId;
                                }
                                await _context.TblQuestionOptions.AddRangeAsync(tblQuestion.tblQuestionOptionlList);
                                await _context.SaveChangesAsync();
                            }
                            await _context.SaveChangesAsync();
                        }
                        return RedirectToAction("Index");
                    }
                }
                return View(fullFormViewModel);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
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
                //var optionlist = HttpContext.Session.Get<List<TblQuestionOption>>("TblOptionList") ?? new List<TblQuestionOption>();

                if (!string.IsNullOrWhiteSpace(fullFormViewModel.TblQuestion?.Question))
                {
                    //fullFormViewModel.TblQuestion.tblQuestionOptionlList = optionlist;
                    //    optionlist.Add(fullFormViewModel.tblQuestionOption);
                    fullFormViewModel.TblQuestion.tblQuestionOptionlList = HttpContext.Session.Get<List<TblQuestionOption>>("TblQuestionOptionList");

                    questionsList.Add(fullFormViewModel.TblQuestion);
                }

                HttpContext.Session.Set("TblQuestionsList", questionsList);
                fullFormViewModel.TblQuestionsList = questionsList;
            }

            return PartialView($"AddQuestion", fullFormViewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOption(FullFormViewModel fullFormViewModel, bool clearSession = false)
        {
            if (!string.IsNullOrWhiteSpace(fullFormViewModel.tblQuestionOption.OptionText))
            {
                var optionList = HttpContext.Session.Get<List<TblQuestionOption>>("TblQuestionOptionList") ?? new List<TblQuestionOption>();
                //var optionlist = HttpContext.Session.Get<List<TblQuestionOption>>("TblOptionList") ?? new List<TblQuestionOption>();

                if (!string.IsNullOrWhiteSpace(fullFormViewModel.tblQuestionOption?.OptionText))
                {
                    optionList.Add(fullFormViewModel.tblQuestionOption);
                }

                HttpContext.Session.Set("TblQuestionOptionList", optionList);
                fullFormViewModel.tblQuestionOptionList = optionList;
            }

            return PartialView($"AddOption", fullFormViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOptionReset(FullFormViewModel fullFormViewModel, bool clearSession = false)
        {
            HttpContext.Session.Remove("TblQuestionOptionList");
            fullFormViewModel.tblQuestionOptionList = new List<TblQuestionOption>();
            return PartialView($"AddOption", fullFormViewModel);
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
        [HttpPost]
        public IActionResult AddComment(TblComment tblComment)
        {
            if (ModelState.IsValid)
            {
                // Ensure Commented_On is set to the current time
                tblComment.Commented_On = DateTime.Now;

                // Validate that the FormId exists
                var formExists = _context.TblForms.Any(f => f.FormId == tblComment.FormId);
                if (!formExists)
                {
                    ModelState.AddModelError("FormId", "The specified form does not exist.");
                    return RedirectToAction("Details", "Forms", new { FormId = tblComment.FormId });
                }

                try
                {
                    _context.TblComments.Add(tblComment);
                    _context.SaveChanges();

                    // Redirect to a relevant view (e.g., the form details page)
                    return RedirectToAction("Details", "Forms", new { FormId = tblComment.FormId });
                }
                catch (Exception ex)
                {
                    // Log the exception (logging logic depends on your setup)
                    ModelState.AddModelError("", "An error occurred while adding the comment.");
                }
            }

            // If we reach here, something went wrong, redisplay the form
            return RedirectToAction("Details", "Forms", new { FormId = tblComment.FormId });
        }

        [HttpGet]
        public IActionResult GetComments(int formId)
        {
            var comments = _context.TblComments
    .Where(c => c.FormId == formId)
    .OrderByDescending(c => c.Commented_On) // Ensure this is a DateTime
    .Select(c => new
    {
        c.Id,
        c.UserId,
        c.Comment,
        CommentedOn = c.Commented_On // No formatting needed here; handle it in the front end
    })
    .ToList();

            return Json(comments);
        }
        public async Task<IActionResult>  Like(TblLike tblLike)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid data provided.");
            }

            // Check if the user exists
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Check if the form exists
            var form = _context.TblForms.FirstOrDefault(f => f.FormId == tblLike.FormId);
            if (form == null)
            {
                return NotFound("Form not found.");
            }

            // Add the like to the TblLikes table
            _context.TblLikes.Add(tblLike);

            // Increment the Likes count on the form
            form.Likes++; // Assuming `Likes` is a property in `TblForm`

            // Save changes to the database
            _context.SaveChanges();

            // Return the updated like count
            return RedirectToAction("Details", "Forms", new { FormId = tblLike.FormId });
        }
        public async Task<IActionResult> Unlike(int FormId)
        {
            // Get the current authenticated user
            var user = await userManager.GetUserAsync(User);
            
            if (user == null)
            {
                return Unauthorized();
            }

            // Check if the like exists for this user and form
            var like = _context.TblLikes
    .Where(l => l.FormId == FormId && l.UserId == user.Id)
    .FirstOrDefault();


            if (like == null)
            {
                // If the user hasn't liked the form, return a message or do nothing
                return NotFound("Like not found.");
            }

            // Remove the like from the TblLikes table
            _context.TblLikes.Remove(like);

            // Get the form and decrement the like count
            var form = await _context.TblForms.FirstOrDefaultAsync(f => f.FormId == FormId);
            if (form != null)
            {
                form.Likes--; // Decrement the like count
                _context.TblForms.Update(form);
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Return the updated like count
            return RedirectToAction("Details", "Forms", new { FormId = FormId });
        }

        [HttpPost]
        public IActionResult SubmitForm(int formId, Dictionary<int, string> answers,string UniqueId)
        {
            if (answers != null && answers.Any())  // Ensure that answers are not null or empty
            {
                // Loop through the answers
                foreach (var answer in answers)
                {
                    var userId = userManager.GetUserId(User);

                    // Create a new response entry for each question answered
                    var answerEntry = new TblResponse
                    {
                        FormId = formId,
                        QuestionId = answer.Key,
                        ResponseText = answer.Value,
                        SubmissionDate = DateTime.Now,
                        UserId = userId,
                        UniqueId = UniqueId,
                    };

                    // Add the response to the context
                    _context.TblResponses.Add(answerEntry);
                    
                }

                // Retrieve the form from the database and increment the submission count
                var tblForm = _context.TblForms.FirstOrDefault(f => f.FormId == formId);
                if (tblForm != null)
                {
                    tblForm.SubmissionCount++;  // Increment the submission count
                    _context.SaveChanges();  // Save the changes to TblForms and TblResponses
                }

                // Commit changes to the database
                _context.SaveChanges();
               
              

                // Redirect to a Thank You page
                return RedirectToAction("ThankYou");
            }

            // If no answers are submitted, show an error page
            return View("Error");
        }


        public ActionResult ThankYou()
        {
            return View();  // This will return the ThankYou.cshtml view
        }
        [HttpGet]
        public IActionResult SubmittedForms()
        {
            return View();
        }
            [HttpGet]
        public IActionResult SubmittedFormsApi()
        {
            // Fetch forms that have answers, including created by, answered by, and submission date
            var submissions = _context.TblResponses
         .Include(r => r.Form) // Ensure Form navigation property is loaded
         .GroupBy(r => r.UniqueId) // Group by UniqueId
         .Select(group => new
         {
             UniqueId = group.Key,
             SubmissionDate = group.FirstOrDefault().SubmissionDate,
             FormId = group.FirstOrDefault().FormId,
             CreatedBy = group.FirstOrDefault().Form.Createdby, // Fetch CreatedBy from Form
             AnsweredBy = group.FirstOrDefault().UserId, // Fetch AnsweredBy from Response
             Responses = group.Select(r => new
             {
                 QuestionId = r.QuestionId,
                 ResponseText = r.ResponseText
             }).ToList()
         })
         .ToList();

           

            return Ok(submissions); // Pass the forms with the extra info to the view
        }



    }

}










