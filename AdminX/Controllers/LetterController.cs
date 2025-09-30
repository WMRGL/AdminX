using ClinicalXPDataConnections.ViewModels;
using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Models;
using ClinicalXPDataConnections.Meta;
using System.Text.RegularExpressions;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;
using PdfSharpCore.Pdf;
using System.Drawing;
using MigraDoc.DocumentObjectModel.Tables;


namespace AdminX.Controllers
{

    public class LetterControllerLOCAL
    {
        private readonly ClinicalContext _clinContext;
        private readonly DocumentContext _docContext;
        private readonly LetterVM _lvm;
        private readonly IPatientData _patientData;
        private readonly IRelativeData _relativeData;
        private readonly IReferralData _referralData;
        private readonly IDictatedLetterData _dictatedLetterData;
        private readonly IStaffUserData _staffUser;
        private readonly IDocumentsData _documentsData;
        private readonly IExternalClinicianData _externalClinicianData;
        private readonly IExternalFacilityData _externalFacilityData;
        private readonly IConstantsData _constantsData;
        private readonly IAddressLookup _add;
        private readonly ILeafletData _leafletData;
        private readonly IAlertData _alertData;

        public LetterControllerLOCAL(ClinicalContext clinContext, DocumentContext docContext) //to be used for testing only
        {
            _clinContext = clinContext;
            _docContext = docContext;
            _lvm = new LetterVM();
            _patientData = new PatientData(_clinContext);
            _relativeData = new RelativeData(_clinContext);
            _referralData = new ReferralData(_clinContext);
            _staffUser = new StaffUserData(_clinContext);
            _dictatedLetterData = new DictatedLetterData(_clinContext);
            _documentsData = new DocumentsData(_docContext);
            _externalClinicianData = new ExternalClinicianData(_clinContext);
            _externalFacilityData = new ExternalFacilityData(_clinContext);
            _constantsData = new ConstantsData(_docContext);
            _add = new AddressLookup(_clinContext, _docContext);
            _leafletData = new LeafletData(_docContext);
            _alertData = new AlertData(_clinContext);
        }
               

        //Creates a preview of the DOT letter
        public void PrintDOTPDF(int dID, string user, bool isPreview)
        {

            _lvm.staffMember = _staffUser.GetStaffMemberDetails(user);
            _lvm.dictatedLetter = _dictatedLetterData.GetDictatedLetterDetails(dID);
            string ourAddress = _docContext.DocumentsContent.FirstOrDefault(d => d.OurAddress != null).OurAddress;
            //creates a new PDF document
            MigraDoc.DocumentObjectModel.Document document = new MigraDoc.DocumentObjectModel.Document();

            Section section = document.AddSection();

            Paragraph contentLogo = section.AddParagraph();

            MigraDoc.DocumentObjectModel.Shapes.Image imgLogo = contentLogo.AddImage(@"wwwroot\Letterhead.jpg");
            imgLogo.ScaleWidth = new Unit(0.5, UnitType.Point);
            imgLogo.ScaleHeight = new Unit(0.5, UnitType.Point);

            contentLogo.Format.Alignment = ParagraphAlignment.Right;

            Paragraph spacer = section.AddParagraph();
            Paragraph title = section.AddParagraph();
            title.AddFormattedText("WEST MIDLANDS REGIONAL CLINICAL GENETICS SERVICE", TextFormat.Bold);
            title.Format.Alignment = ParagraphAlignment.Center;

            spacer = section.AddParagraph();

            MigraDoc.DocumentObjectModel.Tables.Table table = section.AddTable();
            MigraDoc.DocumentObjectModel.Tables.Column contactInfo = table.AddColumn();
            contactInfo.Format.Alignment = ParagraphAlignment.Left;
            MigraDoc.DocumentObjectModel.Tables.Column ourAddressInfo = table.AddColumn();
            ourAddressInfo.Format.Alignment = ParagraphAlignment.Right;
            table.Rows.Height = 50;
            table.Columns.Width = 250;
            MigraDoc.DocumentObjectModel.Tables.Row row1 = table.AddRow();
            row1.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Top;
            MigraDoc.DocumentObjectModel.Tables.Row row2 = table.AddRow();
            row2.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;

            string clinicianHeader = $"Consultant: {_lvm.dictatedLetter.Consultant}" + System.Environment.NewLine + $"Genetic Counsellor: {_lvm.dictatedLetter.GeneticCounsellor}";

            row1.Cells[0].AddParagraph(clinicianHeader);
            row1.Cells[0].Format.Font.Bold = true;
            row1.Cells[1].AddParagraph(ourAddress);

            string phoneNumbers = "Secretaries Direct Line:" + System.Environment.NewLine;

            var secretariesList = _staffUser.GetStaffMemberList().Where(s => s.BILL_ID == _lvm.dictatedLetter.SecTeam && s.CLINIC_SCHEDULER_GROUPS == "Admin");
            foreach (var t in secretariesList)
            {
                phoneNumbers = phoneNumbers + $"{t.NAME} {t.TELEPHONE}" + System.Environment.NewLine;
            }

            row2.Cells[0].AddParagraph(phoneNumbers);
            row2.Cells[1].AddParagraph(_constantsData.GetConstant("MainCGUEmail", 1));

            string datesInfo = "";

            if (_lvm.dictatedLetter.DateDictated != null)
            {
                datesInfo = $"Dictated Date: {_lvm.dictatedLetter.DateDictated.Value.ToString("dd/MM/yyyy")}" + System.Environment.NewLine +
                                   $"Date Typed: {_lvm.dictatedLetter.CreatedDate.Value.ToString("dd/MM/yyyy")}";
            }
            _lvm.patient = _patientData.GetPatientDetails(_lvm.dictatedLetter.MPI.GetValueOrDefault());

            spacer = section.AddParagraph();
            Paragraph contentRefNo = section.AddParagraph($"Please quote our reference on all correspondence: {_lvm.patient.CGU_No}");
            spacer = section.AddParagraph();
            Paragraph contentDatesInfo = section.AddParagraph(datesInfo);

            string address = "";
            address = _lvm.dictatedLetter.LetterTo;

            spacer = section.AddParagraph();
            spacer = section.AddParagraph();
            Paragraph contentPatientAddress = section.AddParagraph(address);
            spacer = section.AddParagraph();
            Paragraph contentToday = section.AddParagraph(DateTime.Today.ToString("dd MMMM yyyy"));
            spacer = section.AddParagraph();
            Paragraph contentSalutation = section.AddParagraph($"Dear {_lvm.dictatedLetter.LetterToSalutation}");
            spacer = section.AddParagraph();
            Paragraph contentLetterRe = section.AddParagraph();
            contentLetterRe.AddFormattedText(_lvm.dictatedLetter.LetterRe, TextFormat.Bold);
            spacer = section.AddParagraph();
            Paragraph contentSummary = section.AddParagraph();
            contentSummary.AddFormattedText(_lvm.dictatedLetter.LetterContentBold, TextFormat.Bold);
            spacer = section.AddParagraph();

            string letterContent = RemoveHTML(_lvm.dictatedLetter.LetterContent);

            Paragraph contentLetterContent = section.AddParagraph(letterContent);

            string signOff = _lvm.dictatedLetter.LetterFrom;
            string sigFilename = $"{_lvm.staffMember.StaffForename.Replace(" ", "")}{_lvm.staffMember.StaffSurname.Replace("'", "").Replace(" ", "")}.jpg";



            spacer = section.AddParagraph();
            spacer = section.AddParagraph();

            Paragraph contentSignOff = section.AddParagraph("Yours sincerely,");
            
            spacer = section.AddParagraph();
            Paragraph contentSig = section.AddParagraph();
            if (System.IO.File.Exists(@$"wwwroot\Signatures\{sigFilename}"))
            {
                MigraDoc.DocumentObjectModel.Shapes.Image sig = contentSig.AddImage(@$"wwwroot\Signatures\{sigFilename}");
            }
            spacer = section.AddParagraph();
            Paragraph contentSignOffName = section.AddParagraph(signOff);

            int printCount = 1;

            string[] ccs = { "", "", "" };

            List<DictatedLettersCopy> ccList = _dictatedLetterData.GetDictatedLettersCopiesList(_lvm.dictatedLetter.DoTID);


            if (ccList.Count() > 0)
            {
                spacer = section.AddParagraph();
                spacer = section.AddParagraph();
                Paragraph ccHead = section.AddParagraph("CC:");

                foreach (var item in ccList)
                {
                    spacer = section.AddParagraph();
                    spacer = section.AddParagraph();
                    Paragraph contentCC = section.AddParagraph(item.CC);
                    spacer = section.AddParagraph();
                    spacer = section.AddParagraph();
                    printCount = printCount += 1;
                }
            }
                        

            PdfDocumentRenderer pdf = new PdfDocumentRenderer();
            pdf.Document = document;
            pdf.RenderDocument();
            pdf.PdfDocument.Save(Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\DOTLetterPreviews\\preview-{user}.pdf"));


            if (!isPreview)
            {
                string fileCGU = _lvm.patient.CGU_No.Replace(".", "-");
                string mpiString = _lvm.patient.MPI.ToString();
                string refIDString = _lvm.dictatedLetter.RefID.ToString();
                string dateTimeString = DateTime.Now.ToString("yyyyMMddHHmmss");


                System.IO.File.Copy($"wwwroot\\DOTLetterPreviews\\preview-{user}.pdf", $@"C:\CGU_DB\Letters\DOTLetter-{fileCGU}-DOT-{mpiString}-0-{refIDString}-{printCount.ToString()}-{dateTimeString}-{dID.ToString()}.pdf");

                /*                 
                can't actually print it because there's no way to give it your username, so it'll all be under the server's name
                */
            }
        }

        public void DoPDF(int id, int mpi, int refID, string user, string referrer, string? additionalText = "", string? enclosures = "", int? reviewAtAge = 0,
            string? tissueType = "", bool? isResearchStudy = false, bool? isScreeningRels = false, int? diaryID = 0, string? freeText1 = "", string? freeText2 = "",
            int? relID = 0, string? clinicianCode = "", string? siteText = "", DateTime? diagDate = null, bool? isPreview = false, string? qrCodeText = "", int? leafletID = 0)
        {
            _lvm.staffMember = _staffUser.GetStaffMemberDetails(user);
            _lvm.patient = _patientData.GetPatientDetails(mpi);
            _lvm.documentsContent = _documentsData.GetDocumentDetails(id);
            _lvm.referrer = _externalClinicianData.GetClinicianDetails(referrer);
            _lvm.gp = _externalClinicianData.GetClinicianDetails(_lvm.patient.GP_Code);
            _lvm.other = _externalClinicianData.GetClinicianDetails(clinicianCode);

            var referral = _referralData.GetReferralDetails(refID);
            string docCode = _lvm.documentsContent.DocCode;
            string name = "";
            string patName = _lvm.patient.Title + " " + _lvm.patient.FIRSTNAME + " " + _lvm.patient.LASTNAME;
            string address = "";
            string patAddress = "";
            string salutation = "";
            DateTime patDOB = DateTime.Now; //have to give it an initial value or the program throws a fit
            if(_lvm.patient.DOB != null) { patDOB = _lvm.patient.DOB.GetValueOrDefault(); } //because you KNOW there's gonna be a null!
            string content1 = "";
            string content2 = "";
            string content3 = "";
            string content4 = "";
            string content5 = "";
            string content6 = "";
            string freetext = freeText1;
            string quoteRef = "";
            string signOff = "";
            string sigFilename = "";


            if (docCode.Contains("CF"))
            {
                DoConsentForm(id, mpi, refID, user, referrer, additionalText, enclosures, reviewAtAge = 0, tissueType, isResearchStudy, isScreeningRels, diaryID, freeText1,
                    freeText2, relID, clinicianCode, siteText, diagDate, isPreview);
            }
            else
            {
                MigraDoc.DocumentObjectModel.Document document = new MigraDoc.DocumentObjectModel.Document();
                Section section = document.AddSection();

                MigraDoc.DocumentObjectModel.Tables.Table table = section.AddTable();
                MigraDoc.DocumentObjectModel.Tables.Column contactInfo = table.AddColumn();
                MigraDoc.DocumentObjectModel.Tables.Column logo = table.AddColumn();
                contactInfo.Format.Alignment = ParagraphAlignment.Left;
                logo.Format.Alignment = ParagraphAlignment.Right;
                //MigraDoc.DocumentObjectModel.Tables.Column ourAddressInfo = table.AddColumn();
                //ourAddressInfo.Format.Alignment = ParagraphAlignment.Right;
                MigraDoc.DocumentObjectModel.Tables.Row row1 = table.AddRow();
                row1.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Top;
                MigraDoc.DocumentObjectModel.Tables.Row row2 = table.AddRow();
                row2.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;
                MigraDoc.DocumentObjectModel.Tables.Row row3 = table.AddRow();
                row3.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;

                Paragraph spacer = section.AddParagraph();

                if (!_lvm.documentsContent.LetterTo.Contains("CF"))
                {
                    table.Columns.Width = 240;
                    contactInfo.Width = 300;
                    logo.Width = 180;
                    row1.Height = 100;
                    row2.Height = 110;
                    row3.Height = 20;


                    quoteRef = "Please quote this reference on all correspondence: " + _lvm.patient.CGU_No + Environment.NewLine;
                    quoteRef = quoteRef + "NHS number: " + _lvm.patient.SOCIAL_SECURITY + Environment.NewLine;
                    quoteRef = quoteRef + "Consultant: " + referral.LeadClinician + Environment.NewLine;
                    quoteRef = quoteRef + "Genetic Counsellor: " + referral.GC;

                    row1.Cells[0].AddParagraph(quoteRef);
                    MigraDoc.DocumentObjectModel.Shapes.Image imgLogo = row1.Cells[1].AddImage(@"wwwroot\Letterhead.jpg");
                    imgLogo.ScaleWidth = new Unit(0.5, UnitType.Point);
                    imgLogo.ScaleHeight = new Unit(0.5, UnitType.Point);

                    row2.Cells[1].AddParagraph(_lvm.documentsContent.OurAddress);

                    row3.Cells[0].AddParagraph(DateTime.Today.ToString("dd MMMM yyyy"));
                    if (_lvm.documentsContent.OurEmailAddress != null) //because obviously there's a null.
                    {
                        row3.Cells[1].AddParagraph(_lvm.documentsContent.OurEmailAddress);
                    }

                }
                else
                {
                    Paragraph contentOurAddress = section.AddParagraph(_lvm.documentsContent.OurAddress);
                    //contentOurAddress.Format.Font.Size = 12;
                    contentOurAddress.Format.Alignment = ParagraphAlignment.Right;
                }



                /* //do we actually need this?
                if (relID == 0)
                {
                    patAddress = _add.GetAddress("PT", refID);
                }
                else
                {
                    _lvm.relative = _relativeData.GetRelativeDetails(relID.GetValueOrDefault());

                    patName = _lvm.relative.Name;
                    patDOB = _lvm.relative.DOB.GetValueOrDefault();
                    salutation = _lvm.relative.Name;

                    patAddress = _add.GetAddress("PTREL", refID, relID);

                }
                */

                patAddress = _add.GetAddress("PT", refID);

                if (_lvm.documentsContent.LetterTo == "PT" || _lvm.documentsContent.LetterTo == "PTREL")
                {
                    if (docCode != "CF01")
                    {
                        name = _lvm.patient.PtLetterAddressee; //relatives' letters get sent to the patient - we don't contact the relative directly, and 
                        salutation = _lvm.patient.SALUTATION; //don't even store their address most of the time

                        //patAddress = _add.GetAddress("PT", refID);
                        address = patAddress;
                    }
                }

                if (_lvm.documentsContent.LetterTo == "RD")
                {
                    address = _add.GetAddress("RD", refID);
                }

                if (_lvm.documentsContent.LetterTo == "GP")
                {
                    address = _add.GetAddress("GP", refID);
                }

                if (_lvm.documentsContent.LetterTo == "Other" || _lvm.documentsContent.LetterTo == "Histo")
                {
                    ExternalClinician clinician = _externalClinicianData.GetClinicianDetails(clinicianCode);
                    name = clinician.TITLE + " " + clinician.FIRST_NAME + " " + clinician.NAME;
                    var hospital = _externalFacilityData.GetFacilityDetails(clinician.FACILITY);
                    salutation = clinician.TITLE + " " + clinician.FIRST_NAME + " " + clinician.NAME;
                    address = salutation + Environment.NewLine;
                    address += hospital.ADDRESS + Environment.NewLine;
                    address += hospital.CITY + Environment.NewLine;
                    address += hospital.STATE + Environment.NewLine;
                    address += hospital.ZIP + Environment.NewLine;
                }


                row2.Cells[0].AddParagraph(address);

                if (_lvm.documentsContent.LetterTo == "PTREL" || _lvm.documentsContent.LetterTo == "Other" || _lvm.documentsContent.DocCode == "DT13")
                {
                    Paragraph contentQuoteRef = section.AddParagraph("CGU No. : " + _lvm.patient.CGU_No);
                }
                spacer = section.AddParagraph();

                if (address != "")
                {
                    Paragraph contentSalutation = section.AddParagraph("Dear " + salutation);
                    spacer = section.AddParagraph();
                }

                string referrerName = "";
                if (_lvm.referrer != null) { referrerName = _lvm.referrer.TITLE + " " + _lvm.referrer.FIRST_NAME + " " + _lvm.referrer.NAME; }
                string gpName = "";
                gpName = _lvm.gp.TITLE + " " + _lvm.gp.FIRST_NAME + " " + _lvm.gp.NAME;
                string otherName = "";
                if (_lvm.other != null)
                {
                    otherName = _lvm.other.TITLE + " " + _lvm.other.FIRST_NAME + " " + _lvm.other.NAME;
                }

                string[] ccs = { "", "", "" };

                int printCount = 0;

                if (_documentsData.GetDocumentData(docCode).HasAdditionalActions)
                {
                    printCount += 1;
                }

                int totalLength = 400; //used for spacing - so the paragraphs can dynamically resize
                int totalLength2 = 40;
                int pageCount = 1;

                string fileCGU = _lvm.patient.CGU_No.Replace(".", "-");
                string mpiString = _lvm.patient.MPI.ToString();
                string refIDString = refID.ToString();
                string dateTimeString = DateTime.Now.ToString("yyyyMMddHHmmss");
                string diaryIDString = diaryID.ToString();



                ///////////////////////////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////////////////////////
                //////All letter templates need to be defined individually here////////////////////////            
                ///////////////////////////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////////////////////////



                //Ack letter
                if (docCode == "Ack")
                {
                    content1 = _lvm.documentsContent.Para1;
                    Paragraph letterContent = section.AddParagraph(content1);
                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;

                }

                //CTB Ack letter
                if (docCode == "CTBAck")
                {
                    content1 = referrerName + " " + _lvm.documentsContent.Para1;
                    content2 = _lvm.documentsContent.Para2;
                    content3 = _lvm.documentsContent.Para3 + Environment.NewLine + Environment.NewLine + _lvm.documentsContent.Para4;
                    Paragraph letterContent1 = section.AddParagraph(content1);
                    spacer = section.AddParagraph();
                    Paragraph letterContent2 = section.AddParagraph(content2);
                    spacer = section.AddParagraph();
                    Paragraph letterContent3 = section.AddParagraph(content3);
                    signOff = "CGU Booking Centre";
                    ccs[0] = referrerName;
                }

                //CTBFol letter
                if(docCode == "CTBFol")
                {
                    content1 = _lvm.documentsContent.Para1;
                    Paragraph letterContent1 = section.AddParagraph(content1);
                    spacer = section.AddParagraph();
                    content2 = _lvm.documentsContent.Para2;
                    Paragraph letterContent2 = section.AddParagraph();
                    letterContent2.AddFormattedText(content2, TextFormat.Bold);
                    spacer = section.AddParagraph();
                    content3 = _lvm.documentsContent.Para3;
                    Paragraph letterContent3 = section.AddParagraph(content3);
                    spacer = section.AddParagraph();
                    content4 = _lvm.documentsContent.Para4;
                    Paragraph letterContent4 = section.AddParagraph(content4);
                    spacer = section.AddParagraph();
                    signOff = "CGU Booking Centre";
                }

                //CTB Rem letter
                if (docCode == "CTBRem")
                {
                    content1 = _lvm.documentsContent.Para1;
                    Paragraph letterContent1 = section.AddParagraph(content1);
                    spacer = section.AddParagraph();
                    content2 = _lvm.documentsContent.Para2;
                    Paragraph letterContent2 = section.AddParagraph();
                    letterContent2.AddFormattedText(content2, TextFormat.Bold);
                    spacer = section.AddParagraph();
                    content3 = _lvm.documentsContent.Para3 + Environment.NewLine + Environment.NewLine + _lvm.documentsContent.Para4;
                    Paragraph letterContent3 = section.AddParagraph(content3);
                    signOff = "CGU Booking Centre";                    
                }

                //CTB No Response letter
                if (docCode == "CTBNR")
                {
                    content1 = _lvm.documentsContent.Para1;
                    Paragraph letterContent1 = section.AddParagraph(content1);
                    spacer = section.AddParagraph();
                    content2 = _lvm.documentsContent.Para2;
                    Paragraph letterContent2 = section.AddParagraph(content2);
                    spacer = section.AddParagraph();
                    content3 = _lvm.documentsContent.Para3;
                    Paragraph letterContent3 = section.AddParagraph(content3);
                    signOff = "CGU Booking Centre";
                    ccs[0] = referrerName;
                    ccs[1] = gpName;
                }

                //K letters
                if (docCode == "Kc")
                {
                    pageCount = 2; //because this can't happen automatically, obviously, so we have to hard code it!

                    content1 = _lvm.documentsContent.Para1 + " " + referrerName + " " + _lvm.documentsContent.Para2 +
                        Environment.NewLine + Environment.NewLine + _lvm.documentsContent.Para3 +
                        Environment.NewLine + Environment.NewLine + _lvm.documentsContent.Para4;
                    content2 = _lvm.documentsContent.Para5;
                    Paragraph letterContent1 = section.AddParagraph(content1);
                    spacer = section.AddParagraph();
                    Paragraph letterContent2 = section.AddParagraph(content2);
                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                }

                if (docCode == "K")
                {
                    pageCount = 2;
                    content1 = _lvm.documentsContent.Para1 + " " + referrerName + " " + _lvm.documentsContent.Para2;
                    Paragraph letterContent1 = section.AddParagraph(content1);
                    spacer = section.AddParagraph();
                    content2 = _lvm.documentsContent.Para3;
                    Paragraph letterContent2 = section.AddParagraph(content2);
                    spacer = section.AddParagraph();
                    content3 = _lvm.documentsContent.Para4;
                    Paragraph letterContent3 = section.AddParagraph(content3);
                    spacer = section.AddParagraph();
                    content4 = _lvm.documentsContent.Para5;
                    Paragraph letterContent4 = section.AddParagraph(content4);
                    spacer = section.AddParagraph();
                    content5 = _lvm.documentsContent.Para6;
                    Paragraph letterContent5 = section.AddParagraph(content5);
                    spacer = section.AddParagraph();

                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                }

                if (docCode == "Krem")
                {
                    content1 = _lvm.documentsContent.Para1;
                    Paragraph letterContent1 = section.AddParagraph(content1);
                    spacer = section.AddParagraph();
                    content2 = _lvm.documentsContent.Para2;
                    Paragraph letterContent2 = section.AddParagraph(content2);
                    spacer = section.AddParagraph();
                    content3 = _lvm.documentsContent.Para3;
                    Paragraph letterContent3 = section.AddParagraph(content3);

                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                }


                //Endo letters
                if (docCode == "EndoAck")
                {
                    


                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                }

                if (docCode == "EndoRem")
                {



                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                }


                //Cardiac letters
                if (docCode == "CardAck")
                {



                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                }

                if (docCode == "CardRem")
                {



                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                }


                if (docCode == "RejFH")
                {
                    Paragraph contentRe = section.AddParagraph();
                    contentRe.AddFormattedText("Re: " + patName + ", " + patDOB.ToString("dd/MM/yyyy"), TextFormat.Bold);
                    spacer = section.AddParagraph();
                    Paragraph letterContent1 = section.AddParagraph(_lvm.documentsContent.Para1 + " " + patName + " " + _lvm.documentsContent.Para2);
                    spacer = section.AddParagraph();
                    Paragraph letterContent2 = section.AddParagraph(_lvm.documentsContent.Para3);
                    spacer = section.AddParagraph();
                    Paragraph letterContent3 = section.AddParagraph(_lvm.documentsContent.Para4);
                    spacer = section.AddParagraph();
                    Paragraph letterContent4 = section.AddParagraph(_lvm.documentsContent.Para5);
                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                }

                if (docCode == "RejFHAW")
                {
                    Paragraph contentRe = section.AddParagraph();
                    contentRe.AddFormattedText("Re: " + patName + ", " + patDOB.ToString("dd/MM/yyyy"), TextFormat.Bold);
                    Paragraph letterContent1 = section.AddParagraph(_lvm.documentsContent.Para1 + " " + patName + " " + _lvm.documentsContent.Para2);
                    spacer = section.AddParagraph();
                    Paragraph letterContent2 = section.AddParagraph(_lvm.documentsContent.Para2);
                    spacer = section.AddParagraph();
                    Paragraph letterContent3 = section.AddParagraph(_lvm.documentsContent.Para3);
                    spacer = section.AddParagraph();
                    Paragraph letterContent4 = section.AddParagraph(_lvm.documentsContent.Para4);
                    spacer = section.AddParagraph();
                    Paragraph letterContent5 = section.AddParagraph(_lvm.documentsContent.Para5);

                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                }

                //OOR1
                if (docCode == "OOR1")
                {
                    MigraDoc.DocumentObjectModel.Tables.Table table1 = section.AddTable();
                    MigraDoc.DocumentObjectModel.Tables.Column contentPatAddress = table.AddColumn();
                    contentPatAddress.Format.Alignment = ParagraphAlignment.Left;
                    MigraDoc.DocumentObjectModel.Tables.Column contentPatDOB = table.AddColumn();
                    contentPatDOB.Format.Alignment = ParagraphAlignment.Right;
                    table.Rows.Height = 50;
                    table.Columns.Width = 150;
                    MigraDoc.DocumentObjectModel.Tables.Row row1_1 = table1.AddRow();
                    row1_1.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Top;
                    row1_1.Format.Font.Bold = true;
                    row1_1.Cells[0].AddParagraph("Re: " + patName + System.Environment.NewLine + patAddress);
                    row1_1.Cells[1].AddParagraph("Date of Birth: " + patDOB.ToString("dd/MM/yyyy"));
                    spacer = section.AddParagraph();
                    Paragraph letterContent1 = section.AddParagraph(_lvm.documentsContent.Para1 + " " + patName + " " + _lvm.documentsContent.Para2);
                    spacer = section.AddParagraph();
                    Paragraph letterContent2 = section.AddParagraph(_lvm.documentsContent.Para2);
                    spacer = section.AddParagraph();
                    Paragraph letterContent3 = section.AddParagraph(_lvm.documentsContent.Para3);
                    spacer = section.AddParagraph();
                    Paragraph letterContent4 = section.AddParagraph(_lvm.documentsContent.Para4);
                }

                //OOR1
                if (docCode == "OOR2")
                {
                    MigraDoc.DocumentObjectModel.Tables.Table table1 = section.AddTable();
                    MigraDoc.DocumentObjectModel.Tables.Column contentPatAddress = table.AddColumn();
                    contentPatAddress.Format.Alignment = ParagraphAlignment.Left;
                    MigraDoc.DocumentObjectModel.Tables.Column contentPatDOB = table.AddColumn();
                    contentPatDOB.Format.Alignment = ParagraphAlignment.Right;
                    table.Rows.Height = 50;
                    table.Columns.Width = 100;
                    MigraDoc.DocumentObjectModel.Tables.Row row1_1 = table.AddRow();
                    row1_1.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Top;
                    row1_1.Format.Font.Bold = true;
                    row1_1.Cells[0].AddParagraph("Re: " + patName + System.Environment.NewLine + patAddress);
                    row1_1.Cells[1].AddParagraph("Date of Birth: " + patDOB.ToString("dd/MM/yyyy"));
                    spacer = section.AddParagraph();
                    Paragraph letterContent1 = section.AddParagraph(_lvm.documentsContent.Para1 + " " + patName + " " + _lvm.documentsContent.Para2);
                    spacer = section.AddParagraph();
                    Paragraph letterContent2 = section.AddParagraph(_lvm.documentsContent.Para2);
                    spacer = section.AddParagraph();
                    Paragraph letterContent3 = section.AddParagraph(_lvm.documentsContent.Para3);
                    spacer = section.AddParagraph();
                    Paragraph letterContent4 = section.AddParagraph(_lvm.documentsContent.Para4);
                }

                //PrC letter
                if (docCode == "PrC")
                {
                    content1 = _lvm.documentsContent.Para1;
                    content2 = _lvm.documentsContent.Para2;
                    content3 = _lvm.documentsContent.Para3;
                    content4 = _lvm.documentsContent.Para4;

                    Paragraph letterContent1 = section.AddParagraph(content1);
                    spacer = section.AddParagraph();
                    Paragraph letterContent2 = section.AddParagraph(content2);
                    spacer = section.AddParagraph();
                    Paragraph letterContent3 = section.AddParagraph(content3);
                    spacer = section.AddParagraph();
                    Paragraph letterContent4 = section.AddParagraph(content4);

                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                }

                //O1 letter
                if (docCode == "O1")
                {
                    content1 = _lvm.documentsContent.Para1 + Environment.NewLine + Environment.NewLine + _lvm.documentsContent.Para2;
                    content2 = additionalText;
                    content3 = _lvm.documentsContent.Para4;
                    content4 = _lvm.documentsContent.Para7;
                    content5 = _lvm.documentsContent.Para9;

                    Paragraph letterContent1 = section.AddParagraph(content1);
                    spacer = section.AddParagraph();
                    if (content2 != null && content2 != "")
                    {
                        Paragraph letterContent2 = section.AddParagraph();
                        letterContent2.AddFormattedText(content2, TextFormat.Bold);
                        spacer = section.AddParagraph();
                    }
                    Paragraph letterContent3 = section.AddParagraph(content3);
                    spacer = section.AddParagraph();
                    Paragraph letterContent4 = section.AddParagraph(content4);
                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                    ccs[0] = referrerName;
                    ccs[1] = gpName;
                }

                //O1a
                if (docCode == "O1A")
                {
                    content1 = _lvm.documentsContent.Para1 + Environment.NewLine + Environment.NewLine + _lvm.documentsContent.Para2;
                    content2 = freeText1;
                    if (reviewAtAge > 0)
                    {
                        content3 = "This advice is based upon the information currently available.  You may wish to contact us again around the age of " +
                            reviewAtAge.ToString() + " so we can update our advice.";
                    }
                    if (tissueType != "")
                    {
                        content4 = "Further Investigations: "; //all these strings have been hard-coded in the Access front-end!
                        if (tissueType == "Blood")
                        {
                            content4 = content4 + "It may also be useful to store a sample of blood from one of your relatives who has had cancer.  This may enable genetic testing to be pursued in the future if there are further developments in knowledge or technology. If you are interested in discussing this further, please contact the department to discuss this with the genetic counsellor.";
                        }
                        else if (tissueType == "Tissue")
                        {
                            content4 = content4 + "We may be able to do further tests on samples of tumour tissue which may have been stored from your relatives who have had cancer. This could help to clarify whether the cancers in the family may be due to a family predisposition. In turn, we may then be able to give more accurate screening advice for you and your relatives. If you are interested in discussing this further, please contact the department to discuss this with the genetic counsellor.";
                        }
                        else if (tissueType == "Blood & Tissue")
                        {
                            content4 = content4 + "We may be able to do further tests on samples of tumour tissue which may have been stored from your relatives who have had cancer. This could help to clarify whether the cancers in the family may be due to a family predisposition. In turn, we may then be able to give more accurate screening advice for you and your relatives. It may also be useful to store a sample of blood from one of your relatives who has had cancer.  This may enable genetic testing to be pursued in the future if there are further developments in knowledge or technology. If you are interested in discussing this further, please contact the department to discuss this with the genetic counsellor.";
                        }
                    }
                    content5 = _lvm.documentsContent.Para3 + Environment.NewLine + Environment.NewLine + _lvm.documentsContent.Para4;
                    if (isResearchStudy.GetValueOrDefault())
                    {
                        content6 = _lvm.documentsContent.Para9;
                    }
                    Paragraph letterContent1 = section.AddParagraph(content1);
                    spacer = section.AddParagraph();
                    if (content2 != null && content2 != "")
                    {
                        Paragraph letterContent2 = section.AddParagraph(content2);
                        spacer = section.AddParagraph();
                    }
                    if (content3 != null && content3 != "")
                    {
                        Paragraph letterContent3 = section.AddParagraph(content3);
                        spacer = section.AddParagraph();
                    }
                    if (content4 != null && content4 != "")
                    {
                        Paragraph letterContent4 = section.AddParagraph(content4);
                        spacer = section.AddParagraph();
                    }
                    Paragraph letterContent5 = section.AddParagraph(content5);
                    spacer = section.AddParagraph();
                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                    ccs[0] = referrerName;
                    ccs[1] = gpName;
                }

                //O1c
                if (docCode == "O1C")
                {
                    content1 = _lvm.documentsContent.Para1 + Environment.NewLine + Environment.NewLine + _lvm.documentsContent.Para2 + Environment.NewLine +
                        Environment.NewLine + _lvm.documentsContent.Para3 + Environment.NewLine + Environment.NewLine + _lvm.documentsContent.Para4;
                    if (reviewAtAge > 0)
                    {
                        content2 = "This advice is based upon the information currently available.  You may wish to contact us again around the age of " +
                            reviewAtAge.ToString() + " so we can update our advice.";
                    }
                    if (isResearchStudy.GetValueOrDefault())
                    {
                        content3 = _lvm.documentsContent.Para9;
                    }

                    Paragraph letterContent1 = section.AddParagraph(content1);
                    spacer = section.AddParagraph();
                    if (content2 != null && content2 != "")
                    {
                        Paragraph letterContent2 = section.AddParagraph(content2);
                        spacer = section.AddParagraph();
                    }
                    if (content3 != null && content3 != "")
                    {
                        Paragraph letterContent3 = section.AddParagraph(content3);
                        spacer = section.AddParagraph();
                    }
                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                    ccs[0] = referrerName;
                    ccs[1] = gpName;
                }

                //O2
                if (docCode == "O2")
                {
                    content1 = _lvm.documentsContent.Para1;
                    content2 = _lvm.documentsContent.Para2 + " " + additionalText;
                    if (isScreeningRels.GetValueOrDefault())
                    {
                        content3 = _lvm.documentsContent.Para8;
                    }
                    if (reviewAtAge > 0)
                    {
                        content4 = "This advice is based upon the information currently available.  You may wish to contact us again around the age of "
                            + reviewAtAge.ToString() + " so we can update our advice.";
                    }
                    if (tissueType != "")
                    {
                        content5 = "Further Investigations: ";
                        if (tissueType == "Blood")
                        {
                            content5 = content5 + "It may also be useful to store a sample of blood from one of your relatives who has had cancer.  This may enable genetic testing to be pursued in the future if there are further developments in knowledge or technology. If you are interested in discussing this further, please contact the department to discuss this with the genetic counsellor.";
                        }
                        else if (tissueType == "Tissue")
                        {
                            content5 = content5 + "We may be able to do further tests on samples of tumour tissue which may have been stored from your relatives who have had cancer. This could help to clarify whether the cancers in the family may be due to a family predisposition. In turn, we may then be able to give more accurate screening advice for you and your relatives. If you are interested in discussing this further, please contact the department to discuss this with the genetic counsellor.";
                        }
                        else if (tissueType == "Blood & Tissue")
                        {
                            content5 = content5 + "We may be able to do further tests on samples of tumour tissue which may have been stored from your relatives who have had cancer. This could help to clarify whether the cancers in the family may be due to a family predisposition. In turn, we may then be able to give more accurate screening advice for you and your relatives. It may also be useful to store a sample of blood from one of your relatives who has had cancer.  This may enable genetic testing to be pursued in the future if there are further developments in knowledge or technology. If you are interested in discussing this further, please contact the department to discuss this with the genetic counsellor.";
                        }
                    }

                    Paragraph letterContent1 = section.AddParagraph(content1);
                    spacer = section.AddParagraph();
                    Paragraph letterContent2 = section.AddParagraph(content2);
                    spacer = section.AddParagraph();
                    if (content3 != "")
                    {
                        Paragraph letterContent3 = section.AddParagraph(content3);
                        spacer = section.AddParagraph();
                    }
                    if (content4 != "")
                    {
                        Paragraph letterContent4 = section.AddParagraph(content4);
                        spacer = section.AddParagraph();
                    }
                    if (content5 != "")
                    {
                        Paragraph letterContent5 = section.AddParagraph(content5);
                        spacer = section.AddParagraph();
                    }
                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                    ccs[0] = referrerName;
                    ccs[1] = gpName;
                }

                //O2a
                if (docCode == "O2a")
                {
                    content1 = _lvm.documentsContent.Para1 + Environment.NewLine + Environment.NewLine + _lvm.documentsContent.Para2 + " " + additionalText;
                    if (tissueType != "")
                    {
                        content2 = "Further Investigations: ";
                        if (tissueType == "Blood")
                        {
                            content2 = content2 + "It may also be useful to store a sample of blood from one of your relatives who has had cancer.  This may enable genetic testing to be pursued in the future if there are further developments in knowledge or technology. If you are interested in discussing this further, please contact the department to discuss this with the genetic counsellor.";
                        }
                        else if (tissueType == "Tissue")
                        {
                            content2 = content2 + "We may be able to do further tests on samples of tumour tissue which may have been stored from your relatives who have had cancer. This could help to clarify whether the cancers in the family may be due to a family predisposition. In turn, we may then be able to give more accurate screening advice for you and your relatives. If you are interested in discussing this further, please contact the department to discuss this with the genetic counsellor.";
                        }
                        else if (tissueType == "Blood & Tissue")
                        {
                            content2 = content2 + "We may be able to do further tests on samples of tumour tissue which may have been stored from your relatives who have had cancer. This could help to clarify whether the cancers in the family may be due to a family predisposition. In turn, we may then be able to give more accurate screening advice for you and your relatives. It may also be useful to store a sample of blood from one of your relatives who has had cancer.  This may enable genetic testing to be pursued in the future if there are further developments in knowledge or technology. If you are interested in discussing this further, please contact the department to discuss this with the genetic counsellor.";
                        }
                    }
                    content3 = _lvm.documentsContent.Para3 + Environment.NewLine + Environment.NewLine + _lvm.documentsContent.Para4;
                    if (isResearchStudy.GetValueOrDefault())
                    {
                        content4 = _lvm.documentsContent.Para9;
                    }
                    if (reviewAtAge > 0)
                    {
                        content5 = "This advice is based upon the information currently available.  You may wish to contact us again around the age of "
                            + reviewAtAge.ToString() + " so we can update our advice.";
                    }

                    Paragraph letterContent1 = section.AddParagraph(content1);
                    spacer = section.AddParagraph();

                    if (content2 != "")
                    {
                        Paragraph letterContent2 = section.AddParagraph(content2);
                        spacer = section.AddParagraph();
                    }

                    Paragraph letterContent3 = section.AddParagraph(content3);
                    spacer = section.AddParagraph();

                    if (content4 != "")
                    {
                        Paragraph letterContent4 = section.AddParagraph(content4);
                        spacer = section.AddParagraph();
                    }
                    if (content5 != "")
                    {
                        Paragraph letterContent5 = section.AddParagraph(content5);
                        spacer = section.AddParagraph();
                    }
                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                    ccs[0] = referrerName;
                    ccs[1] = gpName;
                }

                //O2d
                if (docCode == "O2d")
                {
                    content1 = _lvm.documentsContent.Para1;
                    content2 = _lvm.documentsContent.Para2;
                    content3 = additionalText;
                    Paragraph letterContent1 = section.AddParagraph(content1);                    
                    spacer = section.AddParagraph();
                    Paragraph letterContent2 = section.AddParagraph(content2);                    
                    spacer = section.AddParagraph();
                    Paragraph letterContent3 = section.AddParagraph(content3);                    
                    spacer = section.AddParagraph();
                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                    ccs[0] = referrerName;
                    ccs[1] = gpName;
                }

                //O3
                if (docCode == "O3")
                {
                    List<Risk> _riskList = new List<Risk>();
                    RiskData _rData = new RiskData(_clinContext);
                    Surveillance _surv = new Surveillance();
                    SurveillanceData _survData = new SurveillanceData(_clinContext);
                    _riskList = _rData.GetRiskListByRefID(refID);

                    content1 = _lvm.documentsContent.Para1;

                    foreach (var item in _riskList)
                    {
                        _surv = _survData.GetSurvDetails(item.RiskID);
                        content2 = item.SurvSite + " surveillance " + " by " + item.SurvType + " " + item.SurvFreq + " from the age of " + item.SurvStartAge.ToString(); //TODO - get this to display properly
                        if (item.SurvStopAge != null)
                        {
                            content2 = content2 + " to " + item.SurvStopAge.ToString();
                        }
                    }
                    content3 = _lvm.documentsContent.Para3;

                    content4 = _lvm.documentsContent.Para4;

                    Paragraph letterContent1 = section.AddParagraph(content1);                    
                    spacer = section.AddParagraph();
                    Paragraph letterContent2 = section.AddParagraph(content2);                    
                    spacer = section.AddParagraph();
                    Paragraph letterContent4 = section.AddParagraph(content4);                    
                    spacer = section.AddParagraph();
                    Paragraph letterContent5 = section.AddParagraph(content5);                    
                    spacer = section.AddParagraph();
                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                    ccs[0] = referrerName;
                    ccs[1] = gpName;
                }

                //O3a
                if (docCode == "O3a")
                {
                    content1 = _lvm.documentsContent.Para1;
                    content2 = _lvm.documentsContent.Para2;
                    content3 = _lvm.documentsContent.Para3;
                    content4 = _lvm.documentsContent.Para9;

                    Paragraph letterContent1 = section.AddParagraph(content1);                    
                    spacer = section.AddParagraph();
                    Paragraph letterContent2 = section.AddParagraph(content2);                    
                    spacer = section.AddParagraph();
                    Paragraph letterContent3 = section.AddParagraph(content3);                    
                    spacer = section.AddParagraph();
                    Paragraph letterContent4 = section.AddParagraph(content4);                    
                    spacer = section.AddParagraph();
                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                    ccs[0] = referrerName;
                    ccs[1] = gpName;
                }

                //O4
                if (docCode == "O4")
                {
                    List<Risk> _riskList = new List<Risk>();
                    RiskData _rData = new RiskData(_clinContext);
                    Surveillance _surv = new Surveillance();
                    SurveillanceData _survData = new SurveillanceData(_clinContext);
                    _riskList = _rData.GetRiskListByRefID(refID);

                    content1 = _lvm.documentsContent.Para1;
                    content2 = _lvm.documentsContent.Para2;
                    content3 = _lvm.documentsContent.Para3;
                    content4 = _lvm.documentsContent.Para4;

                    Paragraph letterContent1 = section.AddParagraph(content1);                    
                    spacer = section.AddParagraph();
                    foreach (var item in _riskList)
                    {
                        string riskText = item.SurvSite + " cancer risk category:";

                        MigraDoc.DocumentObjectModel.Tables.Table riskTable = section.AddTable();
                        MigraDoc.DocumentObjectModel.Tables.Column riskCol1 = riskTable.AddColumn();
                        riskCol1.Width = 180;
                        MigraDoc.DocumentObjectModel.Tables.Column riskCol2 = riskTable.AddColumn();
                        MigraDoc.DocumentObjectModel.Tables.Column riskCol3 = riskTable.AddColumn();
                        riskCol3.Width = 150;
                        MigraDoc.DocumentObjectModel.Tables.Column riskCol4 = riskTable.AddColumn();
                        MigraDoc.DocumentObjectModel.Tables.Row riskRow1 = riskTable.AddRow();
                        MigraDoc.DocumentObjectModel.Tables.Row riskRow2 = riskTable.AddRow();
                        MigraDoc.DocumentObjectModel.Tables.Row riskRow3 = riskTable.AddRow();
                        riskRow1.Cells[0].AddParagraph().AddFormattedText(riskText, TextFormat.Bold);
                        riskRow1.Cells[1].AddParagraph().AddFormattedText(item.RiskName, TextFormat.Bold).Color = Colors.Red;
                        riskRow1.Cells[2].AddParagraph().AddFormattedText("Lifetime risk (%):", TextFormat.Bold);
                        riskRow1.Cells[3].AddParagraph().AddFormattedText(item.LifetimeRiskPercentage.ToString(), TextFormat.Bold).Color = Colors.Red;
                        riskRow2.Cells[0].AddParagraph().AddFormattedText("10 year risk age 30-40 (%):", TextFormat.Bold);
                        riskRow2.Cells[1].AddParagraph().AddFormattedText(item.R30_40.ToString(), TextFormat.Bold).Color = Colors.Red;
                        riskRow2.Cells[2].AddParagraph().AddFormattedText("10 year risk age 50-60 (%):", TextFormat.Bold);
                        riskRow2.Cells[3].AddParagraph().AddFormattedText(item.R50_60.ToString(), TextFormat.Bold).Color = Colors.Red;
                        riskRow3.Cells[0].AddParagraph().AddFormattedText("10 year risk age 40-50 (%):", TextFormat.Bold);
                        riskRow3.Cells[1].AddParagraph().AddFormattedText(item.R40_50.ToString(), TextFormat.Bold).Color = Colors.Red;

                    }
                    spacer = section.AddParagraph();
                    Paragraph letterContent2 = section.AddParagraph(content2);                    
                    spacer = section.AddParagraph();
                    foreach (var item in _riskList)
                    {
                        _surv = _survData.GetSurvDetails(item.RiskID);
                        string contentSurv = item.SurvSite + " surveillance ";
                        if (item.SurvType != null)
                        {
                            contentSurv += " by " + item.SurvType; 
                        }
                        contentSurv += " - " + item.SurvFreq + " from the age of " + item.SurvStartAge.ToString(); //TODO - get this to display properly

                        if (item.SurvStopAge != null)
                        {
                            contentSurv = contentSurv + " to " + item.SurvStopAge.ToString();
                        }
                        Paragraph letterContent3 = section.AddParagraph();
                        letterContent3.AddFormattedText(contentSurv, TextFormat.Bold);
                    }
                    spacer = section.AddParagraph();
                    Paragraph letterContent4 = section.AddParagraph(content3);                    
                    spacer = section.AddParagraph();
                    Paragraph letterContent5 = section.AddParagraph(content4);                    
                    spacer = section.AddParagraph();
                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                    ccs[0] = referrerName;
                    ccs[1] = gpName;
                }

                //O4am
                if (docCode == "O4am")
                {
                    List<Risk> _riskList = new List<Risk>();
                    RiskData _rData = new RiskData(_clinContext);
                    Surveillance _surv = new Surveillance();
                    SurveillanceData _survData = new SurveillanceData(_clinContext);
                    _riskList = _rData.GetRiskListByRefID(refID);

                    pageCount = 2;

                    Paragraph contentRe = section.AddParagraph();
                    contentRe.AddFormattedText("Re: " + patName + " - " + patDOB.ToString("dd/MM/yyyy") + " - " + _lvm.patient.SOCIAL_SECURITY, TextFormat.Bold);
                    spacer = section.AddParagraph();

                    content1 = _lvm.documentsContent.Para1;
                    Paragraph letterContent1 = section.AddParagraph(content1);
                    spacer = section.AddParagraph();
                    foreach (var item in _riskList)
                    {
                        string riskText = item.SurvSite + " cancer risk category:";

                        MigraDoc.DocumentObjectModel.Tables.Table riskTable = section.AddTable();
                        MigraDoc.DocumentObjectModel.Tables.Column riskCol1 = riskTable.AddColumn();
                        riskCol1.Width = 180;
                        MigraDoc.DocumentObjectModel.Tables.Column riskCol2 = riskTable.AddColumn();
                        MigraDoc.DocumentObjectModel.Tables.Column riskCol3 = riskTable.AddColumn();
                        riskCol3.Width = 150;
                        MigraDoc.DocumentObjectModel.Tables.Column riskCol4 = riskTable.AddColumn();
                        MigraDoc.DocumentObjectModel.Tables.Row riskRow1 = riskTable.AddRow();
                        MigraDoc.DocumentObjectModel.Tables.Row riskRow2 = riskTable.AddRow();
                        MigraDoc.DocumentObjectModel.Tables.Row riskRow3 = riskTable.AddRow();
                        riskRow1.Cells[0].AddParagraph().AddFormattedText(riskText, TextFormat.Bold);
                        riskRow1.Cells[1].AddParagraph().AddFormattedText(item.RiskName, TextFormat.Bold).Color = Colors.Red;
                        riskRow1.Cells[2].AddParagraph().AddFormattedText("Lifetime risk (%):", TextFormat.Bold);
                        riskRow1.Cells[3].AddParagraph().AddFormattedText(item.LifetimeRiskPercentage.ToString(), TextFormat.Bold).Color = Colors.Red;
                        riskRow2.Cells[0].AddParagraph().AddFormattedText("10 year risk age 30-40 (%):", TextFormat.Bold);
                        riskRow2.Cells[1].AddParagraph().AddFormattedText(item.R30_40.ToString(), TextFormat.Bold).Color = Colors.Red;
                        riskRow2.Cells[2].AddParagraph().AddFormattedText("10 year risk age 50-60 (%):", TextFormat.Bold);
                        riskRow2.Cells[3].AddParagraph().AddFormattedText(item.R50_60.ToString(), TextFormat.Bold).Color = Colors.Red;
                        riskRow3.Cells[0].AddParagraph().AddFormattedText("10 year risk age 40-50 (%):", TextFormat.Bold);
                        riskRow3.Cells[1].AddParagraph().AddFormattedText(item.R40_50.ToString(), TextFormat.Bold).Color = Colors.Red;

                    }
                    spacer = section.AddParagraph();
                    content2 = _lvm.documentsContent.Para2;
                    Paragraph letterContent2 = section.AddParagraph(content2);
                    spacer = section.AddParagraph();
                    foreach (var item in _riskList)
                    {
                        _surv = _survData.GetSurvDetails(item.RiskID);
                        string contentSurv = item.SurvSite + " surveillance ";
                        if (item.SurvType != null)
                        {
                            contentSurv += " by " + item.SurvType;
                        }
                        
                        contentSurv += " - " + item.SurvFreq + " from the age of " + item.SurvStartAge.ToString();

                        if (item.SurvStopAge != null)
                        {
                            contentSurv = contentSurv + " to " + item.SurvStopAge.ToString();
                        }
                        Paragraph letterContent3 = section.AddParagraph();
                        letterContent3.AddFormattedText(contentSurv, TextFormat.Bold);
                    }
                    spacer = section.AddParagraph();
                    content4 = _lvm.documentsContent.Para3;
                    Paragraph letterContent4 = section.AddParagraph(content4);
                    spacer = section.AddParagraph();
                }

                //Reject letter
                if (docCode == "RejectCMA")
                {
                    content1 = _lvm.documentsContent.Para1 + Environment.NewLine + Environment.NewLine + _lvm.documentsContent.Para2;

                    Paragraph letterContent1 = section.AddParagraph(content1);
                    
                    spacer = section.AddParagraph();
                    if (content2 != null)
                    {
                        Paragraph letterContent2 = section.AddParagraph();
                        letterContent2.AddFormattedText(content2, TextFormat.Bold);
                        
                        spacer = section.AddParagraph();
                    }
                    Paragraph letterContent3 = section.AddParagraph(content3);
                    
                    spacer = section.AddParagraph();
                    Paragraph letterContent4 = section.AddParagraph(content4);
                    
                    spacer = section.AddParagraph();
                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                    ccs[0] = referrerName;
                }

                //MR01
                if (docCode == "MR01")
                {                    
                    Paragraph letterContent1 = section.AddParagraph(_lvm.documentsContent.Para1);
                    spacer = section.AddParagraph();
                    Paragraph letterContent2 = section.AddParagraph(_lvm.documentsContent.Para8);
                    spacer = section.AddParagraph();
                    Paragraph letterContent3 = section.AddParagraph();
                    letterContent3.AddFormattedText(_lvm.documentsContent.Para3, TextFormat.Bold);
                    spacer = section.AddParagraph();
                    Paragraph letterContent4 = section.AddParagraph(_lvm.documentsContent.Para4);
                    spacer = section.AddParagraph();
                    Paragraph letterContent5 = section.AddParagraph(_lvm.documentsContent.Para5);

                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                }


                //MR03
                if (docCode == "MR03")
                {
                    string ptDOB = "Date of birth: " + patDOB.ToString("dd/MM/yyyy") + System.Environment.NewLine + System.Environment.NewLine;

                    if (_lvm.patient.DECEASED == -1)
                    {
                        ptDOB = ptDOB + "Date of death: " + _lvm.patient.DECEASED_DATE.Value.ToString("dd/MM/yyyy");
                    }

                    ptDOB = ptDOB + "NHS Number: " + _lvm.patient.SOCIAL_SECURITY;
                    spacer = section.AddParagraph();
                    MigraDoc.DocumentObjectModel.Tables.Table table1 = section.AddTable();
                    MigraDoc.DocumentObjectModel.Tables.Column contentPtName = table.AddColumn();
                    contentPtName.Format.Alignment = ParagraphAlignment.Left;
                    MigraDoc.DocumentObjectModel.Tables.Column contentPtDOB = table.AddColumn();
                    contentPtDOB.Format.Alignment = ParagraphAlignment.Left;
                    table.Rows.Height = 50;
                    table.Columns.Width = 250;
                    MigraDoc.DocumentObjectModel.Tables.Row row1_1 = table.AddRow();
                    row1_1[0].AddParagraph("Re: " + patName + System.Environment.NewLine + patAddress);
                    row1_1[1].AddParagraph(ptDOB);
                    row1_1.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Top;
                    row1_1.Format.Font.Bold = true;
                    spacer = section.AddParagraph();
                    Paragraph letterContent1 = section.AddParagraph(_lvm.documentsContent.Para5);
                    
                    spacer = section.AddParagraph();

                    Paragraph letterContent2 = section.AddParagraph(_lvm.documentsContent.Para6 + $" {siteText.ToLower()} " + _lvm.documentsContent.Para7);
                    
                    spacer = section.AddParagraph();

                    Paragraph letterContent3 = section.AddParagraph(_lvm.documentsContent.Para3);
                    
                    spacer = section.AddParagraph();

                    Paragraph letterContent4 = section.AddParagraph(_lvm.documentsContent.Para4);
                    

                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                }

                //DT01
                if (docCode == "DT01")
                {
                    Paragraph letterContentPatName = section.AddParagraph("Re: " + patName);
                    Paragraph letterContentPatDOB = section.AddParagraph("Date of birth: " + patDOB.ToString("dd/MM/yyyy"));
                    spacer = section.AddParagraph();

                    if (relID == 0)
                    {
                        content1 = _lvm.documentsContent.Para1;
                    }
                    else
                    {
                        content1 = _lvm.documentsContent.Para5;
                    }
                    content2 = _lvm.documentsContent.Para2 + $" {siteText} " + _lvm.documentsContent.Para7;
                    content3 = _lvm.documentsContent.Para3;

                    Paragraph letterContent1 = section.AddParagraph(content1);
                    
                    spacer = section.AddParagraph();
                    Paragraph letterContent2 = section.AddParagraph(content2);
                    
                    spacer = section.AddParagraph();
                    Paragraph letterContent3 = section.AddParagraph(content3);
                    
                    spacer = section.AddParagraph();

                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;

                    enclosures = "Two copies of consent form (Letter code CF02) Letter to give to your GP or hospital (Letter code DT03) " +
                        "Blood sampling kit (containing the relevant tubes, form and packaging) Pre-paid envelope";
                }

                //DT03
                if (docCode == "DT03")
                {
                    salutation = "Colleague";
                    Paragraph letterContentPatName = section.AddParagraph("Re: " + patName);
                    spacer = section.AddParagraph();
                    Paragraph letterContentPatDOB = section.AddParagraph("Date of birth: " + patDOB.ToString("dd/MM/yyyy"));
                    spacer = section.AddParagraph();
                    Paragraph letterContentPatAddress = section.AddParagraph(patAddress);
                    spacer = section.AddParagraph();

                    content1 = _lvm.documentsContent.Para1 + " " + patName + " " + _lvm.documentsContent.Para2;
                    content2 = _lvm.documentsContent.Para3;
                    content3 = _lvm.documentsContent.Para4;
                    content4 = _lvm.documentsContent.Para5;
                    Paragraph letterContent1 = section.AddParagraph(content1);                    
                    spacer = section.AddParagraph();
                    Paragraph letterContent2 = section.AddParagraph(content2);                    
                    spacer = section.AddParagraph();
                    Paragraph letterContent3 = section.AddParagraph(content3);                    
                    spacer = section.AddParagraph();
                    Paragraph letterContent4 = section.AddParagraph(content4);                    
                    spacer = section.AddParagraph();

                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                }

                //DT11
                if (docCode == "DT11")
                {
                    Paragraph letterContentPatName = section.AddParagraph("Re: " + patName);
                    spacer = section.AddParagraph();

                    ExternalClinician clin = _externalClinicianData.GetClinicianDetails(clinicianCode);

                    content1 = _lvm.documentsContent.Para1;
                    Paragraph letterContent1 = section.AddParagraph(content1);                    
                    spacer = section.AddParagraph();
                    content2 = _lvm.documentsContent.Para2 + " " + siteText + " " + _lvm.documentsContent.Para3;
                    Paragraph letterContent2 = section.AddParagraph(content2);                    
                    spacer = section.AddParagraph();
                    content3 = clin.TITLE + " " + clin.FIRST_NAME + clin.NAME + _externalClinicianData.GetCCDetails(clin);
                    Paragraph letterContent3 = section.AddParagraph(content3);                    
                    spacer = section.AddParagraph();
                    content4 = _lvm.documentsContent.Para4;
                    Paragraph letterContent4 = section.AddParagraph(content4);                    
                    spacer = section.AddParagraph();
                    content5 = _lvm.documentsContent.Para8;
                    Paragraph letterContent5 = section.AddParagraph(content5);                    
                    spacer = section.AddParagraph();

                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                    enclosures = "copy of completed consent form (Letter code CF04)";
                    pageCount += 1; //because it's impossible to force it to go to the next page otherwise!
                    ccs[0] = clin.TITLE + " " + clin.FIRST_NAME + clin.NAME;
                }

                //DT11e
                if (docCode == "DT11e")
                {
                    string recipient = "Dr Raji Ganesan" + System.Environment.NewLine +
                        "Cellular Pathology" + System.Environment.NewLine +
                        "Birmingham Women’s Hospital" + System.Environment.NewLine +
                        "Mindelsohn Way" + System.Environment.NewLine +
                        "Birmingham" + System.Environment.NewLine +
                        "B15 2TG"; //because of course it's hard-coded in CGU_DB

                    Paragraph letterContentPatName = section.AddParagraph("Re: " + patName);
                    spacer = section.AddParagraph();
                    content1 = _lvm.documentsContent.Para1;
                    Paragraph letterContent1 = section.AddParagraph(content1);                    
                    spacer = section.AddParagraph();
                    content2 = _lvm.documentsContent.Para2 + " " + siteText + " " + _lvm.documentsContent.Para3;
                    Paragraph letterContent2 = section.AddParagraph(content2);                    
                    spacer = section.AddParagraph();
                    Paragraph letterContentRecipient = section.AddParagraph();
                    letterContentRecipient.AddFormattedText(recipient, TextFormat.Bold);
                    spacer = section.AddParagraph();
                    content3 = _lvm.documentsContent.Para4;
                    Paragraph letterContent3 = section.AddParagraph(content3);                    
                    spacer = section.AddParagraph();
                    content4 = _lvm.documentsContent.Para8;
                    Paragraph letterContent4 = section.AddParagraph(content4);                    
                    spacer = section.AddParagraph();

                    enclosures = "copy of completed consent form (Letter code CF04)";
                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                    ccs[0] = recipient;
                }

                //DT13
                if (docCode == "DT13")
                {
                    Paragraph letterContentPatName = section.AddParagraph();
                    letterContentPatName.AddFormattedText("Re: " + patName + System.Environment.NewLine + patAddress, TextFormat.Bold);
                    spacer = section.AddParagraph();
                    Paragraph letterContentPatDOB = section.AddParagraph();
                    letterContentPatDOB.AddFormattedText("Date of birth: " + patDOB.ToString("dd/MM/yyyy"), TextFormat.Bold);
                    spacer = section.AddParagraph();
                    Paragraph letterContentCancerSite = section.AddParagraph();
                    letterContentCancerSite.AddFormattedText("Cancer Site: " + siteText, TextFormat.Bold);
                    letterContentCancerSite.Format.Alignment = ParagraphAlignment.Right;
                    spacer = section.AddParagraph();
                    content1 = _lvm.documentsContent.Para1;
                    Paragraph letterContent1 = section.AddParagraph(content1);                    
                    spacer = section.AddParagraph();
                    content2 = _lvm.documentsContent.Para2;
                    Paragraph letterContent2 = section.AddParagraph(content2);                    
                    spacer = section.AddParagraph();
                    content3 = _lvm.documentsContent.Para3;
                    Paragraph letterContent3 = section.AddParagraph(content3);                    
                    spacer = section.AddParagraph();
                    content4 = _lvm.documentsContent.Para4;
                    Paragraph letterContent4 = section.AddParagraph(content4);                    
                    spacer = section.AddParagraph();
                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                }

                //DT15
                if (docCode == "DT15")
                {
                    pageCount = 2;
                    string germline = freeText1;
                    string somatic = freeText2;
                    string furtherDetails = additionalText;

                    Paragraph letterContentPatName = section.AddParagraph();
                    letterContentPatName.AddFormattedText("Re: " + patName + "CGUbo: " + _lvm.patient.CGU_No, TextFormat.Bold);
                    
                    Paragraph letterContentPatDOB = section.AddParagraph();
                    letterContentPatDOB.AddFormattedText("Date of birth: " + patDOB.ToString("dd/MM/yyyy") + "NHS number: " + _lvm.patient.SOCIAL_SECURITY, TextFormat.Bold);
                    spacer = section.AddParagraph();
                    string titleText = "Request for formalin-fixed paraffin embedded (FFPE) tissue to enable genetic tissue";
                    Paragraph letterTitle = section.AddParagraph();
                    letterTitle.AddFormattedText(titleText, TextFormat.Bold);
                    spacer = section.AddParagraph();
                    content1 = _lvm.documentsContent.Para1;
                    Paragraph letterContent1 = section.AddParagraph(content1);
                    spacer = section.AddParagraph();
                    content2 = _lvm.documentsContent.Para2 + " " + siteText + " " + _lvm.documentsContent.Para3;
                    Paragraph letterContent2 = section.AddParagraph();
                    letterContent2.AddFormattedText(content2);
                    letterContent2.Format.LeftIndent = 5;
                    spacer = section.AddParagraph();
                    MigraDoc.DocumentObjectModel.Tables.Table histoTable = section.AddTable();
                    MigraDoc.DocumentObjectModel.Tables.Column col1 = histoTable.AddColumn();
                    col1.Width = 250;                    
                    MigraDoc.DocumentObjectModel.Tables.Column col2 = histoTable.AddColumn();
                    col2.Width = 250;
                    MigraDoc.DocumentObjectModel.Tables.Row hRow1 = histoTable.AddRow();
                    MigraDoc.DocumentObjectModel.Tables.Row hRow2 = histoTable.AddRow();
                    MigraDoc.DocumentObjectModel.Tables.Row hRow3 = histoTable.AddRow();
                    MigraDoc.DocumentObjectModel.Tables.Row hRow4 = histoTable.AddRow();
                    MigraDoc.DocumentObjectModel.Tables.Row hRow5 = histoTable.AddRow();
                    MigraDoc.DocumentObjectModel.Tables.Row hRow6 = histoTable.AddRow();
                    MigraDoc.DocumentObjectModel.Tables.Row hRow7 = histoTable.AddRow();
                    histoTable.Rows.Height = 20;

                    histoTable.SetEdge(0, 0, histoTable.Columns.Count, histoTable.Rows.Count, Edge.Box, BorderStyle.Single, 1, Colors.Black);
                    
                    hRow1.Borders.Bottom.Width = 0.5;
                    hRow4.Borders.Bottom.Width = 0.5;

                    content3 = _lvm.documentsContent.Para7;
                    hRow1.Cells[0].MergeRight = 1;
                    hRow1.Cells[0].AddParagraph().AddFormattedText(content3, TextFormat.Bold);
                    hRow2.Cells[0].AddParagraph().AddFormattedText("Slides containing tumour tissue", TextFormat.Bold);
                    hRow3.Cells[0].AddParagraph("Slide reference(s):");
                    hRow4.Cells[0].AddParagraph("Cellularity:");
                    hRow4.Cells[1].AddParagraph("Tumour content:");
                    hRow5.Cells[0].AddParagraph().AddFormattedText("Slides containing 'normal' tissue", TextFormat.Bold);
                    hRow6.Cells[0].AddParagraph("Slide reference(s):");
                    hRow7.Cells[0].AddParagraph("Cellularity:");
                    hRow7.Cells[1].AddParagraph("Tumour content:" + Environment.NewLine + "(if an area of solely 'normal' tissue is not available)");
                    
                    spacer = section.AddParagraph();
                    content4 = _lvm.documentsContent.Para10;
                    Paragraph letterContent3 = section.AddParagraph(content4);
                    spacer = section.AddParagraph();
                    Paragraph letterContent4 = section.AddParagraph();
                    letterContent4.AddFormattedText("Please ensure both sides of this form are included with the patient samples.", TextFormat.Bold);
                    spacer = section.AddParagraph();
                    Paragraph letterContent5 = section.AddParagraph();
                    letterContent5.AddFormattedText("Notes to Histopathology Laboratory:", TextFormat.Bold);
                    spacer = section.AddParagraph();
                    content5 = _lvm.documentsContent.Para4;
                    Paragraph letterContent6 = section.AddParagraph(content5);
                    spacer = section.AddParagraph();
                    content6 = _lvm.documentsContent.Para5;
                    Paragraph letterContent7 = section.AddParagraph(content6);
                    letterContent7.Format.LeftIndent = 5;
                    spacer = section.AddParagraph();
                    string content7 = _lvm.documentsContent.Para6;
                    Paragraph letterContent8 = section.AddParagraph(content7);
                    spacer = section.AddParagraph();

                    MigraDoc.DocumentObjectModel.Tables.Table histoTable2 = section.AddTable();
                    MigraDoc.DocumentObjectModel.Tables.Column col2_1 = histoTable2.AddColumn();
                    col2_1.Width = 500;                                       
                    MigraDoc.DocumentObjectModel.Tables.Row hRow2_1 = histoTable2.AddRow();
                    MigraDoc.DocumentObjectModel.Tables.Row hRow2_2 = histoTable2.AddRow();
                    MigraDoc.DocumentObjectModel.Tables.Row hRow2_3 = histoTable2.AddRow();
                    MigraDoc.DocumentObjectModel.Tables.Row hRow2_4 = histoTable2.AddRow();
                    MigraDoc.DocumentObjectModel.Tables.Row hRow2_5 = histoTable2.AddRow();
                    MigraDoc.DocumentObjectModel.Tables.Row hRow2_6 = histoTable2.AddRow();
                    MigraDoc.DocumentObjectModel.Tables.Row hRow2_7 = histoTable2.AddRow();
                    MigraDoc.DocumentObjectModel.Tables.Row hRow2_8 = histoTable2.AddRow();
                    MigraDoc.DocumentObjectModel.Tables.Row hRow2_9 = histoTable2.AddRow();
                    MigraDoc.DocumentObjectModel.Tables.Row hRow2_10 = histoTable2.AddRow();
                    MigraDoc.DocumentObjectModel.Tables.Row hRow2_11 = histoTable2.AddRow();
                    MigraDoc.DocumentObjectModel.Tables.Row hRow2_12 = histoTable2.AddRow();
                    histoTable2.Rows.Height = 20;

                    histoTable2.SetEdge(0, 0, histoTable2.Columns.Count, histoTable2.Rows.Count, Edge.Box, BorderStyle.Single, 1, Colors.Black);
                    hRow2_2.Borders.Bottom.Width = 0.5;
                    hRow2_6.Borders.Bottom.Width = 0.5;
                    hRow2_10.Borders.Bottom.Width = 0.5;
                    hRow2_12.Borders.Bottom.Width = 0.5;

                    hRow2_1.Cells[0].AddParagraph().AddFormattedText("Details of molecular genetic testing required", TextFormat.Bold);
                    hRow2_1.Height = 10;
                    hRow2_2.Cells[0].AddParagraph("Clinician please detail as appropriate to direct testing at the West Midlands Regional Genetics Laboratory");
                    hRow2_3.Cells[0].AddParagraph().AddFormattedText("Germline analysis", TextFormat.Bold);                    
                    hRow2_4.Cells[0].AddParagraph("(where blood or saliva is not available from the affected family member and indirect testing of other family members is not appropriate)");
                    hRow2_4.Height = 30;
                    hRow2_5.Cells[0].AddParagraph("Gene(s) to be analysed for germline variants:");
                    hRow2_6.Cells[0].AddParagraph(germline).Format.LeftIndent = 2;
                    hRow2_7.Cells[0].AddParagraph().AddFormattedText("Somatic analysis", TextFormat.Bold);
                    hRow2_8.Cells[0].AddParagraph("(following a negative germline screen of a gene(s) in which a molecular defect is indicated)");
                    hRow2_9.Cells[0].AddParagraph("Gene(s) to be analysed for somatic variants:");
                    hRow2_10.Cells[0].AddParagraph(somatic).Format.LeftIndent = 2;
                    hRow2_11.Cells[0].AddParagraph().AddFormattedText("Further patient details and cancer history", TextFormat.Bold);                    ;
                    if (furtherDetails != null)
                    {
                        hRow2_12.Cells[0].AddParagraph(furtherDetails).Format.LeftIndent = 2;
                    }
                    hRow2_12.Height = 50;
                    spacer = section.AddParagraph();
                    Paragraph letterContentClinDets = section.AddParagraph();
                    letterContentClinDets.AddFormattedText("Section 5: Clinician details", TextFormat.Bold);
                    spacer = section.AddParagraph();
                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                }

                //PC01
                if (docCode == "PC01")
                {
                    string relDets = "Re: Affected relative's details" + patName + "  Date of birth: " + patDOB.ToString("dd/MM/yyyy");
                    string patDets = "Our patient's details:" + _lvm.patient.Title + " " + _lvm.patient.FIRSTNAME + " " + _lvm.patient.LASTNAME +
                        " Date of birth: " + _lvm.patient.DOB.Value.ToString("dd/MM/yyyy");

                    Paragraph letterContentRelDets = section.AddParagraph(relDets);
                    spacer = section.AddParagraph();
                    Paragraph letterContentPatDets = section.AddParagraph(patDets);
                    spacer = section.AddParagraph();
                    content1 = _lvm.documentsContent.Para2;
                    Paragraph letterContent1 = section.AddParagraph(content1);                    
                    spacer = section.AddParagraph();
                    content2 = _lvm.documentsContent.Para10;
                    Paragraph letterContent2 = section.AddParagraph(content2);                    
                    spacer = section.AddParagraph();
                    Paragraph letterContent3 = section.AddParagraph(content3);                    
                    spacer = section.AddParagraph();
                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                }

                //GR01
                if (docCode == "GR01")
                {                   
                    Paragraph letterContentPt = section.AddParagraph();
                    letterContentPt.AddFormattedText(patName + ", " + patDOB, TextFormat.Bold);
                    spacer = section.AddParagraph();
                    content1 = _lvm.documentsContent.Para1;
                    Paragraph letterContent1 = section.AddParagraph(content1);                    
                    spacer = section.AddParagraph();
                    content2 = _lvm.documentsContent.Para2;
                    Paragraph letterContent2 = section.AddParagraph(content2);                    
                    spacer = section.AddParagraph();
                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                    enclosures = "Consent form (letter code CF01)";
                }


                //GR01
                if (docCode == "GR03")
                {
                    pageCount = 2;
                    Paragraph letterContentPt = section.AddParagraph();
                    letterContentPt.AddFormattedText(patName + ", " + patDOB, TextFormat.Bold);
                    spacer = section.AddParagraph();
                    content1 = _lvm.documentsContent.Para1;
                    Paragraph letterContent1 = section.AddParagraph(content1);
                    spacer = section.AddParagraph();
                    content2 = _lvm.documentsContent.Para2;
                    Paragraph letterContent2 = section.AddParagraph(content2);
                    spacer = section.AddParagraph();
                    content3 = _lvm.documentsContent.Para3;
                    Paragraph letterContent3 = section.AddParagraph(content3);
                    spacer = section.AddParagraph();
                    content4 = _lvm.documentsContent.Para4;
                    Paragraph letterContent4 = section.AddParagraph(content4);
                    spacer = section.AddParagraph();

                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                    enclosures = "Consent form (letter code CF01)";
                }

                //GenMR01
                if(docCode == "GenMR01")
                {
                    Paragraph letterContentPt = section.AddParagraph();
                    letterContentPt.AddFormattedText(patName + ", " + patDOB, TextFormat.Bold);
                    spacer = section.AddParagraph();
                    content1 = _lvm.documentsContent.Para4 + " " + freetext + " " + _lvm.documentsContent.Para5;
                    Paragraph letterContent1 = section.AddParagraph(content1);
                    spacer = section.AddParagraph();
                    content2 = _lvm.documentsContent.Para6;
                    Paragraph letterContent2 = section.AddParagraph(content2);
                    spacer = section.AddParagraph();
                    content3 = _lvm.documentsContent.Para7;
                    Paragraph letterContent3 = section.AddParagraph(content3);
                    spacer = section.AddParagraph();
                    content4 = _lvm.documentsContent.Para8;
                    Paragraph letterContent4 = section.AddParagraph(content4);
                    spacer = section.AddParagraph();
                    content5 = _lvm.documentsContent.Para9;
                    Paragraph letterContent5 = section.AddParagraph(content5);

                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                }

                if (docCode == "VHRProC")
                {
                    content1 = _lvm.documentsContent.Para1;
                    content2 = _lvm.documentsContent.Para2;
                    Paragraph letterContent1 = section.AddParagraph(content1);                    
                    spacer = section.AddParagraph();
                    Paragraph letterContent2 = section.AddParagraph(content2);                    
                    spacer = section.AddParagraph();
                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                    ccs[0] = gpName;
                    ccs[1] = otherName;
                }

                //Clics letters
                if (docCode == "ClicsFHF")
                {
                    content1 = _lvm.documentsContent.Para1;
                    content2 = _lvm.documentsContent.Para2;
                    content3 = _lvm.documentsContent.Para3;
                    content4 = _lvm.documentsContent.Para4;
                    Paragraph letterContent1 = section.AddParagraph(content1);                    
                    spacer = section.AddParagraph();
                    Paragraph letterContent2 = section.AddParagraph(content2);                    
                    spacer = section.AddParagraph();
                    Paragraph letterContent3 = section.AddParagraph(content3);                    
                    spacer = section.AddParagraph();
                    Paragraph letterContent4 = section.AddParagraph(content4);                    
                    spacer = section.AddParagraph();

                    if (qrCodeText != "")
                    {
                        CreateQRImageFile(qrCodeText, user);

                        Paragraph contentQR = section.AddParagraph();
                        MigraDoc.DocumentObjectModel.Shapes.Image imgQRCode = contentQR.AddImage($"wwwroot\\Images\\qrCode-{user}.jpg");
                        imgQRCode.ScaleWidth = new Unit(1.5, UnitType.Point);
                        imgQRCode.ScaleHeight = new Unit(1.5, UnitType.Point);
                        contentQR.Format.Alignment = ParagraphAlignment.Center;
                    }
                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                    //File.Delete($"wwwroot\\Images\\qrCode-{user}.jpg");
                    ccs[0] = referrerName;
                    if (referrerName != gpName)
                    {
                        ccs[1] = gpName;
                    }
                }

                if (docCode == "ClicsRem")
                {
                    content1 = _lvm.documentsContent.Para1;
                    content2 = _lvm.documentsContent.Para2;
                    content3 = _lvm.documentsContent.Para3;
                    Paragraph letterContent1 = section.AddParagraph(content1);                    
                    spacer = section.AddParagraph();
                    Paragraph letterContent2 = section.AddParagraph(content2);                    
                    spacer = section.AddParagraph();
                    Paragraph letterContent3 = section.AddParagraph(content3);                    
                    spacer = section.AddParagraph();
                    ccs[0] = referrerName;

                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                }

                if (docCode == "ClicsStop")
                {
                    content1 = _lvm.documentsContent.Para1;
                    content2 = _lvm.documentsContent.Para2;
                    content3 = _lvm.documentsContent.Para3;
                    Paragraph letterContent1 = section.AddParagraph(content1);                    
                    spacer = section.AddParagraph();
                    Paragraph letterContent2 = section.AddParagraph(content2);                    
                    spacer = section.AddParagraph();
                    Paragraph letterContent3 = section.AddParagraph(content3);                    
                    spacer = section.AddParagraph();
                    ccs[0] = referrerName;
                    if (referrerName != gpName)
                    {
                        ccs[1] = gpName;
                    }

                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                }
                
                if (docCode == "ClicsMR01")
                {
                    content1 = _lvm.documentsContent.Para1;
                    Paragraph letterContent1 = section.AddParagraph(content1);
                    spacer = section.AddParagraph();
                    content2 = _lvm.documentsContent.Para8;
                    Paragraph letterContent2 = section.AddParagraph(content2);
                    spacer = section.AddParagraph();
                    content3 = _lvm.documentsContent.Para3;
                    Paragraph letterContent3 = section.AddParagraph();
                    letterContent3.AddFormattedText(content3, TextFormat.Bold);
                    spacer = section.AddParagraph();
                    content4 = _lvm.documentsContent.Para4;
                    Paragraph letterContent4 = section.AddParagraph(content4);
                    spacer = section.AddParagraph();
                    content5 = _lvm.documentsContent.Para5;
                    Paragraph letterContent5 = section.AddParagraph(content5);
                    spacer = section.AddParagraph();

                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                }
                
                if (docCode == "ClicsMR03")
                {
                    content1 = _lvm.documentsContent.Para5;
                    Paragraph letterContent1 = section.AddParagraph(content1);
                    spacer = section.AddParagraph();
                    content2 = _lvm.documentsContent.Para6;
                    Paragraph letterContent2 = section.AddParagraph(content2);
                    spacer = section.AddParagraph();
                    content3 = _lvm.documentsContent.Para7;
                    Paragraph letterContent3 = section.AddParagraph(content3);
                    spacer = section.AddParagraph();
                    if(additionalText != null && additionalText != "")
                    {
                        Paragraph addText = section.AddParagraph(additionalText);
                        spacer = section.AddParagraph();
                    }
                    content4 = _lvm.documentsContent.Para8;
                    Paragraph letterContent4 = section.AddParagraph(content4);
                    spacer = section.AddParagraph();
                    content5 = _lvm.documentsContent.Para9;
                    Paragraph letterContent5 = section.AddParagraph(content5);
                    spacer = section.AddParagraph();

                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                }

                if (docCode == "ClicsMRR")
                {
                    content1 = _lvm.documentsContent.Para1;
                    Paragraph letterContent1 = section.AddParagraph(content1);
                    spacer = section.AddParagraph();
                    content2 = _lvm.documentsContent.Para2;
                    Paragraph letterContent2 = section.AddParagraph(content2);
                    spacer = section.AddParagraph();
                    content3 = _lvm.documentsContent.Para3;
                    Paragraph letterContent3 = section.AddParagraph();
                    letterContent3.AddFormattedText(content3);
                    spacer = section.AddParagraph();
                    content4 = _lvm.documentsContent.Para4;
                    Paragraph letterContent4 = section.AddParagraph(content4);

                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                }

                //DNA letters
                if (docCode == "DNAp")
                {
                    content1 = _lvm.documentsContent.Para1 + freeText1 + " on " + freeText2 + _lvm.documentsContent.Para2;
                    Paragraph letterContent1 = section.AddParagraph(content1);
                    spacer = section.AddParagraph();
                    content2 = _lvm.documentsContent.Para3;
                    Paragraph letterContent2 = section.AddParagraph(content2);

                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                }


                if (docCode == "DNAr")
                {
                    Paragraph letterContentPt = section.AddParagraph();
                    letterContentPt.AddFormattedText(patName + ", " + patDOB, TextFormat.Bold);
                    spacer = section.AddParagraph();

                    content1 = _lvm.documentsContent.Para1 + _lvm.patient.SALUTATION + _lvm.documentsContent.Para2;
                    Paragraph letterContent1 = section.AddParagraph(content1);
                    spacer = section.AddParagraph();
                    content2 = _lvm.documentsContent.Para3 + _lvm.patient.SALUTATION + _lvm.documentsContent.Para4;
                    Paragraph letterContent2 = section.AddParagraph(content2);
                    content3 = _lvm.documentsContent.Para5;
                    Paragraph letterContent3 = section.AddParagraph(content3);

                    signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                }

                //tf.DrawString("Letter code: " + docCode, font, XBrushes.Black, new XRect(400, 800, 500, 20));
                spacer = section.AddParagraph();
                sigFilename = _lvm.staffMember.StaffForename + _lvm.staffMember.StaffSurname.Replace("'", "").Replace(" ", "") + ".jpg";

                if (docCode != "DT15")
                {
                    Paragraph contentSignOff = section.AddParagraph("Yours sincerely,");
                }
                spacer = section.AddParagraph();

                if (signOff == "CGU Booking Centre")
                {
                    spacer = section.AddParagraph();
                    spacer = section.AddParagraph(); //apparently it's not possible to set a paragraph to have a fixed height.
                }
                else
                {
                    Paragraph contentSig = section.AddParagraph();

                    if (System.IO.File.Exists(@$"wwwroot\Signatures\{sigFilename}"))
                    {
                        MigraDoc.DocumentObjectModel.Shapes.Image sig = contentSig.AddImage(@$"wwwroot\Signatures\{sigFilename}");
                    }
                    else
                    {
                        spacer = section.AddParagraph();
                        spacer = section.AddParagraph();
                    }
                }

                Paragraph contentSignOffName = section.AddParagraph(signOff);

                if (enclosures != "" && enclosures != null)
                {
                    spacer = section.AddParagraph();
                    spacer = section.AddParagraph();
                    Paragraph contentEncs = section.AddParagraph("Enc: " + enclosures);
                }

                if (ccs[0] != "")
                {
                    section.AddPageBreak();

                    int ccLength = 50;
                    spacer = section.AddParagraph();
                    Paragraph contentCC = section.AddParagraph("cc:");                    

                    //Add a page for all of the CC addresses (must be declared here or we can't use it)            
                    for (int i = 0; i < ccs.Length; i++)
                    {
                        string cc = "";

                        if (ccs[i] != null && ccs[i] != "")
                        {
                            if (ccs[i].Contains("Ganesan")) //because of course it's hard-coded
                            {
                                cc = ccs[i];
                            }
                            else
                            {
                                if (ccs[i] == referrerName)
                                {
                                    cc = referrerName + _externalClinicianData.GetCCDetails(_lvm.referrer);
                                }
                                if (ccs[i] == gpName)
                                {
                                    cc = gpName + _externalClinicianData.GetCCDetails(_lvm.gp);
                                }
                                if (ccs[i] == otherName && ccs[i] != "")
                                {
                                    cc = otherName + _externalClinicianData.GetCCDetails(_lvm.other);
                                }
                            }
                            spacer = section.AddParagraph();
                            spacer = section.AddParagraph();
                            MigraDoc.DocumentObjectModel.Tables.Table tableCC = section.AddTable();

                            MigraDoc.DocumentObjectModel.Tables.Column colCC = tableCC.AddColumn();
                            MigraDoc.DocumentObjectModel.Tables.Column colADDRESS = tableCC.AddColumn();
                            MigraDoc.DocumentObjectModel.Tables.Row rowcc = tableCC.AddRow();
                            colCC.Width = 20;
                            colADDRESS.Width = 300;

                            rowcc[0].AddParagraph("cc:");
                            rowcc[1].AddParagraph(cc);
                            spacer = section.AddParagraph();
                            spacer = section.AddParagraph();
                            printCount = printCount += 1;
                            ccLength += 150;
                            if (_documentsData.GetDocumentData(docCode).HasAdditionalActions)
                            {
                                printCount = printCount += 1;
                            }
                            spacer = section.AddParagraph();
                        }

                    }
                }

                spacer = section.AddParagraph();

                if (leafletID != 0)
                {
                    Leaflet enc = _leafletData.GetLeafletDetails(leafletID.GetValueOrDefault());

                    Paragraph contentEnclosures = section.AddParagraph("Enc " + Environment.NewLine + $"{enc.Code} Leaflet - ({enc.Name})");
                    contentEnclosures.Format.Font.Size = 12;
                }
                spacer = section.AddParagraph();

                Paragraph contentDocCode = section.AddParagraph("Letter code: " + docCode);
                contentDocCode.Format.Alignment = ParagraphAlignment.Right;

                //Finally we set the filename for the output PDF
                //needs to be: "CaStdLetter"-CGU number-DocCode-Patient/relative ID (usually "[MPI]-0")-RefID-"print count (if CCs present)"-date/time stamp-Diary ID
                                
                PdfDocumentRenderer pdf = new PdfDocumentRenderer();
                pdf.Document = document;
                pdf.RenderDocument();

                pdf.PdfDocument.Save(Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\StandardLetterPreviews\\preview-{user}.pdf"));

                if (!isPreview.GetValueOrDefault())
                {
                    string edmsPath = _constantsData.GetConstant("PrintPathEDMS", 1);
                    System.IO.File.Copy($"wwwroot\\StandardLetterPreviews\\preview-{user}.pdf", $@"{edmsPath}\CaStdLetter-{fileCGU}-{docCode}-{mpiString}-0-{refIDString}-{printCount.ToString()}-{dateTimeString}-{diaryIDString}.pdf");

                }


            }
        }

        public void DoConsentForm(int id, int mpi, int refID, string user, string referrer, string? additionalText = "", string? enclosures = "", int? reviewAtAge = 0,
            string? tissueType = "", bool? isResearchStudy = false, bool? isScreeningRels = false, int? diaryID = 0, string? freeText1 = "", string? freeText2 = "",
            int? relID = 0, string? clinicianCode = "", string? siteText = "", DateTime? diagDate = null, bool? isPreview = false)
        {
            //Because of the way these are formatted, these are better off being done in PDFSharp.

            _lvm.staffMember = _staffUser.GetStaffMemberDetails(user);
            _lvm.patient = _patientData.GetPatientDetails(mpi);
            _lvm.documentsContent = _documentsData.GetDocumentDetails(id);
            _lvm.referrer = _externalClinicianData.GetClinicianDetails(referrer);
            _lvm.gp = _externalClinicianData.GetClinicianDetails(_lvm.patient.GP_Code);
            _lvm.other = _externalClinicianData.GetClinicianDetails(clinicianCode);

            var referral = _referralData.GetReferralDetails(refID);
            string docCode = _lvm.documentsContent.DocCode;
            //creates a new PDF document
            PdfSharpCore.Pdf.PdfDocument document = new PdfSharpCore.Pdf.PdfDocument();
            document.Info.Title = "My PDF";
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            var tf = new XTextFormatter(gfx);
            //set the fonts used for the letters
            XFont font = new XFont("Arial", 12, XFontStyle.Regular);
            XFont fontBold = new XFont("Arial", 12, XFontStyle.Bold);
            XFont fontItalic = new XFont("Arial", 12, XFontStyle.Italic);
            XFont fontSmall = new XFont("Arial", 10, XFontStyle.Regular);
            XFont fontSmallBold = new XFont("Arial", 10, XFontStyle.Bold);
            XFont fontSmallItalic = new XFont("Arial", 10, XFontStyle.Italic);
            //Load the image for the letter head
            XImage image = XImage.FromFile(@"wwwroot\Letterhead.jpg");
            gfx.DrawImage(image, 350, 20, image.PixelWidth / 2, image.PixelHeight / 2);
            //Create the stuff that's common to all letters
            tf.Alignment = XParagraphAlignment.Right;
            //Our address and contact details
            tf.DrawString(_lvm.documentsContent.OurAddress, font, XBrushes.Black, new XRect(-20, 150, page.Width, 200));

            //Note: Xrect parameters are: (Xpos, Ypos, Width, Depth) - use to position blocks of text
            //Depth of 10 seems sufficient for one line of text; 30 is sufficient for two lines. 7 lines needs 100.

            //patient's address
            tf.Alignment = XParagraphAlignment.Left;

            string name = "";
            string patName = "";
            string address = "";
            string patAddress = "";
            string salutation = "";
            DateTime patDOB = DateTime.Now;

            if (relID == 0)
            {
                patName = _lvm.patient.PtLetterAddressee;
                patDOB = _lvm.patient.DOB.GetValueOrDefault();
            }
            else
            {
                _lvm.relative = _relativeData.GetRelativeDetails(relID.GetValueOrDefault());

                patName = _lvm.relative.Name;
                patDOB = _lvm.relative.DOB.GetValueOrDefault();
            }


            name = _lvm.patient.PtLetterAddressee;
            salutation = _lvm.patient.SALUTATION;


            //Content containers for all of the paragraphs, as well as other data required
            string content1 = "";
            string content2 = "";
            string content3 = "";
            string content4 = "";
            string content5 = "";
            string content6 = "";
            string freetext = freeText1;
            string quoteRef = "";
            string signOff = "";
            string sigFilename = "";

            //WHY IS THERE ALWAYS A NULL SOMEWHWERE?????????


            int totalLength = 400; //used for spacing - so the paragraphs can dynamically resize
            int totalLength2 = 40;



            tf.DrawString("CGU No. : " + _lvm.patient.CGU_No + "/CF", font, XBrushes.Black, new XRect(40, 130, 400, 20));

            string fileCGU = _lvm.patient.CGU_No.Replace(".", "-");
            string mpiString = _lvm.patient.MPI.ToString();
            string refIDString = refID.ToString();
            string dateTimeString = DateTime.Now.ToString("yyyyMMddHHmmss");
            string diaryIDString = diaryID.ToString();

            //CF01
            if (docCode == "CF01") //apparently the CF01 is hardcoded, for some reason, even though it has an entry in ListDocumentsContent!
            {                       //So I don't want to mess with the existing data.

                totalLength = totalLength - 120;
                tf.DrawString("Consent for Access to Medical Records", fontBold, XBrushes.Black, new XRect(300, totalLength, 500, 20));
                totalLength = totalLength + 20;

                content1 = "I understand that there may be a genetic factor that runs through my family which causes a susceptibility to " +
                    System.Environment.NewLine + System.Environment.NewLine +
                    "---------------------------------------------------------------------------------------------------------------------.";
                tf.DrawString(content1, fontSmall, XBrushes.Black, new XRect(40, totalLength, 500, 75));
                totalLength = totalLength + 40;

                content2 = "Information obtained from my medical records will only be used to provide appropriate advice for me and/or my " +
                    "relatives regarding the inherited condition which may exist in my family. No information other than that relating directly " +
                    "to the family history will be disclosed.";
                tf.DrawString(content2, fontSmall, XBrushes.Black, new XRect(40, totalLength, 500, 50));
                totalLength = totalLength + 50;
                content3 = "I give my consent for the clinical genetics unit at Birmingham Women’s Hospital to obtain access to my medical records.";
                tf.DrawString(content3, fontSmall, XBrushes.Black, new XRect(40, totalLength, 350, 80));
                tf.DrawString("Yes / No", fontSmall, XBrushes.Black, new XRect(400, totalLength, 500, 80));
                totalLength = totalLength + 30;
                content4 = "Fill in details for person whose records are to be accessed";
                tf.DrawString(content4, fontSmallItalic, XBrushes.Black, new XRect(40, totalLength, 500, 40));
                totalLength = totalLength + 20;
                tf.DrawString("Name:", fontSmallBold, XBrushes.Black, new XRect(40, totalLength, 100, 40));

                tf.DrawString(patName, fontSmall, XBrushes.Black, new XRect(150, totalLength, 200, 40));
                tf.DrawString("Date of birth:", fontSmallBold, XBrushes.Black, new XRect(350, totalLength, 100, 40));
                tf.DrawString(_lvm.patient.DOB.Value.ToString("dd/MM/yyyy"), font, XBrushes.Black, new XRect(450, totalLength, 200, 40));

                tf.DrawString("Address:", fontSmallBold, XBrushes.Black, new XRect(40, totalLength + 20, 100, 40));

                totalLength = totalLength + 15;
                tf.DrawString(patAddress, fontSmall, XBrushes.Black, new XRect(150, totalLength, 200, 200));
                totalLength = totalLength + 60;
                tf.DrawString("Daytime telephone no:", fontSmallBold, XBrushes.Black, new XRect(40, totalLength, 200, 40));
                tf.DrawString("-----------------------------------------------------------------------------", fontSmall, XBrushes.Black, new XRect(300, totalLength + 5, 400, 40));
                totalLength = totalLength + 20;
                tf.DrawString("Name of hospital holding records:", fontSmallBold, XBrushes.Black, new XRect(40, totalLength, 200, 40));
                tf.DrawString("-----------------------------------------------------------------------------", fontSmall, XBrushes.Black, new XRect(300, totalLength + 5, 600, 40));
                totalLength = totalLength + 20;
                tf.DrawString("Address of hospital holding records:", fontSmallBold, XBrushes.Black, new XRect(40, totalLength, 200, 40));
                tf.DrawString("-----------------------------------------------------------------------------", fontSmall, XBrushes.Black, new XRect(300, totalLength + 5, 500, 40));
                totalLength = totalLength + 25;
                tf.DrawString("-----------------------------------------------------------------------------------------------------------------------------------", fontSmall, XBrushes.Black, new XRect(100, totalLength, 400, 40));
                totalLength = totalLength + 20;
                tf.DrawString("Name of GP:", fontSmallBold, XBrushes.Black, new XRect(40, totalLength, 200, 40));
                tf.DrawString("----------------------------------------------------------------------------------------------------------------------", fontSmall, XBrushes.Black, new XRect(150, totalLength + 5, 450, 40));
                totalLength = totalLength + 20;
                tf.DrawString("Address of GP:", fontSmallBold, XBrushes.Black, new XRect(40, totalLength, 200, 40));
                tf.DrawString("----------------------------------------------------------------------------------------------------------------------", fontSmall, XBrushes.Black, new XRect(150, totalLength + 5, 400, 40));
                totalLength = totalLength + 20;
                tf.DrawString("--------------------------------------------------------------------------------------------------------------------------------", font, XBrushes.Black, new XRect(40, totalLength, 400, 40));
                totalLength = totalLength + 20;
                tf.DrawString("Signature:", fontSmallBold, XBrushes.Black, new XRect(40, totalLength, 200, 40));
                tf.DrawString("-----------------------------------------------------------------------------------", fontSmall, XBrushes.Black, new XRect(120, totalLength + 5, 400, 40));
                tf.DrawString("Date:", fontSmallBold, XBrushes.Black, new XRect(400, totalLength, 200, 40));
                tf.DrawString("----------------------------", fontSmall, XBrushes.Black, new XRect(450, totalLength + 5, 400, 40));
                totalLength = totalLength + 20;
                content5 = "If you are signing on behalf of a child or individual who is unable to give consent please complete below:";
                tf.DrawString(content5, fontSmallItalic, XBrushes.Black, new XRect(80, totalLength, 500, 40));
                totalLength = totalLength + 20;
                tf.DrawString("Your Name:", fontSmallBold, XBrushes.Black, new XRect(40, totalLength, 200, 40));
                tf.DrawString("---------------------------------------", fontSmall, XBrushes.Black, new XRect(120, totalLength + 5, 400, 40));
                tf.DrawString("Relationship to individual above:", fontSmallBold, XBrushes.Black, new XRect(280, totalLength, 200, 40));
                tf.DrawString("--------------------------------------", fontSmall, XBrushes.Black, new XRect(450, totalLength + 5, 400, 40));
                totalLength = totalLength + 20;
                tf.DrawString("Details of why you are giving consent on their behalf:", fontSmallBold, XBrushes.Black, new XRect(40, totalLength, 300, 40));
                tf.DrawString("-----------------------------------------------------------", fontSmall, XBrushes.Black, new XRect(320, totalLength + 5, 400, 40));
                totalLength = totalLength + 20;
                tf.DrawString("(eg. child, legally nominated representative, etc)", fontSmallItalic, XBrushes.Black, new XRect(40, totalLength, 250, 40));
                tf.DrawString("-----------------------------------------------------------", fontSmall, XBrushes.Black, new XRect(320, totalLength + 5, 400, 40));
                totalLength = totalLength + 20;
                content6 = "NB If you have ‘Power of Attorney’ for this relative and are signing on their behalf, " +
                    "please enclose a copy of your ‘Power of Attorney’ document for our records";
                tf.DrawString(content6, fontSmallItalic, XBrushes.Black, new XRect(40, totalLength, 520, 40));

            }



            //CF02d
            if (docCode == "CF02d")
            {
                totalLength = totalLength - 120;
                tf.DrawString("Consent for DNA Storage", fontBold, XBrushes.Black, new XRect(400, totalLength, 500, 20));
                totalLength = totalLength + 20;

                content1 = _lvm.documentsContent.Para1 + " " + siteText + " cancer.";
                tf.DrawString(content1, fontSmall, XBrushes.Black, new XRect(40, totalLength, 500, content1.Length / 4));
                totalLength = totalLength + content1.Length / 4;

                content2 = _lvm.documentsContent.Para2;
                tf.DrawString(content2, fontSmall, XBrushes.Black, new XRect(40, totalLength, 500, content2.Length / 4));
                totalLength = totalLength + content2.Length / 4;

                content3 = _lvm.documentsContent.Para4; ;
                tf.DrawString(content3, fontSmall, XBrushes.Black, new XRect(40, totalLength, 500, 40));
                tf.DrawString("*Yes / No", fontSmall, XBrushes.Black, new XRect(400, totalLength, 500, 20));
                totalLength = totalLength + 40;

                content4 = _lvm.documentsContent.Para5;
                tf.DrawString(content4, font, XBrushes.Black, new XRect(40, totalLength, 500, 40));
                tf.DrawString("*Yes / No", font, XBrushes.Black, new XRect(400, totalLength, 500, 20));
                totalLength = totalLength + 40;

                content5 = "*Please delete as appropriate";
                tf.DrawString(content5, fontSmallItalic, XBrushes.Blue, new XRect(40, totalLength, 500, 40));
                totalLength = totalLength + 40;

                tf.DrawString("Reference:", fontSmallBold, XBrushes.Black, new XRect(40, totalLength, 500, 40));
                tf.DrawString(_lvm.patient.CGU_No, fontSmall, XBrushes.Black, new XRect(100, totalLength, 500, 40));
                totalLength = totalLength + 20;
                tf.DrawString("Date of birth:", fontSmallBold, XBrushes.Black, new XRect(40, totalLength, 500, 40));
                tf.DrawString(_lvm.patient.DOB.Value.ToString("dd/MM/yyyy"), font, XBrushes.Black, new XRect(100, totalLength, 500, 40));
                totalLength = totalLength + 20;
                tf.DrawString("Details:", fontSmallBold, XBrushes.Black, new XRect(40, totalLength, 500, 40));
                tf.DrawString(patAddress, fontSmall, XBrushes.Black, new XRect(100, totalLength, 500, 80));
                totalLength = totalLength + 80;

                content6 = "Hospital where " + siteText + " surgery performed (please complete if known):";
                tf.DrawString(content6, fontSmall, XBrushes.Black, new XRect(40, totalLength, 500, 40));
                totalLength = totalLength + 20;
                tf.DrawString("_______________________________", fontSmall, XBrushes.Black, new XRect(400, totalLength, 500, 40));
                totalLength = totalLength + 20;

                tf.DrawString("Patient Signature:", fontSmallBold, XBrushes.Black, new XRect(40, totalLength, 500, 40));
                tf.DrawString("___________________________________________", fontSmall, XBrushes.Black, new XRect(150, totalLength, 500, 40));
                tf.DrawString("Date:", fontSmallBold, XBrushes.Black, new XRect(300, totalLength, 500, 40));
                tf.DrawString("_______________________", fontSmall, XBrushes.Black, new XRect(350, totalLength, 500, 40));
                totalLength = totalLength + 40;
                tf.DrawString("Please print your name:", fontSmallBold, XBrushes.Black, new XRect(40, totalLength, 500, 40));
                tf.DrawString("_______________________________________________", fontSmall, XBrushes.Black, new XRect(150, totalLength, 500, 40));

            }

            //CF02t
            if (docCode == "CF02t")
            {
                totalLength = totalLength - 140;
                tf.DrawString("Consent for Tissue Studies", fontBold, XBrushes.Black, new XRect(200, totalLength, 500, 20));
                totalLength = totalLength + 20;

                content1 = _lvm.documentsContent.Para1 + " " + siteText + " cancer.";
                tf.DrawString(content1, fontSmall, XBrushes.Black, new XRect(40, totalLength, 500, content1.Length / 4));
                totalLength = totalLength + content1.Length / 4;

                content2 = _lvm.documentsContent.Para2;
                tf.DrawString(content2, fontSmall, XBrushes.Black, new XRect(40, totalLength, 500, content2.Length / 4));
                totalLength = totalLength + content2.Length / 4;

                content3 = _lvm.documentsContent.Para4; ;
                tf.DrawString(content3, fontSmall, XBrushes.Black, new XRect(40, totalLength, 350, 40));
                tf.DrawString("*Yes / No", fontSmall, XBrushes.Black, new XRect(400, totalLength, 100, 20));
                totalLength = totalLength + 40;

                content4 = _lvm.documentsContent.Para5;
                tf.DrawString(content4, fontSmall, XBrushes.Black, new XRect(40, totalLength, 350, 40));
                tf.DrawString("*Yes / No", fontSmall, XBrushes.Black, new XRect(400, totalLength, 100, 20));
                totalLength = totalLength + 60;

                content5 = _lvm.documentsContent.Para5;
                tf.DrawString(content5, fontSmall, XBrushes.Black, new XRect(40, totalLength, 350, 40));
                tf.DrawString("*Yes / No", fontSmall, XBrushes.Black, new XRect(400, totalLength, 100, 20));
                totalLength = totalLength + 40;

                content6 = "*Please delete as appropriate";
                tf.DrawString(content6, fontSmallItalic, XBrushes.Blue, new XRect(40, totalLength, 500, 40));
                totalLength = totalLength + 40;

                tf.DrawString("Reference:", fontSmallBold, XBrushes.Black, new XRect(40, totalLength, 500, 40));
                tf.DrawString(_lvm.patient.CGU_No, fontSmall, XBrushes.Black, new XRect(150, totalLength, 500, 40));
                totalLength = totalLength + 20;
                tf.DrawString("Name:", fontSmallBold, XBrushes.Black, new XRect(40, totalLength, 500, 40));
                tf.DrawString(patName, fontSmall, XBrushes.Black, new XRect(150, totalLength, 500, 80));
                totalLength = totalLength + 20;
                tf.DrawString("Date of birth:", fontSmallBold, XBrushes.Black, new XRect(40, totalLength, 500, 40));
                tf.DrawString(_lvm.patient.DOB.Value.ToString("dd/MM/yyyy"), fontSmall, XBrushes.Black, new XRect(150, totalLength, 500, 40));


                totalLength = totalLength + 80;

                content6 = "Hospital where " + siteText + " surgery performed (please complete if known):";
                tf.DrawString(content6, fontSmall, XBrushes.Black, new XRect(40, totalLength, 500, 40));
                totalLength = totalLength + 20;
                tf.DrawString("_______________________________", fontSmall, XBrushes.Black, new XRect(400, totalLength, 500, 40));
                totalLength = totalLength + 20;

                tf.DrawString("Patient Signature:", fontSmallBold, XBrushes.Black, new XRect(40, totalLength, 500, 40));
                tf.DrawString("___________________________________________", fontSmall, XBrushes.Black, new XRect(150, totalLength, 500, 40));
                tf.DrawString("Date:", fontSmallBold, XBrushes.Black, new XRect(300, totalLength, 500, 40));
                tf.DrawString("_______________________", fontSmall, XBrushes.Black, new XRect(350, totalLength, 500, 40));
                totalLength = totalLength + 40;
                tf.DrawString("Please print your name:", fontSmallBold, XBrushes.Black, new XRect(40, totalLength, 500, 40));
                tf.DrawString("_______________________________________________", fontSmall, XBrushes.Black, new XRect(150, totalLength, 500, 40));
            }

            if (docCode == "ClicsCF01")
            {
                totalLength = totalLength - 140;
                tf.DrawString("Consent for Access to Medical Records", fontBold, XBrushes.Black, new XRect(200, totalLength, 500, 20));
                totalLength = totalLength + 20;

                content1 = "I understand that information obtained from my medical records will only be used to provide appropriate advice for me and/or " +
                    "my relatives regarding the genetic condition which may exist in my family. No information other than that relating directly to the family " +
                    "history will be disclosed."; //because of course it's hardcoded.
                tf.DrawString(content1, fontSmall, XBrushes.Black, new XRect(40, totalLength, 500, content1.Length / 4));
                totalLength = totalLength + content1.Length / 4;

                tf.DrawString("I give my consent for the clinical genetics unit at Birmingham Women’s Hospital to obtain access to my " +
                    "medical records.", fontSmall, XBrushes.Black, new XRect(40, totalLength, 380, 50));

                tf.DrawString("Yes / No", fontSmall, XBrushes.Black, new XRect(450, totalLength, 100, 20));

                totalLength += 40;

                tf.DrawString("Fill in details for person whose records are to be accessed", fontSmallItalic, XBrushes.Black, new XRect(40, totalLength, 500, 40));
                totalLength += 20;

                tf.DrawString("Name:", fontSmallBold, XBrushes.Black, new XRect(40, totalLength, 100, 40));
                tf.DrawString(patName, fontSmall, XBrushes.Black, new XRect(120, totalLength, 400, 40));
                tf.DrawString("Date of birth:", fontSmallBold, XBrushes.Black, new XRect(350, totalLength, 100, 40));
                tf.DrawString(patDOB.ToString("dd/MM/yyyy"), fontSmall, XBrushes.Black, new XRect(480, totalLength, 400, 40));
                totalLength += 20;

                List<Alert> alerts = _alertData.GetAlertsList(mpi);

                if (alerts.Where(a => a.ProtectedAddress == true).Count() > 1)
                {
                    patAddress = "[Protected Address]";
                }
                else
                {
                    patAddress = _add.GetAddress("PT", refID);
                }

                tf.DrawString("Address:", fontSmallBold, XBrushes.Black, new XRect(40, totalLength, 100, 40));
                tf.DrawString(patAddress, fontSmall, XBrushes.Black, new XRect(120, totalLength, 400, 120));
                totalLength += 100;
                tf.DrawString("Daytime telephone no:", fontSmallBold, XBrushes.Black, new XRect(40, totalLength, 180, 40));
                tf.DrawString("______________________________________", font, XBrushes.Black, new XRect(180, totalLength, 400, 40));
                totalLength += 50;
                tf.DrawString("Name of hospital holding records:", fontSmallBold, XBrushes.Black, new XRect(40, totalLength, 250, 40));
                tf.DrawString("_______________________________________", fontSmall, XBrushes.Black, new XRect(260, totalLength, 400, 40));
                totalLength += 20;
                tf.DrawString("Address of hospital holding records:", fontSmallBold, XBrushes.Black, new XRect(40, totalLength, 220, 40));
                tf.DrawString("_______________________________________", fontSmall, XBrushes.Black, new XRect(260, totalLength, 400, 40));
                totalLength += 20;
                tf.DrawString("________________________________________________________________________", fontSmall, XBrushes.Black, new XRect(40, totalLength, 400, 40));
                totalLength += 20;
                tf.DrawString("________________________________________________________________________", fontSmall, XBrushes.Black, new XRect(40, totalLength, 400, 40));

                totalLength += 50;
                tf.DrawString("Name of GP:", fontSmallBold, XBrushes.Black, new XRect(40, totalLength, 250, 40));
                tf.DrawString("_______________________________________", fontSmall, XBrushes.Black, new XRect(260, totalLength, 400, 40));
                totalLength += 20;
                tf.DrawString("Address of GP:", fontSmallBold, XBrushes.Black, new XRect(40, totalLength, 220, 40));
                tf.DrawString("_______________________________________", fontSmall, XBrushes.Black, new XRect(260, totalLength, 400, 40));
                totalLength += 20;
                tf.DrawString("________________________________________________________________________", fontSmall, XBrushes.Black, new XRect(40, totalLength, 400, 40));
                totalLength += 20;
                tf.DrawString("________________________________________________________________________", fontSmall, XBrushes.Black, new XRect(40, totalLength, 400, 40));


            }



            tf.DrawString("Letter code: " + docCode, fontSmall, XBrushes.Black, new XRect(400, 800, 500, 20));

            sigFilename = _lvm.staffMember.StaffForename + _lvm.staffMember.StaffSurname.Replace("'", "").Replace(" ", "") + ".jpg";

            if (!System.IO.File.Exists(@"wwwroot\Signatures\" + sigFilename)) { sigFilename = "empty.jpg"; } //this only exists because we can't define the image if it's null.

            XImage imageSig = XImage.FromFile(@"wwwroot\Signatures\" + sigFilename);
            int len = imageSig.PixelWidth;
            int hig = imageSig.PixelHeight;

            /*
            var par = _docContext.Constants.FirstOrDefault(p => p.ConstantCode == "FilePathEDMS");
            string filePath = par.ConstantValue;

            //EDMS flename - we have to strip out the spaces that keep inserting themselves into the backend data!
            //Also, we only have a constant value for the OPEX scanner, not the letters folder!
            string letterFileName = filePath.Replace(" ", "") + "\\CaStdLetter-" + fileCGU + "-" + docCode + "-" + mpiString + "-0-" + refIDString + "-" + printCount.ToString() + "-" + dateTimeString + "-" + diaryIDString;
            letterFileName = letterFileName.Replace("ScannerOPEX2", "Letters");
            */
            //document.Save(letterFileName + ".pdf"); - the server can't save it to the watchfolder due to permission issues.
            //So we have to create it locally and have a scheduled job to move it instead.

            //document.Save($@"C:\CGU_DB\Letters\CaStdLetter-{fileCGU}-{docCode}-{mpiString}-0-{refIDString}-{1}-{dateTimeString}-{diaryIDString}.pdf");

            document.Save(Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\StandardLetterPreviews\\preview-{user}.pdf"));

            if (!isPreview.GetValueOrDefault())
            {
                System.IO.File.Copy($"wwwroot\\StandardLetterPreviews\\preview-{user}.pdf", $@"C:\CGU_DB\Letters\CaStdLetter-{fileCGU}-{docCode}-{mpiString}-0-{refIDString}-1-{dateTimeString}-{diaryIDString}.pdf");                
            }            
        }


        void CreateQRImageFile(string qrCode, string user)
        {
            byte[] imageBytes = Convert.FromBase64String(qrCode);
            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                Bitmap image = new Bitmap(ms);
                image.Save($"wwwroot\\Images\\qrCode-{user}.jpg");
            }
        }


        string RemoveHTML(string text)
        {

            text = text.Replace("&nbsp;", System.Environment.NewLine);
            text = text.Replace(System.Environment.NewLine, "newline");
            text = Regex.Replace(text, @"<[^>]+>", "").Trim();
            //text = Regex.Replace(text, @"\n{2,}", " ");
            text = text.Replace("&lt;", "<");
            text = text.Replace("&gt;", ">"); //because sometimes clinicians like to actually use those symbols
            text = text.Replace("newlinenewline", System.Environment.NewLine);
            text = text.Replace("newline", "");
            //this is the ONLY way to strip out the excessive new lines!! (and still can't remove all of them)

            return text;
        }



        
    }
}


