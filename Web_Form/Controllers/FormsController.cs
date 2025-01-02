using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Authorization;
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
        private readonly SignInManager<ApplicationUser> signInManager;

        public FormsController(DbContext context, IFormService formService, ILogger<FormsController> logger, UserManager<ApplicationUser> userManager,SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            this.formService = formService;
            this.logger = logger;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        // GET: Forms
        public async Task<IActionResult> Index()
        {
            return View(await _context.TblForms.ToListAsync());
        }
        public async Task<IActionResult>  GetAllFormsApi()
        {
            var userId = userManager.GetUserId(User);
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            var user =await userManager.GetUserAsync(User);
            var isAdmin =await userManager.IsInRoleAsync(user, "Admin");
            if (isAdmin)
            {
                return Ok(await _context.TblForms.ToListAsync());
            }
            else
            {
                var result = await _context.TblForms.Where(c=> c.UserId == user.Id).ToListAsync();
                return Ok(result);
            }
        }
        public IActionResult GetAllForms()
        {
            var userId = userManager.GetUserId(User);
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            return View();
        }
        public IActionResult LatestTemplateApi()
        {

            var result = _context.TblForms.OrderBy(x => x.CreatedOn).ToList();
            return Ok(result);
        }
        public IActionResult PopularFormApi()
        {
            var result = _context.TblForms.OrderBy(x => x.SubmissionCount).ToList();
            return Ok(result);
        }
        // GET: Forms/Details/5
        
        public async Task<IActionResult> Details(int FormId)
        {
            var userId = userManager.GetUserId(User);
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }


            var subMittedByUser = _context.TblFormSubmissionByUsers
    .FirstOrDefault(e => e.FormId == FormId); // Fetch the single record or null

             // Get the current user's ID

            if (subMittedByUser != null && subMittedByUser.UserId == userId) // Compare UserId safely
            {
                return RedirectToAction("AlreadySubmitted");
            }

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
            var userId = userManager.GetUserId(User);
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
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
        
        
        public async Task<IActionResult> Create(FullFormViewModel fullFormViewModel)
        {
            var userId = userManager.GetUserId(User);
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            try
            {
                if (ModelState.IsValid)
                {
                    // Ensure there's a question to add
                    if (!string.IsNullOrWhiteSpace(fullFormViewModel.TblQuestion?.Question))
                    {
                        var questionsList = HttpContext.Session.Get<List<TblQuestion>>("TblQuestionsList") ??
                                             new List<TblQuestion>();

                        if (fullFormViewModel.TblForm != null)
                        {
                            // Check if the form already exists by Title or another unique field (e.g., FormId)
                            var existingForm = await _context.TblForms
                                .FirstOrDefaultAsync(f => f.Title == fullFormViewModel.TblForm.Title);

                            TblForm formInsert;

                            if (existingForm == null)
                            {
                                // If no existing form, add the new form and get the entity
                                var formEntry = await _context.TblForms.AddAsync(fullFormViewModel.TblForm);
                                await _context.SaveChangesAsync();

                                // Get the entity from the EntityEntry
                                formInsert = formEntry.Entity;
                            }
                            else
                            {
                                // If the form already exists, use the existing form
                                formInsert = existingForm;
                            }

                            // Process the questions associated with the form
                            foreach (var tblQuestion in questionsList)
                            {
                                // Set the FormId for the question
                                tblQuestion.FormId = formInsert.FormId;

                                // Add the question
                                var isQuestionInsert = await _context.TblQuestions.AddAsync(tblQuestion);
                                await _context.SaveChangesAsync();

                                // Add options to the question if they exist
                                if (tblQuestion.tblQuestionOptionlList != null)
                                {
                                    foreach (var tblQuestionOption in tblQuestion.tblQuestionOptionlList)
                                    {
                                        // Set the QuestionId for the options
                                        tblQuestionOption.QuestionId = isQuestionInsert.Entity.QuestionId;
                                    }

                                    // Add the question options
                                    await _context.TblQuestionOptions.AddRangeAsync(tblQuestion.tblQuestionOptionlList);
                                    await _context.SaveChangesAsync();
                                }
                            }

                            // Save changes to the database (if any)
                            await _context.SaveChangesAsync();
                        }

                        return RedirectToAction("Index");
                    }
                }

                // If validation fails, return the form view with validation errors
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
            var userId = userManager.GetUserId(User);
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
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
            var userId = userManager.GetUserId(User);
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
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
            var userId = userManager.GetUserId(User);
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
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
            var userId = userManager.GetUserId(User);
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
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
            var userId = userManager.GetUserId(User);
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
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
            var userId = userManager.GetUserId(User);
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
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
        
        
        public async Task<IActionResult> AddOptionReset(FullFormViewModel fullFormViewModel, bool clearSession = false)
        {
            var userId = userManager.GetUserId(User);
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
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
            var userId = userManager.GetUserId(User);
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
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
            var userId = userManager.GetUserId(User);
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
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
            var userId = userManager.GetUserId(User);
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
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
        
        public async Task<IActionResult> Like(TblLike tblLike)
        {
            var userId = userManager.GetUserId(User);
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
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
            var userId = userManager.GetUserId(User);
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
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
        public IActionResult SubmitForm(int formId, Dictionary<int, string> answers, string UniqueId, string SubmittedBy)
        {
            var CurrentuserId = userManager.GetUserId(User);
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var userId = userManager.GetUserId(User);

            var CreatedBy = _context.TblForms.Find(formId).Createdby;
            if (answers != null && answers.Any())  // Ensure that answers are not null or empty
            {
                // Loop through the answers
                foreach (var answer in answers)
                {


                    // Create a new response entry for each question answered
                    var answerEntry = new TblResponse
                    {
                        FormId = formId,
                        QuestionId = answer.Key,
                        ResponseText = answer.Value,
                        SubmissionDate = DateTime.Now,
                        UserId = userId,
                        UniqueId = UniqueId,
                        CreatedBy = CreatedBy,
                        SubmittedBy = SubmittedBy
                    };

                    // Add the response to the context
                    _context.TblResponses.Add(answerEntry);
                    _context.SaveChanges();
                    var submittedByUser = new TblFormSubmissionByUser
                    {
                        UserId = userId,
                        FormId = formId,
                    };
                    _context.TblFormSubmissionByUsers.Add(submittedByUser);
                    _context.SaveChanges();
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
        
        public IActionResult AlreadySubmitted()
        {
            var userId = userManager.GetUserId(User);
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            return View();
        }
        
        public ActionResult ThankYou()
        {
            var userId = userManager.GetUserId(User);
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            return View();  // This will return the ThankYou.cshtml view
        }
        [HttpGet]
        
        public IActionResult SubmittedForms()
        {
            var userId = userManager.GetUserId(User);
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            return View();
        }
        [HttpGet]
        
        public async Task<IActionResult> SubmittedFormsApi()
        {
            var userId = userManager.GetUserId(User);
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            try
            {
                 // Assuming this is the identifier for the current user

                // Check if the current user is an admin
                var user = await userManager.GetUserAsync(User);
                var isAdmin = await userManager.IsInRoleAsync(user, "Admin");

                // Declare groupedSubmissions first
                 // Declare it as an empty list initially

                // Now, populate the groupedSubmissions based on role
                if (isAdmin)
                {
                    // If the user is an admin, get all data
                    var groupedSubmissions = _context.TblResponses
                        .GroupBy(r => r.UniqueId)
                        .Select(g => new
                        {
                            UniqueId = g.Key,
                            Title = g.FirstOrDefault().Form.Title,
                            CreatedBy = g.FirstOrDefault().CreatedBy,
                            SubmittedBy = g.FirstOrDefault().SubmittedBy,
                            SubmissionDate = g.FirstOrDefault().SubmissionDate
                        })
                        .ToList();
                    return Ok(groupedSubmissions);
                }
                else
                {
                    // If the user is not an admin, only get their own data
                   var groupedSubmissions = _context.TblResponses
                         .Where(r => r.UserId == user.Id)
                        .GroupBy(r => r.UniqueId)
                        
                        .Select(g => new
                        {
                            UniqueId = g.Key,
                            Title = g.FirstOrDefault().Form.Title,
                            CreatedBy = g.FirstOrDefault().CreatedBy,
                            SubmittedBy = g.FirstOrDefault().SubmittedBy,
                            SubmissionDate = g.FirstOrDefault().SubmissionDate
                        })
                        .ToList();
                    return Ok(groupedSubmissions);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                // Log exception (optional)
                return StatusCode(500, new { error = "An error occurred while retrieving submissions.", details = ex.Message });
            }
        }



        [HttpGet]
        
        public async Task<IActionResult>  ViewResponse(string uniqueId)
        {
            var userId = userManager.GetUserId(User);
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            if (string.IsNullOrWhiteSpace(uniqueId))
            {
                return BadRequest(new { Message = "UniqueId is required." });
            }

            var groupedSubmissions =await _context.TblResponses
                .Where(r => r.UniqueId == uniqueId)
                .Select(r => new
                {
                    r.Id,
                    r.Question.Question,
                    r.UniqueId,
                    r.Form.Title,
                    r.CreatedBy,
                    r.SubmittedBy,
                    r.SubmissionDate,
                    r.QuestionId,
                    r.ResponseText,
                    r.Form.Description
                })
                .ToArrayAsync();
            
            if (!groupedSubmissions.Any())
            {
                return NotFound(new { Message = $"No submissions found for UniqueId: {uniqueId}" });
            }

            // Creating ViewModel
            var formResponseViewModel = new ResponseViewModel
            {
                Title = groupedSubmissions.First().Title,
                Description = groupedSubmissions.First().Description,
                CreatedBy = groupedSubmissions.First().CreatedBy,
                SubmittedBy = groupedSubmissions.First().SubmittedBy,
                SubmissionDate = groupedSubmissions.First().SubmissionDate,
                tblResponses = groupedSubmissions.Select(r => new TblResponse
                {
                    // Include the Form object
                    Form = new TblForm { Title = r.Title, Description = r.Description },
                    Question = new TblQuestion
                    {
                        QuestionId = r.QuestionId,   // Assuming you have QuestionId
                        Question = r.Question        // The actual question text
                    },
                    ResponseText = r.ResponseText
                }).ToList()
            };

            return View(formResponseViewModel);
        }

        //[Authorize]
        //public IActionResult ViewResponse()
        //{
        //    return View();
        //}
        [Authorize]
        public IActionResult EditResponse(string uniqueId)
        {
            var userId = userManager.GetUserId(User);

            // Include related entities (Questions and their Options)
            var responses = _context.TblResponses
                .Include(r => r.Question)
                    .ThenInclude(q => q.TblQuestionOptions) // Include QuestionOptions
                .Include(r => r.Form) // Include Form for Title and Description
                .Where(e => e.UniqueId == uniqueId)
                .ToList();

            if (!responses.Any())
            {
                return View("Error", "No responses found for the provided unique ID.");
            }

            // Map the data to the view model
            var editResponseViewModels = responses.Select(res => new EditResponseviewModel
            {
                Id = res.Id,
                Title = res.Form?.Title ?? "N/A",
                Description = res.Form?.Description ?? "N/A",
                CreatedBy = res.CreatedBy,
                SubmittedBy = res.SubmittedBy,
                SubmissionDate = res.SubmissionDate,
                QuestionId = res.QuestionId,
                ResponseText = res.ResponseText,
                UniqueId = res.UniqueId,
                FormId = res.FormId,
                UserId = userId,
                OptionId = res.OptionId,
                Questions = res.Question, // Pass the Question entity
            }).ToList();

            return View(editResponseViewModels);
        }
        [HttpPost]
        
        public IActionResult EditResponse(List<EditResponseviewModel> model)
        {
            var userId = userManager.GetUserId(User);
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            if (model == null || !model.Any())
            {
                return View("Error", "No data provided for updating responses.");
            }

            foreach (var responseModel in model)
            {
                // Find the existing response in the database
                var existingResponse = _context.TblResponses
                    .Include(r => r.Question) // Include related question
                    .FirstOrDefault(r => r.Id == responseModel.Id);

                if (existingResponse == null)
                {
                    // Log or handle missing response error
                    continue; // Skip if the response is not found
                }

                // Update fields based on the model
                existingResponse.ResponseText = responseModel.ResponseText;
                existingResponse.OptionId = responseModel.OptionId; // For questions with options
                existingResponse.SubmissionDate = DateTime.Now; // Update submission date

                // Save changes for each response
                _context.TblResponses.Update(existingResponse);
            }

            // Commit all changes to the database
            _context.SaveChanges();

            return RedirectToAction("SubmittedForms"); // Redirect to a success page
        }
        [HttpGet]
        public async Task<IActionResult> EditForm(int FormId)
        {
            var userId = userManager.GetUserId(User);
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            var form = await _context.TblForms
                .Include(f => f.TblQuestions)
                .ThenInclude(q => q.TblQuestionOptions)
                .FirstOrDefaultAsync(f => f.FormId == FormId);

            if (form == null)
            {
                return NotFound(); // Handle case where form is not found
            }

            var viewModel = new EditFormViewModel
            {
                FormId = form.FormId,
                Title = form.Title,
                Description = form.Description,
                HeaderPhoto = form.HeaderPhoto,
                IsFavourite = form.IsFavourite,
                FormStatus = form.FormStatus,
                BackgroundColor = form.BackgroundColor,
                Email = form.Email,
                Name = form.Name,
                Status = form.Status,
                LastOpened = form.LastOpened,
                Createdby = form.Createdby,
                CreatedOn = form.CreatedOn,
                UpdatedBy = form.UpdatedBy,
                UpdatedOn = form.UpdatedOn,
                SubmissionCount = form.SubmissionCount,
                Likes = form.Likes,
                TblQuestions = form.TblQuestions
           .Select(q => new QuestionViewModel
           {
               QuestionId = q.QuestionId,
               Question = q.Question,
               QuestionType = q.QuestionType,
               tblQuestionOptionlList = q.TblQuestionOptions
                   .Select(o => new QuestionOptionViewModel
                   {
                       OptionId = o.OptionId,
                       OptionText = o.OptionText
                   })
                   .ToList()
           })
           .ToList()
            };

            return View(viewModel);
        }
        [HttpPost]
        
        public async Task<IActionResult> EditForm(EditFormViewModel model)
        {
            var userId = userManager.GetUserId(User);
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            if (!ModelState.IsValid)
            {
                return View(model); // Return the view with validation errors
            }

            var form = await _context.TblForms
                .Include(f => f.TblQuestions)
                .FirstOrDefaultAsync(f => f.FormId == model.FormId);

            if (form == null)
            {
                return NotFound(); // Handle case where form is not found
            }

            // Update form properties
            form.Title = model.Title;
            form.Description = model.Description;
            form.FormId = model.FormId;
            form.UpdatedOn = DateTime.Now;

            // Update questions
            form.TblQuestions = model.TblQuestions.Select(q => new TblQuestion
            {
                QuestionId = q.QuestionId,
                Question = q.Question,
                QuestionType = q.QuestionType,
                TblQuestionOptions = q.tblQuestionOptionlList.Select(o => new TblQuestionOption
                {
                    OptionId = o.OptionId,
                    OptionText = o.OptionText
                }).ToList()
            }).ToList();

            // Save changes
            _context.TblForms.Update(form);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index)); // Redirect to the list or detail view
        }

        public IActionResult PrivateDetailsPartial()
        {
            // Add any necessary data to the model
            return PartialView("_PrivateDetailsPartial");
        }

        public IActionResult AutoCompleteTagApi()
        {
            var result = _context.TblForms.ToList();
            return Ok(result);
        }
        public IActionResult AutoCompleteUserApi()
        {
            var result = userManager.Users.ToList();
            return Ok(result);
        }



    }
}












