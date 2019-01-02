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
    public class DetailsModel : PageModel
    {
        private readonly ServerCore.DataModel.PuzzleServerContext _context;

        public DetailsModel(ServerCore.DataModel.PuzzleServerContext context)
        {
            _context = context;
        }

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
    }
}
