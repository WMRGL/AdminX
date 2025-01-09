using ClinicalXPDataConnections.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ClinicalXPDataConnections.Meta;
using AdminX.Meta;
using AdminX.ViewModels;
using AdminX.Models;

namespace AdminX.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly ReviewVM _rvm;
        private readonly IConfiguration _config;
        private readonly IActivityData _activityData;        
        private readonly IPatientData _patientData;
        private readonly IStaffUserData _staffUser;
        private readonly IReviewData _reviewData;
        private readonly ICRUD _crud;
        private readonly IAuditService _audit;
        private readonly IReferralData _referralData;

        public ReviewController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            _rvm = new ReviewVM();
            _patientData = new PatientData(_clinContext);
            _activityData = new ActivityData(_clinContext);
            _staffUser = new StaffUserData(_clinContext);
            _reviewData = new ReviewData(_clinContext);
            _crud = new CRUD(_config);
            _audit = new AuditService(_config);
            _referralData = new ReferralData(_clinContext);
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {
                if (User.Identity.Name is null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }

                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;

                IPAddressFinder _ip = new IPAddressFinder(HttpContext);               
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Reviews","", _ip.GetIPAddress());

                _rvm.reviewList = _reviewData.GetReviewsList(User.Identity.Name);

                return View(_rvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Review" });
            }
        }

        public async Task<IActionResult> ReviewsForPatient(int id)
        {
            try
            {
                if (User.Identity.Name is null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }

                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Reviews", "MPI=" + id.ToString(), _ip.GetIPAddress());

                _rvm.reviewList = _reviewData.GetReviewsListForPatient(id);
                _rvm.patient = _patientData.GetPatientDetails(id);

                return View(_rvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Review" });
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult AddReview(int mpi, int refID)
        {
            _rvm.staffMember = _staffUser.GetStaffMemberDetails(User.Identity.Name);
            string staffCode = _rvm.staffMember.STAFF_CODE;
            IPAddressFinder _ip = new IPAddressFinder(HttpContext);
            _audit.CreateUsageAuditEntry(staffCode, "AdminX - Add Review", "RefID=" + refID.ToString(), _ip.GetIPAddress());

            _rvm.referral = _referralData.GetReferralDetails(refID);
            _rvm.reviewList = _reviewData.GetReviewsListForPatient(mpi);
            string login = User.Identity?.Name ?? "Unknown";
            _rvm.staffMember = _staffUser.GetStaffMemberDetails(login);
            _rvm.activity = _activityData.GetActivityDetails(refID);
            _rvm.activityList = _activityData.GetActivityList(mpi).Where(c => c.REFERRAL_DATE != null).ToList();
            _rvm.patient = _patientData.GetPatientDetails(_rvm.referral.MPI);

            if (_rvm.patient != null && _rvm.patient.DOB != null)
            {
                DateTime today = DateTime.Today;
                int age = today.Year - _rvm.patient.DOB.Value.Year;
                if (_rvm.patient.DOB.Value.Date > today.AddYears(-age))
                {
                    age--;
                }
                ViewBag.IsUnder15 = (age < 45);
            }
            else
            {
                ViewBag.IsUnder15 = false;
            }


            ViewBag.Breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Text = "Home", Controller = "Home", Action = "Index" },
                new BreadcrumbItem
                {
                    Text = "Referrals",
                    Controller = "Referral",
                    Action = "ReferralDetails",
                    RouteValues = new Dictionary<string, string>
                    {
                        { "refID", refID.ToString() }
                    }
                },
                new BreadcrumbItem
                {
                    Text = "Review",
                    Controller = "Referral",
                    Action = "Review",
                    RouteValues = new Dictionary<string, string>
                    {
                        { "refID", refID.ToString() },
                         { "mpi", mpi.ToString() }

                    }
                },
                new BreadcrumbItem { Text = "Add" }
            };


            return View(_rvm);
        }

        [HttpPost]
        public IActionResult AddReview(int mpi, int refID, string Owner, string Category, string Pathway, int Parent_RefID,
            string Review_Recipient, DateTime? Completed_Date, string Completed_By, DateTime? Planned_Date, string Review_Status, string Comments
            )
        {

            string login = User.Identity?.Name ?? "Unknown";

            int success = _crud.PatientReview(
                     sType: "Review",
                     sOperation: "Create",
                     int1: mpi,
                     string1: Pathway,
                     string2: Owner,
                     string3: Review_Recipient,
                     string4: Category,
                     string5: Completed_By,
                     string7: Review_Status,
                     string8: Comments,
                     dDate1: Planned_Date,
                     dDate2: Completed_Date,
                     int2: Parent_RefID

                 );
            if (success != 1)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Referral-edit(SQL)" });
            }


            return RedirectToAction("Review", new { refID = refID, mpi = mpi });
        }

        [HttpGet]
        public IActionResult GetLinkedAppointment(int refID)
        {
            var activityDetails = _activityData.GetActivityDetails(refID);
            string linkedAppointment = activityDetails.RefID.ToString();
            string referral = activityDetails.TYPE;
            string pathway = activityDetails.PATHWAY;

            return Json(new { linkedAppointment, referral, pathway });
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit (int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Edit Review", "ID=" + id.ToString(), _ip.GetIPAddress());

                _rvm.review = _reviewData.GetReviewDetails(id);
                _rvm.patient = _patientData.GetPatientDetails(_rvm.review.MPI);

                if (_rvm.review == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }

                return View(_rvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Review-edit" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string status, string comments, string revDate)
        {
            try
            {
                _rvm.review = _reviewData.GetReviewDetails(id);

                DateTime reviewDate = new DateTime();

                if (revDate != null)
                {
                    reviewDate = DateTime.Parse(revDate);
                }
                else
                {
                    reviewDate = DateTime.Parse("1/1/1900");
                }
                               
                int success = _crud.CallStoredProcedure("Review", "Edit", id, 0, 0, status, comments, "", "", User.Identity.Name,
                    reviewDate);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Review-edit(SQL)" }); }

                return RedirectToAction("Index", "Review");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Review-edit" });
            }
        }
    }
}
