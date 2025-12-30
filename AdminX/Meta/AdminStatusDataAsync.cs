using AdminX.Data;
using AdminX.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminX.Meta
{
    public interface IAdminStatusDataAsync
    {
        public Task<List<ListStatusAdmin>> GetStatusAdmin();
    }

    public class AdminStatusDataAsync : IAdminStatusDataAsync
    {
        private readonly AdminContext _adminContext;

        public AdminStatusDataAsync(AdminContext context)
        {
            _adminContext = context;
        }

       

        public async Task<List<ListStatusAdmin>> GetStatusAdmin()
        {
            List<ListStatusAdmin> statusAdmins = await _adminContext.ListStatusAdmin.OrderBy(a => a.Sequence).ToListAsync();

            return statusAdmins;
        }
    }
}
