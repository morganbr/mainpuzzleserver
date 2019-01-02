using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;

namespace ServerCore.Pages.UnlockWaves
{
    public class EditModel : PageModel
    {
        private readonly ServerCore.DataModel.PuzzleServerContext _context;

        public EditModel(ServerCore.DataModel.PuzzleServerContext context)
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

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(UnlockWave).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UnlockWaveExists(UnlockWave.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool UnlockWaveExists(int id)
        {
            return _context.UnlockWaves.Any(e => e.Id == id);
        }
    }
}
