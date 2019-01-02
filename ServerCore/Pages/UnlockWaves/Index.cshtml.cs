using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.UnlockWaves
{
    public class IndexModel : EventSpecificPageModel
    {
        private readonly ServerCore.DataModel.PuzzleServerContext _context;

        public IndexModel(ServerCore.DataModel.PuzzleServerContext context)
        {
            _context = context;
        }

        public IList<UnlockWave> UnlockWaves { get;set; }

        public async Task OnGetAsync()
        {
            UnlockWaves = await _context.UnlockWaves.Where((wave) => wave.Event != null && wave.Event == Event).ToListAsync();
        }
    }
}
