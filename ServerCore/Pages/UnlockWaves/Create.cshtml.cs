using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServerCore.DataModel;
using ServerCore.ModelBases;
using Microsoft.EntityFrameworkCore;

namespace ServerCore.Pages.UnlockWaves
{
    public class CreateModel : EventSpecificPageModel
    {
        private readonly PuzzleServerContext _context;

        public CreateModel(PuzzleServerContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGet()
        {
            //UnlockWave = new UnlockWave()
            //{
            //    Event = Event,
            //};
            OtherWaves = await (from wave in _context.UnlockWaves
                               where wave.Event == Event
                               select wave).ToListAsync();
            return Page();
        }

        [BindProperty]
        public UnlockWave UnlockWave { get; set; }

        public IList<UnlockWave> OtherWaves { get; set; }

        [BindProperty]
        public UnlockWave PreviousWave { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            // The event gets populated after the user is done with it, so don't validate
            ModelState.Remove("UnlockWave.Event");
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // This was round-tripped through the page to make validation happy,
            // but we can just blindly overwrite it with the current event
            UnlockWave.Event = Event;

            _context.UnlockWaves.Add(UnlockWave);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}