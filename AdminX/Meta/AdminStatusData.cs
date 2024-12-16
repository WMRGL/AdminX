using AdminX.Data;
using AdminX.Models;

namespace AdminX.Meta
{
    public interface IAdminStatusData
    {
        public List<ListStatusAdmin> GetStatusAdmin();
    }

    public class AdminStatusData : IAdminStatusData
    {
        private readonly AdminContext _adminContext;

        public AdminStatusData(AdminContext context)
        {
            _adminContext = context;
        }

       

        public List<ListStatusAdmin> GetStatusAdmin()
        {
            List<ListStatusAdmin> statusAdmins = _adminContext.ListStatusAdmin.ToList();
            return statusAdmins;
        }
    }
}
