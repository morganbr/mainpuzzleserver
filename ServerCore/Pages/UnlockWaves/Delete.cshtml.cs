using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;

namespace ServerCore.Pages.UnlockWaves
{
    public class DeleteModel : PageModel
    {
        private readonly ServerCore.DataModel.PuzzleServerContext _context;

        public DeleteModel(ServerCore.DataModel.PuzzleServerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public UnlockWave UnlockWave { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            UnlockWave = await _context.UnlockWaves.FirstOrDefaultAsync(m => m.Id == id);

            if (UnlockWave == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            UnlockWave = await _context.UnlockWaves.FindAsync(id);

            if (UnlockWave != null)
            {
                _context.UnlockWaves.Remove(UnlockWave);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
