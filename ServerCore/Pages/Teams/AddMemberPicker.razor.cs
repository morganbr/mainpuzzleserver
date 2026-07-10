using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using ServerCore.DataModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    public partial class AddMemberPicker
    {
        [Parameter]
        public int TeamId { get; set; }

        List<PuzzleUser> CurrentMembers { get; set; } = new List<PuzzleUser>();

        protected override async Task OnParametersSetAsync()
        {
            await UpdateCurrentMembersAsync();

            await base.OnParametersSetAsync();
        }

        protected override async Task<List<PuzzleUser>> GetAllUsersAsync()
        {
            await _contextLock.WaitAsync();
            try
            {
                return await (from user in _context.PuzzleUsers
                              where !((from teamMember in _context.TeamMembers
                                       where teamMember.Team.EventID == EventId
                                       where teamMember.Member == user
                                       select teamMember).Any())
                              select user).ToListAsync();
            }
            finally
            {
                _contextLock.Release();
            }
        }

        private async Task UpdateCurrentMembersAsync()
        {
            await _contextLock.WaitAsync();
            try
            {
                CurrentMembers = await (from teamMember in _context.TeamMembers
                                        where teamMember.TeamID == TeamId
                                        select teamMember.Member).ToListAsync();

            }
            finally
            {
                _contextLock.Release();
            }
        }

        protected override async Task OnUserAddedAsync(int addedUserId)
        {
            await _contextLock.WaitAsync();
            try
            {
                Event ev = await (from evt in _context.Events
                                  where evt.ID == EventId
                                  select evt).SingleAsync();
                var (success, error) = await TeamHelper.AddMemberAsync(_context, ev, EventRole.admin, TeamId, addedUserId);
                if (!success)
                {
                    ErrorText = error;
                    return;
                }
            }
            finally
            {
                _contextLock.Release();
            }

            await UpdateCurrentMembersAsync();
        }
    }
}
