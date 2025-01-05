using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public FormsController(DbContext context, IFormService formService, ILogger<FormsController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            this.formService = formService;
            this.logger = logger;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.TblForms.ToListAsync());
        }

        public async Task<IActionResult> GetAllFormsApi()
        {
            var userId = userManager.GetUserId(User);
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            var user = await userManager.GetUserAsync(User);
            var isAdmin = await userManager.IsInRoleAsync(user, "Admin");
            if (isAdmin)
            {
                return Ok(await _context.TblForms.ToListAsync());
            }
            else
            {
                var result = await _context.TblForms.Where(c => c.UserId == user.Id).ToListAsync();
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

        public async Task<IActionResult> Details(int FormId)
        {
            var userId = userManager.GetUserId(User);
            var isPrivateUser = _context.TblPrivateUsers
                            .Any(pu => pu.UserId == userId && pu.FormId == FormId);
            var form = _context.TblForms.Where(c => c.FormId == FormId).FirstOrDefault();
            if (!form.IsPublic && !isPrivateUser)
            {
                return RedirectToPage("/Account/AccessDenied", new { area = "Identity" });
            }
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var subMittedByUser = _context.TblFormSubmissionByUsers
    .FirstOrDefault(e => e.FormId == FormId);

            if (subMittedByUser != null && subMittedByUser.UserId == userId)
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

            var user = await userManager.GetUserAsync(User);
            bool isLikedByCurrentUser = false;

            if (user != null)
            {
                isLikedByCurrentUser = tblForm.TblLikes.Any(l => l.UserId == user.Id);
            }

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

        public IActionResult Create()
        {
            var userId = userManager.GetUserId(User);
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            var appUsers = userManager.Users
        .Select(u => new SelectListItem
        {
            Value = u.Id, 
            Text = u.Id   
        }).ToList()
        .ToList();
            
            var fullForm = new FullFormViewModel
            {
                QuestionType = _context.TblKeywordMasters.Where(c => c.KeywordType == "QuestionType").ToList(),
                appUsers = appUsers,
               
            };

            return View(fullForm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(FullFormViewModel fullFormViewModel, List<string> states, List<string>  tags)
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
                    if (!string.IsNullOrWhiteSpace(fullFormViewModel.TblQuestion?.Question))
                    {
                        var questionsList = HttpContext.Session.Get<List<TblQuestion>>("TblQuestionsList") ??
                                             new List<TblQuestion>();

                        if (fullFormViewModel.TblForm != null)
                        {
                            var existingForm = await _context.TblForms
                                .FirstOrDefaultAsync(f => f.Title == fullFormViewModel.TblForm.Title);

                            TblForm formInsert;

                            if (existingForm == null)
                            {
                                var formEntry = await _context.TblForms.AddAsync(fullFormViewModel.TblForm);
                                await _context.SaveChangesAsync();
                                formInsert = formEntry.Entity;
                            }
                            else
                            {
                                formInsert = existingForm;
                            }

                            foreach (var tblQuestion in questionsList)
                            {
                                tblQuestion.FormId = formInsert.FormId;

                                var isQuestionInsert = await _context.TblQuestions.AddAsync(tblQuestion);
                                await _context.SaveChangesAsync();

                                if (tblQuestion.tblQuestionOptionlList != null)
                                {
                                    foreach (var tblQuestionOption in tblQuestion.tblQuestionOptionlList)
                                    {
                                        tblQuestionOption.QuestionId = isQuestionInsert.Entity.QuestionId;
                                    }

                                    await _context.TblQuestionOptions.AddRangeAsync(tblQuestion.tblQuestionOptionlList);
                                    await _context.SaveChangesAsync();
                                }
                            }
                            if (states != null && states.Any())
                            {
                                foreach (var stateId in states)
                                {
                                    var tblPrivateUsers = new TblPrivateUser
                                    {
                                        UserId = stateId,
                                        FormId = fullFormViewModel.TblForm.FormId
                                    };

                                    _context.Add(tblPrivateUsers);
                                    await _context.SaveChangesAsync();
                                }
                            }

                            //if (tags != null && tags.Any())
                            //{
                            //    // Deserialize the JSON into a list of TblTag objects
                            //    //var tagList = JsonConvert.DeserializeObject<List<TblTag>>(tags);

                            //    // Extract just the values (e.g., "j" and "k")
                            //    //var values = tagList.Select(tag => tag.Tag).ToList();

                            //    // Loop through the list of values and save each tag
                            //    foreach (var value in values)
                            //    {
                            //        var tbltag = new TblTag
                            //        {

                            //            Tag = value,  // Assign the individual string value from the values list
                            //            FormId = fullFormViewModel.TblForm.FormId
                            //        };
                                    
                            //        _context.Add(tbltag);
                            //        await _context.SaveChangesAsync();
                            //    }
                            //}

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

                if (!string.IsNullOrWhiteSpace(fullFormViewModel.TblQuestion?.Question))
                {
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
            HttpContext.Session.Clear();
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
                tblComment.Commented_On = DateTime.Now;

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
                    return RedirectToAction("Details", "Forms", new { FormId = tblComment.FormId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while adding the comment.");
                }
            }

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
                .OrderByDescending(c => c.Commented_On)
                .Select(c => new
                {
                    c.Id,
                    c.UserId,
                    c.Comment,
                    CommentedOn = c.Commented_On
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

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var form = _context.TblForms.FirstOrDefault(f => f.FormId == tblLike.FormId);
            if (form == null)
            {
                return NotFound("Form not found.");
            }

            _context.TblLikes.Add(tblLike);
            form.Likes++;

            _context.SaveChanges();

            return RedirectToAction("Details", "Forms", new { FormId = tblLike.FormId });
        }

        public async Task<IActionResult> Unlike(int FormId)
        {
            var userId = userManager.GetUserId(User);
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            var user = await userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            var like = _context.TblLikes
                .Where(l => l.FormId == FormId && l.UserId == user.Id)
                .FirstOrDefault();

            if (like == null)
            {
                return NotFound("Like not found.");
            }

            _context.TblLikes.Remove(like);

            var form = await _context.TblForms.FirstOrDefaultAsync(f => f.FormId == FormId);
            if (form != null)
            {
                form.Likes--;
                _context.TblForms.Update(form);
            }

            await _context.SaveChangesAsync();

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
            if (answers != null && answers.Any())
            {
                foreach (var answer in answers)
                {
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

                var tblForm = _context.TblForms.FirstOrDefault(f => f.FormId == formId);
                if (tblForm != null)
                {
                    tblForm.SubmissionCount++;
                    _context.SaveChanges();
                }

                _context.SaveChanges();

                return RedirectToAction("ThankYou");
            }

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
            return View();
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
                var user = await userManager.GetUserAsync(User);
                var isAdmin = await userManager.IsInRoleAsync(user, "Admin");

                if (isAdmin)
                {
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
                return StatusCode(500, new { error = "An error occurred while retrieving submissions.", details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewResponse(string uniqueId)
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

            var groupedSubmissions = await _context.TblResponses
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

            var formResponseViewModel = new ResponseViewModel
            {
                Title = groupedSubmissions.First().Title,
                Description = groupedSubmissions.First().Description,
                CreatedBy = groupedSubmissions.First().CreatedBy,
                SubmittedBy = groupedSubmissions.First().SubmittedBy,
                SubmissionDate = groupedSubmissions.First().SubmissionDate,
                tblResponses = groupedSubmissions.Select(r => new TblResponse
                {
                    Form = new TblForm { Title = r.Title, Description = r.Description },
                    Question = new TblQuestion
                    {
                        QuestionId = r.QuestionId,
                        Question = r.Question
                    },
                    ResponseText = r.ResponseText
                }).ToList()
            };

            return View(formResponseViewModel);
        }

        [Authorize]
        public IActionResult EditResponse(string uniqueId)
        {
            var userId = userManager.GetUserId(User);

            var responses = _context.TblResponses
                .Include(r => r.Question)
                    .ThenInclude(q => q.TblQuestionOptions)
                .Include(r => r.Form)
                .Where(e => e.UniqueId == uniqueId)
                .ToList();

            if (!responses.Any())
            {
                return View("Error", "No responses found for the provided unique ID.");
            }

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
                Questions = res.Question,
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
                var existingResponse = _context.TblResponses
                    .Include(r => r.Question)
                    .FirstOrDefault(r => r.Id == responseModel.Id);

                if (existingResponse == null)
                {
                    continue;
                }

                existingResponse.ResponseText = responseModel.ResponseText;
                existingResponse.OptionId = responseModel.OptionId;
                existingResponse.SubmissionDate = DateTime.Now;

                _context.TblResponses.Update(existingResponse);
            }

            _context.SaveChanges();

            return RedirectToAction("SubmittedForms");
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
                return NotFound();
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
                return View(model);
            }

            var form = await _context.TblForms
                .Include(f => f.TblQuestions)
                .FirstOrDefaultAsync(f => f.FormId == model.FormId);

            if (form == null)
            {
                return NotFound();
            }

            form.Title = model.Title;
            form.Description = model.Description;
            form.FormId = model.FormId;
            form.UpdatedOn = DateTime.Now;

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

            _context.TblForms.Update(form);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult PrivateDetailsPartial()
        {
            return PartialView("_PrivateDetailsPartial");
        }

        public IActionResult AutoCompleteTagApi()
        {
            var result = _context.TblTags.ToList();
            return Ok(result);
        }

        public IActionResult AutoCompleteUserApi()
        {
            var result = userManager.Users.ToList();
            return View("Create" , result);
        }
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> DeleteForm(int FormId)
        {
            var form = await _context.TblForms.FindAsync(FormId);

            if (form == null)
            {
                return NotFound();
            }

            // Remove the form and save changes
            _context.TblForms.Remove(form);
            await _context.SaveChangesAsync();

            // Optionally, you can redirect to another action or return a JSON response
            return RedirectToAction(nameof(GetAllForms)); // or any other action you want to redirect to after deleting
        }



    }
}











