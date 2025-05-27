using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITMO_FinalWork.Data;
using ITMO_FinalWork.Models;
using ITMO_FinalWork.Utilities;
using System.Drawing.Printing;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ITMO_FinalWork.Controllers
{
    public class PassportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10;

        public PassportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Passports
        public async Task<IActionResult> Index(
    string sortOrder,
    string currentFilter,
    string series,
    string number,
    int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["SeriesSortParam"] = sortOrder == "series" ? "series_desc" : "series";
            ViewData["NumberSortParam"] = sortOrder == "number" ? "number_desc" : "number";
            ViewData["DateSortParam"] = sortOrder == "date" ? "date_desc" : "date";

            // Сохраняем параметры поиска для View
            ViewData["CurrentSeries"] = series;
            ViewData["CurrentNumber"] = number;

            var passports = _context.Passports.AsQueryable();

            // Применяем фильтрацию
            if (!string.IsNullOrEmpty(series) || !string.IsNullOrEmpty(number))
            {
                passports = passports.Where(p =>
                    (string.IsNullOrEmpty(series) || p.Series == series) &&
                    (string.IsNullOrEmpty(number) || p.Number == number));
            }

            // Сортировка
            passports = sortOrder switch
            {
                "series" => passports.OrderBy(p => p.Series),
                "series_desc" => passports.OrderByDescending(p => p.Series),
                "number" => passports.OrderBy(p => p.Number),
                "number_desc" => passports.OrderByDescending(p => p.Number),
                "date" => passports.OrderBy(p => p.IssueDate),
                "date_desc" => passports.OrderByDescending(p => p.IssueDate),
                _ => passports.OrderBy(p => p.Series)
            };

            int pageSize = 10;
            return View(await PaginatedList<Passport>.CreateAsync(
                passports.AsNoTracking(),
                pageNumber ?? 1,
                pageSize,
                currentFilter: currentFilter,
                currentSort: sortOrder,
                entityType: "Passport"));
        }

        // GET: Passports/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var passport = await _context.Passports
                .FirstOrDefaultAsync(m => m.PassportID == id);
            if (passport == null)
            {
                return NotFound();
            }

            return View(passport);
        }

        // GET: Passports/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Passports/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PassportId,Series,Number,IssueDate,ExpiryDate")] Passport passport)
        {
            if (ModelState.IsValid)
            {
                // Проверка на существование паспорта
                if (await _context.Passports.AnyAsync(p =>
                    p.Series == passport.Series &&
                    p.Number == passport.Number))
                {
                    ModelState.AddModelError("", "Паспорт с такой серией и номером уже существует");
                    return View(passport);
                }
                if (passport.ExpiryDate.HasValue && passport.ExpiryDate <= passport.IssueDate)
                {
                    ModelState.AddModelError("ExpiryDate", "Дата окончания действия должна быть позже даты выдачи");
                    return View(passport);
                }
                _context.Add(passport);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            return View(passport);
        }

        // GET: Passports/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var passport = await _context.Passports.FindAsync(id);
            if (passport == null)
            {
                return NotFound();
            }
            return View(passport);
        }

        // POST: Passports/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
        [Bind("PassportID,Series,Number,IssueDate,ExpiryDate")]
        Passport passport)
        {
            if (id != passport.PassportID)
            {
                return NotFound();
            }

            var existingPassport = await _context.Passports
                .FirstOrDefaultAsync(p => p.PassportID == id);

            if (existingPassport == null)
            {
                return NotFound();
            }

            var originalSeries = existingPassport.Series;
            var originalNumber = existingPassport.Number;

            if (ModelState.IsValid)
            {
                try
                {
                    if (existingPassport.Series != passport.Series ||
                        existingPassport.Number != passport.Number)
                    {
                        var duplicatePassport = await _context.Passports
                            .FirstOrDefaultAsync(p =>
                                p.Series == passport.Series &&
                                p.Number == passport.Number);

                        if (duplicatePassport != null)
                        {
                            ModelState.AddModelError("",
                                "Паспорт с такой серией и номером уже существует");
                            return View(passport);
                        }
                    }

                    if (passport.ExpiryDate.HasValue &&
                        passport.ExpiryDate <= passport.IssueDate)
                    {
                        ModelState.AddModelError("ExpiryDate",
                            "Дата окончания действия должна быть позже даты выдачи");
                        return View(passport);
                    }

                    existingPassport.Series = passport.Series;
                    existingPassport.Number = passport.Number;
                    existingPassport.IssueDate = passport.IssueDate;
                    existingPassport.ExpiryDate = passport.ExpiryDate;

                    _context.Update(existingPassport);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Ошибка сохранения: " +
                        ex.InnerException?.Message);
                }
            }

            return View(passport);
        }
        //public async Task<IActionResult> Edit(int id, 
        //[Bind("PassportID,Series,Number,IssueDate,ExpiryDate")]
        //Passport passport)
        //{
        //    if (id != passport.PassportID)
        //    {
        //        return NotFound();
        //    }

        //    var existingPassport = await _context.Passports
        //    .FirstOrDefaultAsync(p => p.PassportID == id);

        //    var originalSeries = existingPassport.Series;
        //    var originalNumber = existingPassport.Number;

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            if (existingPassport.Series != passport.Series ||
        //         existingPassport.Number != passport.Number)
        //            {
        //                // Проверка на существование паспорта с новыми реквизитами
        //                var duplicatePassport = await _context.Passports
        //                    .FirstOrDefaultAsync(p =>
        //                        p.Series == passport.Series &&
        //                        p.Number == passport.Number);

        //                if (duplicatePassport != null)
        //                {
        //                    ModelState.AddModelError("",
        //                        "Паспорт с такой серией и номером уже существует");
        //                    return View(passport);
        //                }
        //            }

        //            if (passport.ExpiryDate.HasValue &&
        //                passport.ExpiryDate <= passport.IssueDate)
        //            {
        //                ModelState.AddModelError("ExpiryDate",
        //                    "Дата окончания действия должна быть позже даты выдачи");
        //                return View(passport);
        //            }
        //            existingPassport.Series = passport.Series;
        //            existingPassport.Number = passport.Number;
        //            existingPassport.IssueDate = passport.IssueDate;
        //            existingPassport.ExpiryDate = passport.ExpiryDate;

        //            _context.Update(passport);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateException ex)
        //        {
        //            ModelState.AddModelError("", "Ошибка сохранения: " +
        //                ex.InnerException?.Message);
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(passport);
        //}

        // GET: Passports/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var passport = await _context.Passports
                .FirstOrDefaultAsync(m => m.PassportID == id);
            if (passport == null)
            {
                return NotFound();
            }

            return View(passport);
        }

        // POST: Passports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var passport = await _context.Passports.FindAsync(id);
            if (passport != null)
            {
                _context.Passports.Remove(passport);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PassportExists(int id)
        {
            return _context.Passports.Any(e => e.PassportID == id);
        }
    }
}
