using AdminX.Data;
using AdminX.Models;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace AdminX.Meta
{
    public interface IAlertTypeDataAsync
    {
        public Task<List<AlertTypes>> GetAlertTypes();
    }
    public class AlertTypeDataAsync : IAlertTypeDataAsync
    {
        private readonly AdminContext _adminContext;

        public AlertTypeDataAsync(AdminContext adminContext)
        {
            _adminContext = adminContext;
        }

        public async Task<List<AlertTypes>> GetAlertTypes()
        {
            IQueryable<AlertTypes> alertTypes = _adminContext.AlertTypes.Where(t => t.InUse == true);
            
            return await alertTypes.ToListAsync();
        }
        
    }
}