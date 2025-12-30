using AdminX.Data;
using AdminX.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminX.Meta
{
    public interface IHSDataAsync
    {
        public Task<List<HS>> GetHSList(string pedno);
    }
    public class HSDataAsync : IHSDataAsync
    {
        private readonly AdminContext _context;

        public HSDataAsync(AdminContext context)
        {
            _context = context;
        }

        public async Task<List<HS>> GetHSList(string pedno)
        {
            IQueryable<HS> hsList = _context.HS.Where(h => h.MasterFileNo == pedno);

            return await hsList.ToListAsync();
        }
    }
}
