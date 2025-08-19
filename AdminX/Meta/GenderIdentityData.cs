using AdminX.Data;
using AdminX.Models;

namespace AdminX.Meta
{
    public interface IGenderIdentityData
    {
        public List<GenderIdentity> GetGenderIdentities();

    }

    public class GenderIdentityData : IGenderIdentityData
    {
        private readonly AdminContext _adminXContext;
        public GenderIdentityData(AdminContext context)
        {
            _adminXContext = context;
        }
        public List<GenderIdentity> GetGenderIdentities()
        {
            IQueryable<GenderIdentity> genderIdentities = _adminXContext.GenderIdentity;
            return genderIdentities.ToList();
        }
    }
}
