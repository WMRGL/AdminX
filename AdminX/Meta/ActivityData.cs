using AdminX.Data;
using AdminX.Models;

namespace AdminX.Meta
{
    interface IActivityData
    {
        public ActivityItem GetActivityDetails(int id);
        public List<ActivityItem> GetClinicDetailsList(int refID);
        public List<ActivityItem> GetActivityList(int mpi);
        public List<ActivityItem> GetActiveReferralList(int mpi);
    }
    public class ActivityData : IActivityData
    {
        private readonly ClinicalContext _clinContext;

        public ActivityData(ClinicalContext context)
        {
            _clinContext = context;
        }
        
        public ActivityItem GetActivityDetails(int id) //Get details of any activity item by RefID
        {
            ActivityItem referral = _clinContext.ActivityItems?.FirstOrDefault(i => i.RefID == id);
            return referral;
        }

        public List<ActivityItem> GetActivityList(int mpi) //Get all activity for a specific patient
        {
            IQueryable<ActivityItem> cl = from c in _clinContext.ActivityItems
                                          where c.MPI == mpi
                                          orderby c.DATE_SCHEDULED
                                          select c;
            
            return cl.ToList();
        }

        public List<ActivityItem> GetActiveReferralList(int mpi) //Get all active referrals for a specific patient
        {
            IQueryable<ActivityItem> cl = from c in _clinContext.ActivityItems
                                          where c.MPI == mpi && c.TYPE.Contains("Ref") && c.COMPLETE == "Active"
                                          orderby c.DATE_SCHEDULED
                                          select c;

            return cl.ToList();
        }

        public List<ActivityItem> GetClinicDetailsList(int refID) //Get details of an appointment by the RefID for editing
        {
            IQueryable<ActivityItem> cl = from c in _clinContext.ActivityItems
                     where c.RefID == refID
                     select c;

            List<ActivityItem> clinics = new List<ActivityItem>();

            foreach (var c in cl)
            {
                //Because the model can't handle spaces, we have to strip them out... but it bombs out if the value is null, so we have to
                //use this incredibly convoluted method to pass it to the HTML!!!
                string letterReq = "";
                string counseled = "";

                if (c.COUNSELED != null)
                {
                    counseled = c.COUNSELED.Replace(" ", "_");
                }
                else
                {
                    counseled = c.COUNSELED;
                }

                if (c.LetterReq != null)
                {
                    letterReq = c.LetterReq.Replace(" ", "_");
                }
                else
                {
                    letterReq = c.LetterReq;
                }

                clinics.Add(new ActivityItem()
                {
                    RefID = c.RefID,
                    BOOKED_DATE = c.BOOKED_DATE,
                    BOOKED_TIME = c.BOOKED_TIME,
                    TYPE = c.TYPE,
                    STAFF_CODE_1 = c.STAFF_CODE_1,
                    FACILITY = c.FACILITY,
                    COUNSELED = counseled,
                    SEEN_BY = c.SEEN_BY,
                    NOPATIENTS_SEEN = c.NOPATIENTS_SEEN,
                    LetterReq = letterReq,
                    ARRIVAL_TIME = c.ARRIVAL_TIME,
                    ClockStop = c.ClockStop
                });
            }

            return clinics;
        }
    }
}
