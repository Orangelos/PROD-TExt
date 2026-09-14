using Diplom.Models;
using Diplom.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Diplom.Controllers
{
    public class AnalysisController : Controller
    {
        private readonly DiplomDbContext _context;

        public AnalysisController(DiplomDbContext context)
        {
            _context = context;
        }

        // GET: Analysis/StructuralAnalysis/5
        public async Task<IActionResult> StructuralAnalysis(int? id)
        {
            if (id == null)
                return NotFound();

            var textFile = await _context.TextFiles.FindAsync(id);
            if (textFile == null)
                return NotFound();

            var analysisResult = TextAnalysisService.Analyze(
                textFile.Content,
                textFile.FileName,
                textFile.UploadedAt
            );

            return View(analysisResult);
        }
    }
}