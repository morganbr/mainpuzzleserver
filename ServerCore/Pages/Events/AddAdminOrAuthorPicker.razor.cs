using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using ServerCore.Pages.Components;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using ServerCore.DataModel;
using System.Collections.Generic;
using System;

namespace ServerCore.Pages.Events
{
    public partial class AddAdminOrAuthorPicker
    {
        [Parameter]
        public bool AddAdmin { get; set; }

        List<PuzzleUser> CurrentAdminOrAuthors { get; set; } = new List<PuzzleUser>();

        protected override async Task OnParametersSetAsync()
        {
            await UpdateCurrentAdminOrAuthorsAsync();

            await base.OnParametersSetAsync();
        }

        private async Task UpdateCurrentAdminOrAuthorsAsync()
        {
            await _contextLock.WaitAsync();
            try
            {
                if (AddAdmin)
                {
                    CurrentAdminOrAuthors = await (from admin in _context.EventAdmins
                                                   where admin.EventID == EventId
                                                   select admin.Admin).ToListAsync();
                }
                else
                {
                    CurrentAdminOrAuthors = await (from author in _context.EventAuthors
                                                   where author.EventID == EventId
                                                   select author.Author).ToListAsync();
                }
            }
            finally
            {
                _contextLock.Release();
            }
        }

        protected override async Task<List<PuzzleUser>> GetAllUsersAsync()
        {
            await _contextLock.WaitAsync();

            try
            {
                if (AddAdmin)
                {
                    return await (from user in _context.PuzzleUsers
                                  where !((from eventAdmin in _context.EventAdmins
                                           where eventAdmin.EventID == EventId
                                           where eventAdmin.Admin == user
                                           select eventAdmin).Any())
                                  select user).ToListAsync();
                }
                else
                {
                    return await (from user in _context.PuzzleUsers
                                  where !((from eventAuthor in _context.EventAuthors
                                           where eventAuthor.EventID == EventId
                                           where eventAuthor.Author == user
                                           select eventAuthor).Any())
                                  select user).ToListAsync();
                }
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
                PuzzleUser user = await _context.PuzzleUsers.FirstOrDefaultAsync(m => m.ID == addedUserId);
                if (user == null)
                {
                    throw new Exception("Could not find user with ID '" + addedUserId + "'. Check to make sure the user hasn't been removed.");
                }

                if (AddAdmin)
                {
                    EventAdmins admin = new EventAdmins();
                    admin.AdminID = addedUserId;
                    admin.EventID = EventId;

                    _context.EventAdmins.Add(admin);
                }
                else
                {
                    EventAuthors author = new EventAuthors();
                    author.AuthorID = addedUserId;
                    author.EventID = EventId;

                    _context.EventAuthors.Add(author);
                }

                user.MayBeAdminOrAuthor = true;

                await _context.SaveChangesAsync();
            }
            finally
            {
                _contextLock.Release();
            }

            await UpdateCurrentAdminOrAuthorsAsync();
        }
    }
}
