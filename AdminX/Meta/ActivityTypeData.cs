using AdminX.Data;
using AdminX.Models;
using ClinicalXPDataConnections.Data;
using System.Data;

namespace AdminX.Meta
{
    interface IActivityTypeData
    {
        public List<ActivityType> GetReferralTypes();
        public List<ActivityType> GetApptTypes();
    }
    public class ActivityTypeData : IActivityTypeData
    {
        private readonly ClinicalContext _clinContext;
        private readonly AdminContext _adminContext;

        public ActivityTypeData(ClinicalContext context, AdminContext adminContext)
        {
            _clinContext = context;
            _adminContext = adminContext;
        }

        public List<ActivityType> GetReferralTypes()
        {
            IQueryable<ActivityType> apptypes = _adminContext.ActivityType.Where(t => (t.NON_ACTIVE == 0 && t.ISREFERRAL == true) || t.APP_TYPE.Contains("Temp"));
            return apptypes.ToList();
        }
        public List<ActivityType> GetApptTypes()
        {
            IQueryable<ActivityType> apptypes = _adminContext.ActivityType.Where(t => t.NON_ACTIVE == 0 && t.ISAPPT == true);
            return apptypes.ToList();
        }
    }
}