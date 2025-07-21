using ClinicalXPDataConnections.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ClinicalXPDataConnections.Meta;
using AdminX.Meta;
using AdminX.ViewModels;
using AdminX.Models;
using ClinicalXPDataConnections.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

                _rvm.reviewList = _reviewData.GetReviewsListAll();
                ViewBag.Breadcrumbs = new List<BreadcrumbItem>
                {
                    new BreadcrumbItem { Text = "Home", Controller = "Home", Action = "Index" },

                    new BreadcrumbItem { Text = "Review" }
                };

                return View(_rvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Review" });
            }
        }

        [HttpPost]
        public IActionResult GetReviews()
        {
            try
            {
                // DataTables parameters
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
                var sortColumnName = Request.Form["columns[" + sortColumnIndex + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                List<Review> reviews = _reviewData.GetReviewsListAll();

				if (!string.IsNullOrEmpty(searchValue))
				{
					searchValue = searchValue.ToLower();
					reviews = reviews.Where(r =>
						(r.CGU_No !=null && r.CGU_No.ToLower().Contains(searchValue)) ||
						(r.FIRSTNAME != null && r.FIRSTNAME.ToLower().Contains(searchValue)) ||
						(r.LASTNAME != null && r.LASTNAME.ToLower().Contains(searchValue)) ||
						(r.Owner != null && r.Owner.ToLower().Contains(searchValue)) ||
						(r.Planned_Date.HasValue && r.Planned_Date.Value.ToString("dd/MM/yyyy").ToLower().Contains(searchValue))
					
					).ToList(); 
				}

				if (!string.IsNullOrEmpty(sortColumnName) && !string.IsNullOrEmpty(sortColumnDirection))
                {
                    Func<Review, object> orderingFunction = (r) =>
                    {
                        switch (sortColumnName)
                        {
                            case "CGU_No": return r.CGU_No;
                            case "Patient": return r.FIRSTNAME; 
                            case "MPI": return r.MPI;
                            case "Planned_Date": return r.Planned_Date;
                            case "Owner": return r.Owner;
                            default: return r.Planned_Date; 
                        }
                    };

                    if (sortColumnDirection == "asc")
                    {
                        reviews = reviews.OrderBy(orderingFunction).ToList();
                    }
                    else
                    {
                        reviews = reviews.OrderByDescending(orderingFunction).ToList();
                    }
                }
                int recordsTotal = reviews.Count();

                var data = reviews.Skip(skip).Take(pageSize).ToList();

                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Create(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Create Review", "ID=" + id.ToString());

                _rvm.activity = _activityData.GetActivityDetails(id);
                _rvm.staffMembers = _staffUser.GetClinicalStaffList();
                _rvm.patient = _patientData.GetPatientDetails(_rvm.activity.MPI);
                _rvm.activityList = _activityData.GetActivityList(_rvm.patient.MPI).Where(c => c.REFERRAL_DATE != null).ToList();
                _rvm.staffMember = _staffUser.GetStaffMemberDetails(User.Identity.Name);

                if (_rvm.patient != null && _rvm.patient.DOB != null)
                {
                    DateTime today = DateTime.Today;
                    int age = today.Year - _rvm.patient.DOB.Value.Year;
                    if (_rvm.patient.DOB.Value.Date > today.AddYears(-age))
                    {
                        age--;
                    }
                    ViewBag.IsUnder15 = (age < 15);
                    ViewBag.DateOfFifteen = _rvm.patient.DOB.Value.AddYears(15).ToString("yyyy-MM-dd");

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
                    Text = "Review",
                    Controller = "Review",
                    Action = "Index",
                    
                },
                new BreadcrumbItem { Text = "Add" }
            };


                return View(_rvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Review-add" });
            }
        }

        [Authorize]
        [HttpPost]
        public IActionResult Create(int mpi, int refID,  string Owner, string Category, string Pathway, int Parent_RefID,
        string Review_Recipient, string Completed_By, DateTime? Planned_Date, string Review_Status, string Comments
        )
        {

            string login = User.Identity?.Name ?? "Unknown";

            int success = _crud.PatientReview(
                     sType: "Review",
                     sOperation: "Create",
                     int1: mpi,
                     sLogin: User.Identity.Name,
                     string1: Pathway,
                     string2: Owner,
                     string3: Review_Recipient,
                     string4: Category,
                     string7: Review_Status,
                     string8: Comments,
                     dDate1: Planned_Date,
                     int2: Parent_RefID

                 );
            if (success != 1)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Referral-edit(SQL)" });
            }


            return RedirectToAction("Index");
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
                _rvm.staffMembers = _staffUser.GetClinicalStaffList();
                _rvm.staffMember = _staffUser.GetStaffMemberDetails(User.Identity.Name);

                _rvm.activityDetail = _activityData.GetActivityDetails((int)_rvm.review.Parent_RefID);

                if (_rvm.activityDetail is null)
                {
                    TempData["ErrorMessage"] = "Patient does not have Referral ID";
                    return RedirectToAction("Index", "Review");
                }

                if (_rvm.patient is null)
                {
                    TempData["ErrorMessage"] = "Patient does not have Patient ID";
                    return RedirectToAction("Index", "Review");
                }
                ViewBag.Breadcrumbs = new List<BreadcrumbItem>
                {
                    new BreadcrumbItem { Text = "Home", Controller = "Home", Action = "Index" },
                    new BreadcrumbItem
                    {
                        Text = "Review",
                        Controller = "Review",
                        Action = "Index",

                    },
                new BreadcrumbItem { Text = "Update" }
                };


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
        public async Task<IActionResult> Edit(int id, string Review_Status, string Comments, string revDate, DateTime? Planned_Date)
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
                               
                int success = _crud.CallStoredProcedure("Review", "Update", id, 0, 0, string1: Review_Status, string2: Comments, "", "", User.Identity.Name,
                    reviewDate, dDate2: Planned_Date);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Review-edit(SQL)" }); }

                TempData["SuccessMessage"] = "Patient updated successfully";

                return RedirectToAction("Index", "Review");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Review-edit" });
            }
        }

        [HttpGet]
        public IActionResult Review(int mpi, int refID)
        {

            _rvm.reviewList = _reviewData.GetReviewsListForPatient(mpi);
            _rvm.referral = _referralData.GetReferralDetails(refID);

            ViewBag.Breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Text = "Home", Controller = "Home", Action = "Index" },
                
                new BreadcrumbItem { Text = "Review" }
            };

            return View(_rvm);
        }

        [HttpGet]
        public IActionResult AddReview(int mpi, int refID)
        {
            _rvm.patient = _patientData.GetPatientDetails(mpi);
            _rvm.activityList = _activityData.GetActivityList(mpi);
            _rvm.staffMember = _staffUser.GetStaffMemberDetails(User.Identity.Name);
            _rvm.staffMembers = _staffUser.GetStaffMemberList();

            if(refID != 0)
            {
                _rvm.activity = _activityData.GetActivityDetails(refID);

                ViewBag.Breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Text = "Home", Controller = "Home", Action = "Index" },
                new BreadcrumbItem
                {
                    Text = "Review",
                    Controller = "Review",
                    Action = "Index",

                },
                new BreadcrumbItem { Text = "Add" }
            };

                if (_rvm.patient != null && _rvm.patient.DOB != null)
                {
                    DateTime today = DateTime.Today;
                    int age = today.Year - _rvm.patient.DOB.Value.Year;
                    if (_rvm.patient.DOB.Value.Date > today.AddYears(-age))
                    {
                        age--;
                    }
                    ViewBag.IsUnder15 = (age < 15);
                    ViewBag.DateOfFifteen = _rvm.patient.DOB.Value.AddYears(15).ToString("yyyy-MM-dd");

                }
                else
                {
                    ViewBag.IsUnder15 = false;
                }
            }

            return View(_rvm);
        }

        [HttpPost]
        public IActionResult AddReview(int mpi, int refID, string Owner, string Category, string Pathway, string Review_Recipient, DateTime? Planned_Date, string Comments)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Create Review", "RefID=" + refID.ToString());

                _rvm.activity = _activityData.GetActivityDetails(refID);
                _rvm.staffMembers = _staffUser.GetClinicalStaffList();
                _rvm.patient = _patientData.GetPatientDetails(_rvm.activity.MPI);
                _rvm.activityList = _activityData.GetActivityList(_rvm.patient.MPI).Where(c => c.REFERRAL_DATE != null).ToList();
                _rvm.staffMember = _staffUser.GetStaffMemberDetails(User.Identity.Name);

                

                _crud.PatientReview("Review", "Create", User.Identity.Name, mpi, Pathway, Owner, Review_Recipient, Category, "", "Pending", Comments, Planned_Date, refID);

                return View(_rvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Review-add" });
            }
        }
    }
}
