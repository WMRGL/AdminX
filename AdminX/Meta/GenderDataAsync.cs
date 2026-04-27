using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminX.Meta
{
    public interface IGenderDataAsync
    {
        public Task<List<Gender>> GetGenderList();
    }
    public class GenderDataAsync : IGenderDataAsync
    {                
        private readonly ClinicalContext _context;

        public GenderDataAsync(ClinicalContext context)
        {
            _context = context;
        }

        public async Task<List<Gender>> GetGenderList()
        {
            IQueryable<Gender> genders = _context.Genders;

            return await genders.ToListAsync();
        }
    }
}
