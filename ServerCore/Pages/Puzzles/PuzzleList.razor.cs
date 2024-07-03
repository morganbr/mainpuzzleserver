using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Puzzles
{
    public partial class PuzzleList
    {
        [Parameter]
        public int PuzzleUserId { get; set; }

        [Parameter]
        public int TeamId { get; set; }

        [Parameter]
        public int EventId { get; set; }

        [Parameter]
        public EventRole EventRole { get; set; }

        [Inject]
        public PuzzleServerContext _context { get; set; }

        Event Event { get; set; }

        Team Team { get; set; }

        IEnumerable<PuzzleView> VisibleTeamPuzzleViews { get; set; } = Enumerable.Empty<PuzzleView>();

        bool Loaded = false;

        public class PuzzleView
        {
            public int ID { get; set; }
            public string Group { get; set; }
            public int OrderInGroup { get; set; }
            public string Name { get; set; }
            public string Errata { get; set; }
            //public string PuzzleUrl { get; set; }
            //public string SolutionUrl { get; set; }
            public DateTime? SolvedTime { get; set; }
            public PieceMetaUsage PieceMetaUsage { get; set; }
            //public string CustomUrl { get; set; }
            //public string CustomSolutionUrl { get; set; }
            public string ViewUrl { get; set; }
        }

        public enum SortOrder
        {
            PuzzleAscending,
            PuzzleDescending,
            GroupAscending,
            GroupDescending,
            SolveAscending,
            SolveDescending
        }

        public enum PuzzleStateFilter
        {
            All,
            Unsolved
        }

        SortOrder _sortOrder { get; set; }

        const SortOrder DefaultSort = SortOrder.GroupAscending;

        PuzzleStateFilter _stateFilter { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            Event = await _context.Events.Where(ev => ev.ID == EventId).SingleAsync();
            Team = await _context.Teams.Where(t => t.ID == TeamId).SingleAsync();

            VisibleTeamPuzzleViews = await GetVisibleTeamPlayerPuzzleViews(_sortOrder);

            Loaded = true;
            await base.OnParametersSetAsync();
        }

        private async Task<IEnumerable<PuzzleView>> GetVisibleTeamPlayerPuzzleViews(SortOrder? sortOrder)
        {
            // If the event has not yet begun, no puzzles yet
            if (DateTime.UtcNow < Event.EventBegin && !Team.IsDisqualified)
            {
                return Enumerable.Empty<PuzzleView>();
            }

            // all puzzles for this event that are real puzzles
            var puzzlesInEventQ = _context.Puzzles.Where(puzzle => puzzle.Event.ID == Event.ID && puzzle.IsPuzzle && !puzzle.IsForSinglePlayer);

            // unless we're in a global lockout, then filter to those!
            var puzzlesCausingGlobalLockoutQ = PuzzleStateHelper.PuzzlesCausingGlobalLockout(_context, Event, Team);
            if (await puzzlesCausingGlobalLockoutQ.AnyAsync())
            {
                puzzlesInEventQ = puzzlesCausingGlobalLockoutQ;
            }

            // all puzzle states for this team that are unlocked (note: IsUnlocked bool is going to harm perf, just null check the time here)
            // Note that it's OK if some puzzles do not yet have a state record; those puzzles are clearly still locked and hence invisible.
            // All puzzles will show if all answers have been released)
            var stateForTeamQ = _context.PuzzleStatePerTeam.Where(state => state.TeamID == Team.ID && (Event.AreAnswersAvailableNow || state.UnlockedTime != null));

            // join 'em (note: just getting all properties for max flexibility, can pick and choose columns for perf later)
            // Note: EF gotcha is that you have to join into anonymous types in order to not lose valuable stuff
            var visiblePuzzlesQ = from Puzzle puzzle in puzzlesInEventQ
                                  join PuzzleStatePerTeam pspt in stateForTeamQ on puzzle.ID equals pspt.PuzzleID

                                  select new PuzzleView { ID = puzzle.ID, Group = puzzle.Group, OrderInGroup = puzzle.OrderInGroup, Name = puzzle.Name,/* CustomUrl = puzzle.CustomURL, CustomSolutionUrl = puzzle.CustomSolutionURL,*/ Errata = puzzle.Errata, SolvedTime = pspt.SolvedTime, PieceMetaUsage = puzzle.PieceMetaUsage,
                                  ViewUrl = $"/{Event.EventID}/{EventRole}/Submissions/{puzzle.ID}"};

            // todo morganb: sort and filter should be separate from the db query
            if (_stateFilter == PuzzleStateFilter.Unsolved)
            {
                visiblePuzzlesQ = visiblePuzzlesQ.Where(puzzles => puzzles.SolvedTime == null);
            }

            return GetSortedView(visiblePuzzlesQ, sortOrder);
        }

        private IEnumerable<PuzzleView> GetSortedView(IEnumerable<PuzzleView> puzzleViews, SortOrder? sortOrder)
        {
            SortOrder actualSortOrder = sortOrder.HasValue ? sortOrder.Value : DefaultSort;

            switch (actualSortOrder)
            {
                case SortOrder.PuzzleAscending:
                    return puzzleViews.OrderBy(pv => pv.Name);
                case SortOrder.PuzzleDescending:
                    return puzzleViews.OrderByDescending(pv => pv.Name);
                case SortOrder.GroupAscending:
                    return puzzleViews.OrderBy(pv => pv.Group).ThenBy(pv => pv.OrderInGroup).ThenBy(pv => pv.Name);
                case SortOrder.GroupDescending:
                    return puzzleViews.OrderByDescending(pv => pv.Group).ThenByDescending(pv => pv.OrderInGroup).ThenByDescending(pv => pv.Name);
                case SortOrder.SolveAscending:
                    return puzzleViews.OrderBy(pv => pv.SolvedTime ?? DateTime.MaxValue);
                case SortOrder.SolveDescending:
                    return puzzleViews.OrderByDescending(pv => pv.SolvedTime ?? DateTime.MaxValue);
                default:
                    throw new ArgumentException($"unknown sort: {sortOrder}");
            }
        }
    }
}
