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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Xml.Linq;

namespace ITMO_FinalWork.Controllers
{
    public class MigrationRegController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10;

        public MigrationRegController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MigrationReg
        public async Task<IActionResult> Index(
       string sortOrder,
       string searchString,
       string currentFilter,
       int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParam"] = sortOrder == "name" ? "name_desc" : "name";
            ViewData["DateSortParam"] = sortOrder == "date" ? "date_desc" : "date";
            ViewData["StatusSortParam"] = sortOrder == "status" ? "status_desc" : "status";
            ViewData["CurrentFilter"] = searchString;

            var migrationRegs = _context.MigrationReg
                .Include(m => m.Passport)
                .Include(m => m.Reason)
                .Include(m => m.Card)
                .AsQueryable();

            migrationRegs = sortOrder switch
            {
                "name" => migrationRegs.OrderBy(m => m.LastName),
                "name_desc" => migrationRegs.OrderByDescending(m => m.LastName),
                "date" => migrationRegs.OrderBy(m => m.EntryDate),
                "date_desc" => migrationRegs.OrderByDescending(m => m.EntryDate), 
                "status" => migrationRegs.OrderBy(r => r.Status == "IsRegistrated"),
                "status_desc" => migrationRegs.OrderByDescending(r => r.Status == "IsRegistrated"),
                _ => migrationRegs.OrderBy(m => m.LastName)
            };

            if (!string.IsNullOrEmpty(searchString))
            {
                migrationRegs = migrationRegs.Where(m =>
                    m.LastName.Contains(searchString) ||
                    m.FirstName.Contains(searchString) ||
                    m.Passport.Series.Contains(searchString) ||
                    m.Passport.Number.Contains(searchString));
            }

            return View(await PaginatedList<MigrationReg>.CreateAsync(
                migrationRegs.AsNoTracking(),
                pageNumber ?? 1,
                PageSize,
                currentSort: sortOrder,
                entityType: "MigrationReg"));
        }

        // GET: MigrationReg/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var migrationReg = await _context.MigrationReg
                .Include(m => m.Card)
                .Include(m => m.Passport)
                .Include(m => m.Reason)
                .FirstOrDefaultAsync(m => m.MigrantID == id);
            if (migrationReg == null)
            {
                return NotFound();
            }

            return View(migrationReg);
        }

        // GET: MigrationReg/Create
        public IActionResult Create()
        {
            ViewData["CardId"] = new SelectList(_context.MigrationCards, "CardId", "CardNumber");
            ViewData["PassportId"] = new SelectList(_context.Passports, "PassportId", "Number");
            ViewData["ReasonId"] = new SelectList(_context.StayReasons, "ReasonId", "ReasonId");
            return View();
        }

        // POST: MigrationReg/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
    [Bind("LastName,FirstName,MiddleName,BirthDate,Gender,BirthCountry,Citizenship," +
          "EntryDate,RegistrationDate,RegistrationEndDate,RegistrationAddress," +
          "Passport,Card,Reason")]
    MigrationReg migrationReg)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    var existingPassport = await _context.Passports
                        .FirstOrDefaultAsync(p =>
                            p.Series == migrationReg.Passport.Series &&
                            p.Number == migrationReg.Passport.Number);
                    
                    if (existingPassport != null)
                    {
                        migrationReg.PassportId = existingPassport.PassportID;
                        migrationReg.Passport = existingPassport;
                    }
                    else
                    {
                        if (migrationReg.Passport.ExpiryDate.HasValue && migrationReg.Passport.ExpiryDate
                                                <= migrationReg.Passport.IssueDate)
                        {
                            ModelState.AddModelError("", "Дата окончания действия паспорта должна быть позже даты выдачи");
                            return View(migrationReg);
                        }
                        _context.Add(migrationReg.Passport);
                        await _context.SaveChangesAsync();
                        migrationReg.PassportId = migrationReg.Passport.PassportID;
                    }
                    
                    if ( migrationReg.RegistrationDate
                       <= migrationReg.EntryDate)
                    {
                        ModelState.AddModelError("", "Дата регистрации должна быть позже даты въезда");
                        return View(migrationReg);
                    }
                    if (migrationReg.RegistrationEndDate.HasValue && migrationReg.RegistrationEndDate
                       <= migrationReg.RegistrationDate)
                    {
                        ModelState.AddModelError("", "Дата окончания действия регистрации должна быть позже даты регистрации");
                        return View(migrationReg);
                    }

                    var existingCard = await _context.MigrationCards
                        .FirstOrDefaultAsync(p =>
                            p.CardSeries == migrationReg.Card.CardSeries &&
                            p.CardNumber == migrationReg.Card.CardNumber);

                    if (existingCard != null)
                    {
                        migrationReg.CardId = existingCard.CardID;
                        migrationReg.Card = existingCard;
                    }
                    else
                    {
                        if (migrationReg.Card.ExpiryDate.HasValue && migrationReg.Card.ExpiryDate
                                                <= migrationReg.Card.IssueDate)
                        {
                            ModelState.AddModelError("", "Дата окончания действия миграционной карты " +
                                "должна быть позже даты выдачи");
                            return View(migrationReg);
                        }
                        _context.Add(migrationReg.Card);
                        await _context.SaveChangesAsync();
                        migrationReg.CardId = migrationReg.Card.CardID;
                    }

                    _context.Add(migrationReg.Reason);
                    await _context.SaveChangesAsync();
                    migrationReg.ReasonId = migrationReg.Reason?.ReasonID;

                    // Сохранение основной записи
                    migrationReg.Status = "IsRegistrated";
                    _context.Add(migrationReg);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Ошибка сохранения: " + ex.Message);
            }

            ViewData["CardId"] = new SelectList(_context.MigrationCards, "CardId", "CardNumber");
            ViewData["PassportId"] = new SelectList(_context.Passports, "PassportId", "Number");
            ViewData["ReasonId"] = new SelectList(_context.StayReasons, "ReasonId", "ReasonType");
            return View(migrationReg);
        }
        // GET: MigrationReg/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var migrationReg = await _context.MigrationReg
                .Include(m => m.Passport)      
                .Include(m => m.Card)         
                .Include(m => m.Reason)       
                .FirstOrDefaultAsync(m => m.MigrantID == id);
            if (migrationReg == null) 
            {
                return NotFound();
            }
            ViewData["CardId"] = new SelectList(_context.MigrationCards, "CardID", "CardNumber", migrationReg.CardId);
            ViewData["PassportId"] = new SelectList(_context.Passports, "PassportID", "Number", migrationReg.PassportId);
            ViewData["ReasonId"] = new SelectList(_context.StayReasons, "ReasonID", "ReasonType", migrationReg.ReasonId);
            return View(migrationReg);
        }

        // POST: MigrationReg/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
        [Bind("MigrantID,LastName,FirstName,MiddleName,BirthDate,Gender,BirthCountry,Citizenship," +
          "EntryDate,RegistrationDate,RegistrationEndDate,RegistrationAddress,Status," +
          "Passport,PassportId,ReasonId,CardId")]
        MigrationReg migrationReg)
        {
            if (id != migrationReg.MigrantID)
            {
                return NotFound();
            }

            var existingReg = await _context.MigrationReg
                .Include(m => m.Passport)
                .Include(m => m.Reason)
                .Include(m => m.Card)
                .FirstOrDefaultAsync(m => m.MigrantID == id);

            if (existingReg == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    bool passportChanged = existingReg.Passport.Series != migrationReg.Passport.Series
                                         || existingReg.Passport.Number != migrationReg.Passport.Number;

                    if (passportChanged)
                    {
                        var existingPassport = await _context.Passports
                            .FirstOrDefaultAsync(p =>
                                p.Series == migrationReg.Passport.Series &&
                                p.Number == migrationReg.Passport.Number);

                        if (existingPassport != null)
                        {
                            existingReg.PassportId = existingPassport.PassportID;
                            existingReg.Passport = existingPassport;
                        }
                        else
                        {
                            var newPassport = new Passport
                            {
                                Series = migrationReg.Passport.Series,
                                Number = migrationReg.Passport.Number,
                                IssueDate = migrationReg.Passport.IssueDate,
                                ExpiryDate = migrationReg.Passport.ExpiryDate
                            };

                            if (newPassport.ExpiryDate.HasValue &&
                                newPassport.ExpiryDate <= newPassport.IssueDate)
                            {
                                ModelState.AddModelError("Passport",
                                    "Дата окончания действия должна быть позже даты выдачи");
                                return View(migrationReg);
                            }

                            _context.Passports.Add(newPassport);
                            await _context.SaveChangesAsync();
                            existingReg.PassportId = newPassport.PassportID;
                        }
                    }
                    else
                    {
                        existingReg.Passport.IssueDate = migrationReg.Passport.IssueDate;
                        existingReg.Passport.ExpiryDate = migrationReg.Passport.ExpiryDate;

                        if (existingReg.Passport.ExpiryDate.HasValue &&
                            existingReg.Passport.ExpiryDate <= existingReg.Passport.IssueDate)
                        {
                            ModelState.AddModelError("Passport",
                                "Дата окончания действия должна быть позже даты выдачи");
                            return View(migrationReg);
                        }
                    }

                    existingReg.LastName = migrationReg.LastName;
                    existingReg.FirstName = migrationReg.FirstName;
                    existingReg.MiddleName = migrationReg.MiddleName;
                    existingReg.BirthDate = migrationReg.BirthDate;
                    existingReg.Gender = migrationReg.Gender;
                    existingReg.BirthCountry = migrationReg.BirthCountry;
                    existingReg.Citizenship = migrationReg.Citizenship;
                    existingReg.EntryDate = migrationReg.EntryDate;
                    existingReg.RegistrationDate = migrationReg.RegistrationDate;
                    existingReg.RegistrationEndDate = migrationReg.RegistrationEndDate;
                    existingReg.RegistrationAddress = migrationReg.RegistrationAddress;
                    existingReg.Status = migrationReg.Status;
                    existingReg.ReasonId = migrationReg.ReasonId;
                    existingReg.CardId = migrationReg.CardId;

                    if (existingReg.RegistrationEndDate.HasValue &&
                        existingReg.RegistrationEndDate <= existingReg.RegistrationDate)
                    {
                        ModelState.AddModelError("RegistrationEndDate",
                            "Дата окончания регистрации должна быть позже даты регистрации");
                        return View(migrationReg);
                    }
                    if(existingReg.Card.ExpiryDate.HasValue && 
                       existingReg.Card.ExpiryDate <= existingReg.RegistrationDate)
                    {
                        ModelState.AddModelError("", "Дата окончания действия миграционной карты " +
                                "должна быть позже даты выдачи");
                        return View(migrationReg);
                    }

                    _context.Update(existingReg);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Ошибка сохранения: " + ex.InnerException?.Message);
                }
            }

            // Заполнение списков для выпадающих меню
            ViewData["CardId"] = new SelectList(_context.MigrationCards, "CardID", "Number", migrationReg.CardId);
            ViewData["ReasonId"] = new SelectList(_context.StayReasons, "ReasonID", "Name", migrationReg.ReasonId);
            ViewData["PassportId"] = new SelectList(_context.Passports, "PassportID", "Series", migrationReg.PassportId);

            return View(migrationReg);
        }

        // GET: MigrationReg/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var migrationReg = await _context.MigrationReg
                .Include(m => m.Card)
                .Include(m => m.Passport)
                .Include(m => m.Reason)
                .FirstOrDefaultAsync(m => m.MigrantID == id);
            if (migrationReg == null)
            {
                return NotFound();
            }

            return View(migrationReg);
        }

        // POST: MigrationReg/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var migrationReg = await _context.MigrationReg.FindAsync(id);
            if (migrationReg != null)
            {
                _context.MigrationReg.Remove(migrationReg);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var migrationReg = await _context.MigrationReg
                .Include(m => m.Card)
                .Include(m => m.Passport)
                .Include(m => m.Reason)
                .FirstOrDefaultAsync(m => m.MigrantID == id);

            if (migrationReg != null)
            {
                migrationReg.Status = "Removed";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
