using AdminX.Data;
using AdminX.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminX.Meta
{
    public interface IGenderIdentityDataAsync
    {
        public Task<List<GenderIdentity>> GetGenderIdentities();
    }

    public class GenderIdentityDataAsync : IGenderIdentityDataAsync
    {
        private readonly AdminContext _adminXContext;
        public GenderIdentityDataAsync(AdminContext context)
        {
            _adminXContext = context;
        }
        public async Task<List<GenderIdentity>> GetGenderIdentities()
        {
            IQueryable<GenderIdentity> genderIdentities = _adminXContext.GenderIdentity;
            return await genderIdentities.ToListAsync();
        }
    }
}
