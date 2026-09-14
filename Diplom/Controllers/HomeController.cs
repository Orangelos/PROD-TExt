using Microsoft.AspNetCore.Mvc;
using Diplom.Models;          // для DiplomDbContext
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Diplom.Controllers
{
    public class HomeController : Controller
    {
        private readonly DiplomDbContext _context;

        public HomeController(DiplomDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Получаем все файлы, отсортированные по дате загрузки (сначала новые)
            var files = await _context.TextFiles.OrderByDescending(f => f.UploadedAt).ToListAsync();
            return View(files);
        }
    }
}