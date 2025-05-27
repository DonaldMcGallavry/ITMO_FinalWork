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

namespace ITMO_FinalWork.Controllers
{
    public class RegistrationRegController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10;
        public RegistrationRegController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: RegistrationReg
        public async Task<IActionResult> Index(
           string sortOrder,
           string currentFilter,
           string searchString,
           int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParam"] = sortOrder == "name" ? "name_desc" : "name";
            ViewData["DateSortParam"] = sortOrder == "date" ? "date_desc" : "date";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var registrations = _context.RegistrationReg
                .Include(r => r.Passport)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                registrations = registrations.Where(r =>
                    r.LastName.Contains(searchString) ||
                    r.FirstName.Contains(searchString) ||
                    r.Passport.Series.Contains(searchString) ||
                    r.Passport.Number.Contains(searchString));
            }

            registrations = sortOrder switch
            {
                "name" => registrations.OrderBy(r => r.LastName),
                "name_desc" => registrations.OrderByDescending(r => r.LastName),
                "date" => registrations.OrderBy(r => r.RegistrationDate),
                "date_desc" => registrations.OrderByDescending(r => r.RegistrationDate),
                _ => registrations.OrderBy(r => r.LastName)
            };

            return View(await PaginatedList<RegistrationReg>.CreateAsync(registrations.AsNoTracking(), pageNumber ?? 1, PageSize));
        }

        // GET: RegistrationReg/Create
        public IActionResult Create()
        {
            ViewData["PassportId"] = new SelectList(_context.Passports, "PassportID", "PassportID");
            return View();
        }

        // POST: RegistrationReg/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("LastName,FirstName,MiddleName,BirthDate,Gender,RegistrationDate,RegistrationAddress," +
          "Passport")]
    RegistrationReg registrationReg)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    var existingPassport = await _context.Passports
                        .FirstOrDefaultAsync(p =>
                        p.Series == registrationReg.Passport.Series &&
                        p.Number == registrationReg.Passport.Number);

                    if (existingPassport != null)
                    {
                        registrationReg.PassportId = existingPassport.PassportID;
                        registrationReg.Passport = existingPassport;
                    }
                    else
                    {
                        if (registrationReg.Passport.ExpiryDate.HasValue &&
                            registrationReg.Passport.ExpiryDate <= registrationReg.Passport.IssueDate)
                        {
                            ModelState.AddModelError("Passport", "Дата окончания действия паспорта должна быть позже даты выдачи");
                            return View(registrationReg);
                        }

                        _context.Passports.Add(registrationReg.Passport);
                        await _context.SaveChangesAsync();
                        registrationReg.PassportId = registrationReg.Passport.PassportID;
                        registrationReg.Passport = existingPassport;
                    }
                    registrationReg.Status = "IsRegistrated";
                    _context.Add(registrationReg);
                    await _context.SaveChangesAsync();
                    
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Ошибка сохранения: " + ex.Message);
            }

            ViewData["PassportId"] = new SelectList(_context.Passports, "PassportID", "PassportID");
            return View(registrationReg);
        }

        // GET: RegistrationReg/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var registrationReg = await _context.RegistrationReg
                .Include(m => m.Passport)
                .FirstOrDefaultAsync(m => m.RecordID == id);

            if (registrationReg == null)
            {
                return NotFound();
            }
            return View(registrationReg);
        }

        // POST: RegistrationReg/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
        [Bind("RecordID,LastName,FirstName,MiddleName,BirthDate,Gender,RegistrationDate," +
        "RegistrationAddress,Passport,PassportId")]
        RegistrationReg registrationReg)
        {
            if (id != registrationReg.RecordID)
            {
                return NotFound();
            }
            var existingReg = await _context.RegistrationReg
            .Include(r => r.Passport)
            .FirstOrDefaultAsync(r => r.RecordID == id);

            if (existingReg == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Проверка изменения паспортных данных
                    bool passportChanged = existingReg.Passport.Series != registrationReg.Passport.Series
                                        || existingReg.Passport.Number != registrationReg.Passport.Number;

                    // Обработка паспорта
                    if (passportChanged)
                    {
                        var existingPassport = await _context.Passports
                            .FirstOrDefaultAsync(p => p.Series == registrationReg.Passport.Series
                                                   && p.Number == registrationReg.Passport.Number);

                        if (existingPassport != null)
                        {
                            // Используем существующий паспорт
                            existingReg.PassportId = existingPassport.PassportID;
                            existingReg.Passport = existingPassport;
                        }
                        else
                        {
                            // Создаем новый паспорт
                            var newPassport = new Passport
                            {
                                Series = registrationReg.Passport.Series,
                                Number = registrationReg.Passport.Number,
                                IssueDate = registrationReg.Passport.IssueDate,
                                ExpiryDate = registrationReg.Passport.ExpiryDate
                            };

                            // Валидация дат паспорта
                            if (newPassport.ExpiryDate.HasValue && newPassport.ExpiryDate <= newPassport.IssueDate)
                            {
                                ModelState.AddModelError("Passport",
                                    "Дата окончания действия должна быть позже даты выдачи");
                                return View(registrationReg);
                            }

                            _context.Passports.Add(newPassport);
                            await _context.SaveChangesAsync(); 
                            existingReg.PassportId = newPassport.PassportID;
                        }
                    }
                    else
                    {
                        // Обновляем данные текущего паспорта
                        existingReg.Passport.IssueDate = registrationReg.Passport.IssueDate;
                        existingReg.Passport.ExpiryDate = registrationReg.Passport.ExpiryDate;

                        if (existingReg.Passport.ExpiryDate.HasValue
                            && existingReg.Passport.ExpiryDate <= existingReg.Passport.IssueDate)
                        {
                            ModelState.AddModelError("Passport",
                                "Дата окончания действия должна быть позже даты выдачи");
                            return View(registrationReg);
                        }
                    }

                    // Обновление основных полей регистрации
                    existingReg.LastName = registrationReg.LastName;
                    existingReg.FirstName = registrationReg.FirstName;
                    existingReg.MiddleName = registrationReg.MiddleName;
                    existingReg.BirthDate = registrationReg.BirthDate;
                    existingReg.Gender = registrationReg.Gender;
                    existingReg.RegistrationDate = registrationReg.RegistrationDate;
                    existingReg.RegistrationAddress = registrationReg.RegistrationAddress;

                    _context.Update(existingReg);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Ошибка сохранения: " + ex.InnerException?.Message);
                }
            }

            ViewData["PassportId"] = new SelectList(_context.Passports, "PassportID", "PassportID",
                registrationReg.PassportId);
            return View(registrationReg);
        }

        // GET: RegistrationReg/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var registrationReg = await _context.RegistrationReg
                .Include(r => r.Passport)
                .FirstOrDefaultAsync(m => m.RecordID == id);
            if (registrationReg == null)
            {
                return NotFound();
            }

            return View(registrationReg);
        }

        // POST: RegistrationReg/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var registrationReg = await _context.RegistrationReg.FindAsync(id);
            if (registrationReg != null)
            {
                _context.RegistrationReg.Remove(registrationReg);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var registrationReg = await _context.RegistrationReg
                 .Include(r => r.Passport)
                .FirstOrDefaultAsync (r => r.RecordID == id);
            if (registrationReg == null)
            {
                return NotFound();
            }
            return View(registrationReg);
        }
    }
}
