using AdminX.Data;
using AdminX.Meta;
using AdminX.Models;
using AdminX.ViewModels;
using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

namespace AdminX.Controllers
{
    public class DictatedLetterController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly DocumentContext _docContext;
        private readonly AdminContext _adminContext;
        private readonly LetterController _lc;
        private readonly DictatedLetterVM _lvm;
        private readonly IConfiguration _config;
        private readonly ICRUD _crud;
        private readonly IPatientData _patientData;
        private readonly IStaffUserData _staffUser;
        private readonly IActivityData _activityData;
        private readonly IDictatedLetterData _dictatedLetterData;
        private readonly IExternalClinicianData _externalClinicianData;
        private readonly IExternalFacilityData _externalFacilityData;
        private readonly IDictatedLettersReportData _dotReportData;
        private readonly IAuditService _audit;
        private readonly IConstantsData _constantsData;
        private readonly IPAddressFinder _ip;

        public DictatedLetterController(IConfiguration config, ClinicalContext clinContext, DocumentContext docContext, AdminContext adminContext)
        {
            _clinContext = clinContext;
            _docContext = docContext;
            _adminContext = adminContext;
            _config = config;
            _crud = new CRUD(_config);
            _lvm = new DictatedLetterVM();
            _staffUser = new StaffUserData(_clinContext);
            _patientData = new PatientData(_clinContext);
            _activityData = new ActivityData(_clinContext);
            _dictatedLetterData = new DictatedLetterData(_clinContext);
            _externalClinicianData = new ExternalClinicianData(_clinContext);
            _externalFacilityData = new ExternalFacilityData(_clinContext);
            _dotReportData = new DictatedLettersReportData(_adminContext);
            _lc = new LetterController(_clinContext, _docContext);
            _audit = new AuditService(_config);
            _constantsData = new ConstantsData(_docContext);
            _ip = new IPAddressFinder(HttpContext);
        }

        [Authorize]
        public async Task<IActionResult> Index(string? staffCode)
        {
            try
            {
                if (User.Identity.Name is null)
                {
                    return NotFound();
                }

                var user = _staffUser.GetStaffMemberDetails(User.Identity.Name);

                //IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(user.STAFF_CODE, "AdminX - Letters", "", _ip.GetIPAddress());

                var letters = _dictatedLetterData.GetDictatedLettersListFull();

                if(staffCode != null)
                {
                    letters = letters.Where(l => l.LetterFromCode == staffCode).ToList();
                }
                _lvm.clinicalStaff = _staffUser.GetClinicalStaffList();
                _lvm.dictatedLettersForApproval = letters.Where(l => l.Status != "For Printing").ToList();
                _lvm.dictatedLettersForPrinting = letters.Where(l => l.Status == "For Printing").ToList();

                ViewBag.Breadcrumbs = new List<BreadcrumbItem>
                {
                    new BreadcrumbItem { Text = "Home", Controller = "Home", Action = "Index" },

                    new BreadcrumbItem { Text = "Letters" }
                };

                _lvm.dictatedlettersReportClinicians = _dotReportData.GetReportClinicians();
                _lvm.dictatedLettersSecTeamReports = _dotReportData.GetDictatedLettersReport();

                return View(_lvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName="DictatedLetter" });
            }
        }

        public async Task<IActionResult> DictatedLettersForPatient(string cguNo)
        {
            try
            {
                if (User.Identity.Name is null)
                {
                    return NotFound();
                }

                var user = _staffUser.GetStaffMemberDetails(User.Identity.Name);

                //IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(user.STAFF_CODE, "AdminX - Letters", "", _ip.GetIPAddress());

                _lvm.patientDetails = _patientData.GetPatientDetailsByCGUNo(cguNo);

                if (_lvm.patientDetails != null)
                {
                    var letters = _dictatedLetterData.GetDictatedLettersForPatient(_lvm.patientDetails.MPI);

                    _lvm.dictatedLettersForApproval = letters.Where(l => l.Status != "For Printing" && l.Status != "Printed").ToList();
                    _lvm.dictatedLettersForPrinting = letters.Where(l => l.Status == "For Printing").ToList();
                }

                return View(_lvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {            
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;

                //IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Edit Letter", "ID=" + id.ToString(), _ip.GetIPAddress());

                _lvm.dictatedLetters = _dictatedLetterData.GetDictatedLetterDetails(id);
                _lvm.dictatedLettersPatients = _dictatedLetterData.GetDictatedLettersPatientsList(id);
                _lvm.dictatedLettersCopies = _dictatedLetterData.GetDictatedLettersCopiesList(id);
                _lvm.patients = _dictatedLetterData.GetDictatedLetterPatientsList(id);
                _lvm.staffMemberList = _staffUser.GetClinicalStaffList();
                _lvm.secteams = _staffUser.GetSecTeamsList();
                _lvm.consultants = _staffUser.GetConsultantsList();
                _lvm.gcs = _staffUser.GetGCList();                
                int? mpi = _lvm.dictatedLetters.MPI;
                int? refID = _lvm.dictatedLetters.RefID;
                _lvm.patientDetails = _patientData.GetPatientDetails(mpi.GetValueOrDefault());
                _lvm.activityDetails = _activityData.GetActivityDetails(refID.GetValueOrDefault());
                string sGPCode = _lvm.patientDetails.GP_Facility_Code;
                if (sGPCode == null ) { sGPCode = "Unknown1"; } //because obviously there are nulls.
                string sRefFacCode = _lvm.activityDetails.REF_FAC;
                if (sRefFacCode == null) { sRefFacCode = "Unknown"; } 
                string sRefPhysCode = _lvm.activityDetails.REF_PHYS;
                if (sRefPhysCode == null) { sRefPhysCode = "Unknown"; }
                _lvm.referrerFacility = _externalFacilityData.GetFacilityDetails(sRefFacCode);                
                _lvm.referrer = _externalClinicianData.GetClinicianDetails(sRefPhysCode);                
                _lvm.GPFacility = _externalFacilityData.GetFacilityDetails(sGPCode);
                _lvm.facilities = _externalFacilityData.GetFacilityList().Where(f => f.IS_GP_SURGERY == 0).ToList();
                _lvm.clinicians = _externalClinicianData.GetClinicianList().Where(c => c.Is_GP == 0 && c.LAST_NAME != null && c.FACILITY != null).ToList();                
                List<ExternalCliniciansAndFacilities> extClins = _lvm.clinicians.Where(c => c.POSITION != null).ToList();                
                _lvm.specialities = _externalClinicianData.GetClinicianTypeList();
                _lvm.edmsLink = _constantsData.GetConstant("GEMRLink", 1);

                ViewBag.Breadcrumbs = new List<BreadcrumbItem>
                {
                    new BreadcrumbItem { Text = "Home", Controller = "Home", Action = "Index" },
                    new BreadcrumbItem { Text = "Letters", Controller = "DictatedLetter", Action = "Index"  },

                    new BreadcrumbItem { Text = "Update" }
                };

                return View(_lvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-edit" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int dID, string status, string letterTo, string letterFromCode, string letterContent, string letterContentBold, 
            bool isAddresseeChanged, string secTeam, string consultant, string gc, string dateDictated, string letterToCode, string enclosures, string comments, string letterFrom)
        {
            try
            {
                DateTime dDateDictated = new DateTime();
                dDateDictated = DateTime.Parse(dateDictated);
                //two updates required - one to update the addressee (if addressee has changed)
                if (isAddresseeChanged)
                {
                    int success2 = _crud.CallStoredProcedure("Letter", "UpdateAddresses", dID, 0, 0, "", letterToCode, letterFromCode, letterTo, User.Identity.Name);

                    if (success2 == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DictatedLetter-edit(SQL)" }); }
                }

                int success = _crud.CallStoredProcedure("Letter", "Update", dID, 0, 0, status, enclosures, letterContentBold, letterContent, User.Identity.Name, dDateDictated, null, false, false, 0, 0, 0, secTeam, consultant, gc, 0,0,0,0,0, comments, letterFrom );

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DictatedLetter-edit(SQL)" }); }

                return RedirectToAction("Edit", new { id = dID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter" });
            }
        }

        public async Task<IActionResult> Create(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;                
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Create Letter", "New Letter", _ip.GetIPAddress());

                int success = _crud.CallStoredProcedure("Letter", "Create", 0, id, 0, "", "", staffCode, "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DictatedLetter-create(SQL)" }); }

                var dot = await _clinContext.DictatedLetters.OrderByDescending(l => l.CreatedDate).FirstOrDefaultAsync(l => l.RefID == id);
                int dID = dot.DoTID;

                return RedirectToAction("Edit", new { id = dID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-create" });
            }
        }

        public async Task<IActionResult> Delete(int dID)
        {
            try 
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Delete Letter", "ID=" + dID.ToString(), _ip.GetIPAddress());

                int success = _crud.CallStoredProcedure("Letter", "Delete", dID, 0, 0, "", "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DictatedLetter-create(SQL)" }); }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-create" });
            }
        }        

        [HttpPost]
        public async Task<IActionResult> AddPatientToDOT(int pID, int dID)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Add Patient to DOT", "ID=" + dID.ToString(), _ip.GetIPAddress());

                int success = _crud.CallStoredProcedure("Letter", "AddFamilyMember", dID, pID, 0, "", "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DictatedLetter-addPt(SQL)" }); }

                return RedirectToAction("Edit", new { id = dID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-addPt" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddCCToDOT(int dID, string cc)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Add CC to DOT", "ID=" + dID.ToString(), _ip.GetIPAddress());

                int success = _crud.CallStoredProcedure("Letter", "AddCC", dID, 0, 0, cc, "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DictatedLetter-addCC(SQL)" }); }

                return RedirectToAction("Edit", new { id = dID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-addCC" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCCFromDOT(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Delete CC from DOT", "ID=" + id.ToString(), _ip.GetIPAddress());

                var letter = _dictatedLetterData.GetDictatedLetterCopyDetails(id);
                
                int dID = letter.DotID;

                int success = _crud.CallStoredProcedure("Letter", "DeleteCC", id, 0, 0, "", "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DictatedLetter-deleteCC(SQL)" }); }

                return RedirectToAction("Edit", new { id = dID });
                
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-deleteCC" });
            }
        }

        public async Task<IActionResult> PreviewDOT(int dID)
        {
            //try
            //{                
            //    _lc.PrintDOTPDF(dID, User.Identity.Name, true);
            //    //return RedirectToAction("Edit", new { id = dID });
            //    return File($"~/DOTLetterPreviews/preview-{User.Identity.Name}.pdf", "Application/PDF");
            //}
            //catch (Exception ex)
            //{
            //    return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-preview" });
            //}

            try
            {
                string user = User.Identity.Name;

                byte[] pdfData = GenerateDOTPDFStream(dID, user, true);

                return File(pdfData, "application/pdf");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-preview" });
            }
        }

        private byte[] GenerateDOTPDFStream(int dID, string user, bool isPreview)
        {
            LetterVM localLvm = new LetterVM();

           
            localLvm.staffMember = _staffUser.GetStaffMemberDetails(user);
            localLvm.dictatedLetter = _dictatedLetterData.GetDictatedLetterDetails(dID);

            var docContent = _docContext.DocumentsContent.FirstOrDefault(d => d.OurAddress != null);
            string ourAddress = docContent != null ? docContent.OurAddress : "Address Not Found";

            MigraDoc.DocumentObjectModel.Document document = new MigraDoc.DocumentObjectModel.Document();
            Section section = document.AddSection();

            // Logo
            Paragraph contentLogo = section.AddParagraph();
            string logoPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\Letterhead.jpg");
            if (System.IO.File.Exists(logoPath))
            {
                MigraDoc.DocumentObjectModel.Shapes.Image imgLogo = contentLogo.AddImage(logoPath);
                imgLogo.ScaleWidth = new Unit(0.5, UnitType.Point);
                imgLogo.ScaleHeight = new Unit(0.5, UnitType.Point);
            }
            contentLogo.Format.Alignment = ParagraphAlignment.Right;

            // Title
            Paragraph spacer = section.AddParagraph();
            Paragraph title = section.AddParagraph();
            title.AddFormattedText("WEST MIDLANDS REGIONAL CLINICAL GENETICS SERVICE", TextFormat.Bold);
            title.Format.Alignment = ParagraphAlignment.Center;
            title.Format.Font.Size = 12;
            spacer = section.AddParagraph();

            // Header Table
            Table table = section.AddTable();
            Column contactInfo = table.AddColumn();
            contactInfo.Format.Alignment = ParagraphAlignment.Left;
            Column ourAddressInfo = table.AddColumn();
            ourAddressInfo.Format.Alignment = ParagraphAlignment.Right;
            table.Rows.Height = 50;
            table.Columns.Width = 250;
            table.Format.Font.Size = 12;

            Row row1 = table.AddRow();
            row1.VerticalAlignment = VerticalAlignment.Top;

            string clinicianHeader = $"Consultant: {localLvm.dictatedLetter.Consultant}" + Environment.NewLine + $"Genetic Counsellor: {localLvm.dictatedLetter.GeneticCounsellor}";
            string phoneNumbers = "Secretaries Direct Line:" + Environment.NewLine;

            var secretariesList = _staffUser.GetStaffMemberList().Where(s => s.BILL_ID == localLvm.dictatedLetter.SecTeam && s.CLINIC_SCHEDULER_GROUPS == "Admin");
            foreach (var t in secretariesList)
            {
                phoneNumbers += $"{t.NAME} {t.TELEPHONE}" + Environment.NewLine;
            }

            row1.Cells[0].AddParagraph(clinicianHeader + Environment.NewLine + Environment.NewLine + phoneNumbers);
            row1.Cells[1].AddParagraph(ourAddress + Environment.NewLine + Environment.NewLine + _constantsData.GetConstant("MainCGUEmail", 1));

            // Dates
            string datesInfo = "";
            if (localLvm.dictatedLetter.DateDictated != null)
            {
                datesInfo = $"Dictated Date: {localLvm.dictatedLetter.DateDictated.Value:dd/MM/yyyy}" + Environment.NewLine +
                            $"Date Typed: {localLvm.dictatedLetter.CreatedDate.Value:dd/MM/yyyy}";
            }

            localLvm.patient = _patientData.GetPatientDetails(localLvm.dictatedLetter.MPI.GetValueOrDefault());

            spacer = section.AddParagraph();
            Paragraph contentRefNo = section.AddParagraph($"Please quote our reference on all correspondence: {Environment.NewLine} {localLvm.patient.CGU_No}");
            contentRefNo.Format.Font.Size = 12;

            spacer = section.AddParagraph();
            Paragraph contentDatesInfo = section.AddParagraph(datesInfo);
            contentDatesInfo.Format.Font.Size = 12;

            // Address
            string address = localLvm.dictatedLetter.LetterTo;
            spacer = section.AddParagraph();
            spacer = section.AddParagraph();

            Paragraph contentPatientAddress = section.AddParagraph(address);
            contentPatientAddress.Format.Font.Size = 12;

            spacer = section.AddParagraph();
            Paragraph contentToday = section.AddParagraph(DateTime.Today.ToString("dd MMMM yyyy"));
            contentToday.Format.Font.Size = 12;

            spacer = section.AddParagraph();
            Paragraph contentSalutation = section.AddParagraph($"Dear {localLvm.dictatedLetter.LetterToSalutation}");
            contentSalutation.Format.Font.Size = 12;

            // Letter Body
            spacer = section.AddParagraph();
            Paragraph contentLetterRe = section.AddParagraph();
            contentLetterRe.AddFormattedText(localLvm.dictatedLetter.LetterRe, TextFormat.Bold);
            contentLetterRe.Format.Font.Size = 12;

            spacer = section.AddParagraph();
            Paragraph contentSummary = section.AddParagraph();
            contentSummary.AddFormattedText(localLvm.dictatedLetter.LetterContentBold, TextFormat.Bold);
            contentSummary.Format.Font.Size = 12;

            spacer = section.AddParagraph();
            string letterContent = RemoveHTML(localLvm.dictatedLetter.LetterContent);
            Paragraph contentLetterContent = section.AddParagraph();
            contentLetterContent.Format.Font.Size = 12;

            if (letterContent.Contains("<<strong>>"))
            {
                List<string> letterContentParts = ParseBold(letterContent);
                foreach (var item in letterContentParts)
                {
                    if (item.Contains("NOTBOLD"))
                    {
                        contentLetterContent.AddFormattedText(item.Replace("NOTBOLD", ""), TextFormat.NotBold);
                    }
                    else if (item.Contains("BOLD"))
                    {
                        contentLetterContent.AddFormattedText(item.Replace("BOLD", ""), TextFormat.Bold);
                    }
                    else
                    {
                        contentLetterContent.AddFormattedText(item, TextFormat.NotBold);
                    }
                }
            }
            else
            {
                contentLetterContent.AddFormattedText(letterContent, TextFormat.NotBold);
            }

            // Sign Off
            string signOff = localLvm.staffMember.NAME + Environment.NewLine + localLvm.staffMember.POSITION;
            string sigFilename = $"{localLvm.staffMember.StaffForename.Replace(" ", "")}{localLvm.staffMember.StaffSurname.Replace("'", "").Replace(" ", "")}.jpg";
            string sigPath = Path.Combine(Directory.GetCurrentDirectory(), $@"wwwroot\Signatures\{sigFilename}");

            spacer = section.AddParagraph();
            spacer = section.AddParagraph();

            Paragraph contentSignOff = section.AddParagraph("Yours sincerely,");
            contentSignOff.Format.Font.Size = 12;

            spacer = section.AddParagraph();
            Paragraph contentSig = section.AddParagraph();
            if (System.IO.File.Exists(sigPath))
            {
                MigraDoc.DocumentObjectModel.Shapes.Image sig = contentSig.AddImage(sigPath);
            }

            spacer = section.AddParagraph();
            Paragraph contentSignOffName = section.AddParagraph(signOff);
            contentSignOffName.Format.Font.Size = 12;

            if (!string.IsNullOrEmpty(localLvm.dictatedLetter.Enclosures))
            {
                spacer = section.AddParagraph();
                spacer = section.AddParagraph();
                Paragraph enclosures = section.AddParagraph("Enclosures: " + localLvm.dictatedLetter.Enclosures);
                enclosures.Format.Font.Size = 12;
            }

            List<DictatedLettersCopy> ccList = _dictatedLetterData.GetDictatedLettersCopiesList(localLvm.dictatedLetter.DoTID);
            if (ccList.Count > 0)
            {
                section.AddPageBreak();
                foreach (var item in ccList)
                {
                    spacer = section.AddParagraph();
                    spacer = section.AddParagraph();
                    Table tableCC = section.AddTable();
                    Column colCC = tableCC.AddColumn();
                    Column colADDRESS = tableCC.AddColumn();
                    Row rowcc = tableCC.AddRow();
                    colCC.Width = 20;
                    colADDRESS.Width = 300;
                    rowcc[0].AddParagraph("cc:");
                    rowcc[1].AddParagraph(item.CC);
                    spacer = section.AddParagraph();
                }
            }

            spacer = section.AddParagraph();
            Paragraph contentDocCode = section.AddParagraph("Letter code: DOT");
            contentDocCode.Format.Alignment = ParagraphAlignment.Right;
            contentDocCode.Format.Font.Size = 8;

            PdfDocumentRenderer pdf = new PdfDocumentRenderer();
            pdf.Document = document;
            pdf.RenderDocument();

            using (MemoryStream stream = new MemoryStream())
            {
                pdf.PdfDocument.Save(stream, false);
                return stream.ToArray();
            }
        }

        private string RemoveHTML(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";

            text = text.Replace("<div><font face=Arial size=3>", "");
            text = text.Replace("</font></div>", "");
            text = text.Replace("<div>&nbsp;</div>", "");
            text = text.Replace("&nbsp;", "");
            text = text.Replace("&amp;", "&");
            text = text.Replace(Environment.NewLine, "newline");
            text = text.Replace("newlinenewlinenewlinenewline", Environment.NewLine + Environment.NewLine);
            text = text.Replace("newlinenewline", Environment.NewLine);
            text = text.Replace("newline", "");
            text = text.Replace("<br>", Environment.NewLine + Environment.NewLine);
            text = text.Replace("<div>", "");
            text = text.Replace("</div>", "");
            text = text.Replace("b>", "strong>");

            if (text.Contains("<span style=\"font-weight: 600;\">"))
            {
                text = text.Replace("<span style=\"font-weight: 600;\">", "<strong>");
            }

            text = text.Replace("</span>", "</strong>");
            text = text.Replace("<strong>", "<<strong>>");
            text = text.Replace("</strong>", "<</strong>>");

            return text;
        }

        private List<string> ParseBold(string text)
        {
            List<string> newText = new List<string>();
            if (string.IsNullOrEmpty(text)) return newText;

            if (text.Contains("<strong>"))
            {
                string[] textBlocks = text.Split(new[] { "strong>>" }, StringSplitOptions.None);

                foreach (var item in textBlocks)
                {
                    if (item.Contains("<</"))
                    {
                        newText.Add(item.Replace("<</", "") + "BOLD ");
                    }
                    else if (item.Contains("<<"))
                    {
                        newText.Add(item.Replace("<<", "") + "NOTBOLD ");
                    }
                    else
                    {
                        newText.Add(item);
                    }
                }
            }
            return newText;
        }

        public async Task<IActionResult> PrintDOT(int dID)
        {
            try
            {
                //_lc.PrintDOTPDF(dID, User.Identity.Name, false);
                //return RedirectToAction("Edit", new { id = dID });
                string user = User.Identity.Name;

                byte[] pdfData = GenerateDOTPDFStream(dID, user, false);
                _crud.CallStoredProcedure("DictatedLetter", "Print", dID,0,0,"","","","",User.Identity.Name,null,null); //updates everything to say the letter was printed
                //return File($"~/DOTLetterPreviews/preview-{User.Identity.Name}.pdf", "Application/PDF");
                return File(pdfData, "application/pdf");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-preview" });
            }
        }

        public async Task<IActionResult> ActivityItems(int id)
        {
            try
            {
                _lvm.patientDetails = _patientData.GetPatientDetails(id);
                _lvm.activities = _activityData.GetActivityList(id);
                
                return View(_lvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-activityitems" });
            }

        }
    }
}
