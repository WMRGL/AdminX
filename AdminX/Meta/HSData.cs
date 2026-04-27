using AdminX.Data;
using AdminX.Models;

namespace AdminX.Meta
{
    public interface IHSData
    {
        public List<HS> GetHSList(string pedno);
    }
    public class HSData : IHSData
    {
        private readonly AdminContext _context;

        public HSData(AdminContext context)
        {
            _context = context;
        }

        public List<HS> GetHSList(string pedno)
        {
            IQueryable<HS> hsList = _context.HS.Where(h => h.MasterFileNo == pedno);

            return hsList.ToList();
        }
    }
}
