﻿using System;
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
    public class FormsController : Controller
    {
        private readonly DbContext _context;

        public FormsController(DbContext context)
        {
            _context = context;
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
            return View();
        }

        // POST: Forms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TblForm tblForm)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tblForm);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tblForm);
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
    }
}