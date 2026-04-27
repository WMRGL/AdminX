using AdminX.Data;
using AdminX.Models;
using System.Data;

namespace AdminX.Meta
{
    interface IAlertTypeData
    {
        public List<AlertTypes> GetAlertTypes();
    }
    public class AlertTypeData : IAlertTypeData
    {
        private readonly AdminContext _adminContext;

        public AlertTypeData(AdminContext adminContext)
        {
            _adminContext = adminContext;
        }

        public List<AlertTypes> GetAlertTypes()
        {
            IQueryable<AlertTypes> alertTypes = _adminContext.AlertTypes.Where(t => t.InUse == true);
            return alertTypes.ToList();
        }
        
    }
}