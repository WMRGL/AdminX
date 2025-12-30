using AdminX.Data;
using AdminX.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminX.Meta
{
    public interface IDiaryActionDataAsync
    {
        public Task<DiaryAction> GetDiaryActionDetails(string actionCode);
        public Task<List<DiaryAction>> GetDiaryActions();        
    }
    public class DiaryActionDataAsync : IDiaryActionDataAsync
    {
        private readonly AdminContext _adminContext;

        public DiaryActionDataAsync(AdminContext adminContext)
        {
            _adminContext = adminContext;
        }

        public async Task<DiaryAction> GetDiaryActionDetails(string actionCode)
        {
            DiaryAction action = await _adminContext.DiaryAction.FirstAsync(a => a.ActionCode == actionCode);

            return action;
        }

        public async Task<List<DiaryAction>> GetDiaryActions()
        {
            IQueryable<DiaryAction> actions = _adminContext.DiaryAction;
            
            return await actions.ToListAsync();
        }
    }
}