using AdminX.Data;
using AdminX.Models;

namespace AdminX.Meta
{
    public interface IDiaryActionData
    {
        public DiaryAction GetDiaryActionDetails(string actionCode);
        public List<DiaryAction> GetDiaryActions();        
    }
    public class DiaryActionData : IDiaryActionData
    {
        private readonly AdminContext _adminContext;

        public DiaryActionData(AdminContext adminContext)
        {
            _adminContext = adminContext;
        }

        public DiaryAction GetDiaryActionDetails(string actionCode)
        {
            DiaryAction action = _adminContext.DiaryAction.First(a => a.ActionCode == actionCode);

            return action;
        }

        public List<DiaryAction> GetDiaryActions()
        {
            IQueryable<DiaryAction> actions = _adminContext.DiaryAction;
            return actions.ToList();
        }
    }
}