using AdminX.Data;
using AdminX.Models;
using System.Data;

namespace AdminX.Meta
{
    interface IReviewData
    {
        public List<Review> GetReviewsList(string username);
        public List<Review> GetReviewsListForPatient(int mpi);
        public Review GetReviewDetails(int id);
    }
    public class ReviewData : IReviewData
    {
        private readonly ClinicalContext _clinContext;
        private readonly StaffUserData _staffUser;

        public ReviewData(ClinicalContext context)
        {
            _clinContext = context;
            _staffUser = new StaffUserData(_clinContext);
        }
       

        public List<Review> GetReviewsList(string username) 
        {
            string staffCode = _staffUser.GetStaffMemberDetails(username).STAFF_CODE;

            IQueryable<Review> reviews = from r in _clinContext.Reviews
                          where r.Review_Recipient == staffCode && r.Review_Status == "Pending"
                          orderby r.Planned_Date
                          select r;

            return reviews.ToList();
        }

        public List<Review> GetReviewsListForPatient(int mpi)
       {
            IQueryable<Review> reviews = from r in _clinContext.Reviews
                                         where r.MPI == mpi && r.Review_Status == "Pending"
                                         orderby r.Planned_Date
                                         select r;

            return reviews.ToList();
        }

        public Review GetReviewDetails(int id)
        {
            Review review = _clinContext.Reviews.FirstOrDefault(r => r.ReviewID == id);

            return review;
        }


        
    }
}
