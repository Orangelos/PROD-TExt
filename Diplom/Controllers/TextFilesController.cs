using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Diplom.Models;
using Diplom.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Diplom.Controllers
{
    public class TextFilesController : Controller
    {
        private readonly DiplomDbContext _context;
        private static readonly string[] AllowedExtensions = { ".txt", ".docx", ".pdf" };

        public TextFilesController(DiplomDbContext context)
        {
            _context = context;
        }

        // GET: TextFiles
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;
            var files = _context.TextFiles.AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                files = files.Where(f => f.FileName.Contains(searchString));
            }
            return View(await files.OrderByDescending(f => f.UploadedAt).ToListAsync());
        }

        // GET: TextFiles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var textFile = await _context.TextFiles.FirstOrDefaultAsync(m => m.Id == id);
            if (textFile == null) return NotFound();
            return View(textFile);
        }

        // GET: TextFiles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TextFiles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile uploadedFile)
        {
            try
            {
                if (uploadedFile == null || uploadedFile.Length == 0)
                {
                    ModelState.AddModelError("", "Пожалуйста, выберите файл.");
                    return View();
                }

                string extension = Path.GetExtension(uploadedFile.FileName).ToLower();
                if (!AllowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("", "Разрешены только файлы .txt, .docx, .pdf");
                    return View();
                }

                string content;
                using (var stream = uploadedFile.OpenReadStream())
                {
                    content = FileTextExtractor.ExtractText(stream, extension);
                }

                var textFile = new TextFile
                {
                    FileName = uploadedFile.FileName,
                    Content = content,
                    UploadedAt = DateTime.Now
                };

                _context.Add(textFile);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Ошибка при загрузке файла: " + ex.Message);
                return View();
            }
        }

        // GET: TextFiles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var textFile = await _context.TextFiles.FindAsync(id);
            if (textFile == null) return NotFound();
            return View(textFile);
        }

        // POST: TextFiles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormFile? uploadedFile)
        {
            var textFile = await _context.TextFiles.FindAsync(id);
            if (textFile == null) return NotFound();

            if (uploadedFile != null && uploadedFile.Length > 0)
            {
                string extension = Path.GetExtension(uploadedFile.FileName).ToLower();
                if (!AllowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("", "Разрешены только файлы .txt, .docx, .pdf");
                    return View(textFile);
                }

                using (var stream = uploadedFile.OpenReadStream())
                {
                    textFile.Content = FileTextExtractor.ExtractText(stream, extension);
                }
                textFile.FileName = uploadedFile.FileName;
            }

            textFile.UploadedAt = DateTime.Now;
            _context.Update(textFile);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: TextFiles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var textFile = await _context.TextFiles.FirstOrDefaultAsync(m => m.Id == id);
            if (textFile == null) return NotFound();
            return View(textFile);
        }

        // POST: TextFiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var textFile = await _context.TextFiles.FindAsync(id);
            if (textFile != null) _context.TextFiles.Remove(textFile);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}