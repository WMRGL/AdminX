
using AdminX.Data;
using AdminX.Meta;
using AdminX.Models;
using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;


namespace AdminX.Controllers
{
    public class HSController
    {
        private readonly ClinicalContext _context;
        private readonly DocumentContext _docContext;
        private readonly AdminContext _adminContext;
        private readonly IHSData _hSData;
        private readonly IReferralData _referralData;
        private readonly IPatientData _patientData;
        private readonly IConstantsData _constantsData;

        public HSController(ClinicalContext context, DocumentContext documentContext, AdminContext adminContext)
        {
            _context = context;
            _docContext = documentContext;
            _adminContext = adminContext;
            _hSData = new HSData(_adminContext);
            _referralData = new ReferralData(_context);
            _patientData = new PatientData(_context);
            _constantsData = new ConstantsData(_docContext);
        }

        public void PrintHSForm(int refid, int diaryID, string user, bool? isPreview = false)
        {
            string docCode = "HS";
            Referral referral = _referralData.GetReferralDetails(refid);
            Patient pat = _patientData.GetPatientDetails(referral.MPI);
            
            List<HS> hs = _hSData.GetHSList(pat.PEDNO);

            MigraDoc.DocumentObjectModel.Document document = new MigraDoc.DocumentObjectModel.Document();

            Section section = document.AddSection();

            Table tableHeader = section.AddTable();
            Column colh1 = tableHeader.AddColumn();
            colh1.Width = 400;
            Column colh2 = tableHeader.AddColumn();
            colh2.Width = 100;
            Row rowh1 = tableHeader.AddRow();
            rowh1.Cells[0].AddParagraph().AddFormattedText("West Midlands Family Cancer Strategy", TextFormat.Bold);

            MigraDoc.DocumentObjectModel.Shapes.Image imgLogo = rowh1.Cells[1].AddImage(@"wwwroot\Images\NHSlogo.png");
            imgLogo.ScaleWidth = new Unit(0.75, UnitType.Point);
            imgLogo.ScaleHeight = new Unit(0.75, UnitType.Point);
            
            //Paragraph title = section.AddParagraph();
            //title.AddFormattedText("West Midlands Family Cancer Strategy", TextFormat.Bold);

            Paragraph spacer = section.AddParagraph();
            Paragraph title2 = section.AddParagraph();
            title2.AddFormattedText("Cancer registry search results", TextFormat.Bold).Color = Colors.LightGray;

            spacer = section.AddParagraph();
            Paragraph contentPatient = section.AddParagraph();
            contentPatient.AddFormattedText(pat.FIRSTNAME + " " + pat.LASTNAME + " " + pat.DOB.GetValueOrDefault().ToString("dd/MM/yyyy"), TextFormat.Bold).Color = Colors.Blue;
            contentPatient.Format.Font.Size = 10;

            spacer = section.AddParagraph();
            Paragraph contentCguno = section.AddParagraph();
            contentCguno.AddFormattedText("CGU No: " + pat.CGU_No+ " WMFACS ID:" + pat.WMFACSID, TextFormat.Bold);
            contentCguno.Format.Font.Size = 10;

            spacer = section.AddParagraph();
            Paragraph contentConsultant = section.AddParagraph();
            contentConsultant.AddFormattedText(referral.LeadClinician, TextFormat.Bold);
            contentConsultant.Format.Font.Size = 10;

            spacer = section.AddParagraph();
            Paragraph contentRegistryHeader = section.AddParagraph();
            contentRegistryHeader.AddFormattedText("Cancer registry records for the following relatives of the above patient have been obtained:", TextFormat.Bold);
            contentRegistryHeader.Format.Font.Size = 10;
            spacer = section.AddParagraph();

            foreach (var item in hs)
            {
                Table table = section.AddTable();
                Column col1 = table.AddColumn();
                col1.Width = 100;
                col1.Format.Alignment = ParagraphAlignment.Right;
                Column col2 = table.AddColumn();
                col2.Width = 200;
                Column col3 = table.AddColumn();
                col3.Width = 100;
                col3.Format.Alignment = ParagraphAlignment.Right;
                Column col4 = table.AddColumn();
                col4.Width = 100;
                Row row1 = table.AddRow();
                Row row2 = table.AddRow();
                Row row3 = table.AddRow();
                Row row4 = table.AddRow();
                Row row5 = table.AddRow();
                Row row6 = table.AddRow();
                Row row7 = table.AddRow();
                Row row8 = table.AddRow();

                table.SetEdge(0, 0, table.Columns.Count, table.Rows.Count, Edge.Box, BorderStyle.Single, 1, Colors.Black);
                                
                row8.Borders.Bottom.Width = 0.5;

                row1.Cells[0].AddParagraph().AddFormattedText("Name:", TextFormat.Bold);
                row1.Cells[1].AddParagraph().AddFormattedText(item.RelName, TextFormat.Bold);
                row1.Cells[2].AddParagraph().AddFormattedText("Date of birth:", TextFormat.Bold);
                row1.Cells[3].AddParagraph(item.RelDOB);

                row2.Cells[0].AddParagraph().AddFormattedText("Address:", TextFormat.Bold);
                string address = "";
                if(item.RelAdd != null) { address = item.RelAdd; }
                row2.Cells[1].AddParagraph(address);
                row2.Cells[2].AddParagraph().AddFormattedText("Date of death:", TextFormat.Bold);
                row2.Cells[3].AddParagraph(item.RelDOD);

                row3.Cells[2].AddParagraph().AddFormattedText("Comments:", TextFormat.Bold);
                string notes = "";
                if (item.Notes != null) { notes = item.Notes; }
                row3.Cells[3].AddParagraph(notes);

                row4.Cells[0].AddParagraph().AddFormattedText("Diagnosis Date:", TextFormat.Bold);
                row4.Cells[1].AddParagraph(item.ConfDiagDate + " age: " + item.ConfDiagAge);

                row5.Cells[0].AddParagraph().AddFormattedText("Cancer Registry:", TextFormat.Bold);
                string registry = "";
                if(item.Registry != null) { registry = item.Registry; }
                row5.Cells[1].AddParagraph(registry);
                
                row6.Cells[0].AddParagraph().AddFormattedText("Site:", TextFormat.Bold);
                string site = "";
                if (item.Site != null) { site = item.Site; }
                row6.Cells[1].AddParagraph(site);

                row7.Cells[0].AddParagraph().AddFormattedText("Side:", TextFormat.Bold);
                string lat = "";
                if (item.Lat != null) { lat = item.Lat; }
                row7.Cells[1].AddParagraph(lat);

                row8.Cells[0].AddParagraph().AddFormattedText("Morphology:", TextFormat.Bold);
                string morph = "";
                if (item.Morph != null) { morph = item.Morph; }
                row8.Cells[1].AddParagraph(morph);

                spacer = section.AddParagraph();
            }

            PdfDocumentRenderer pdf = new PdfDocumentRenderer();
            pdf.Document = document;
            pdf.RenderDocument();

            pdf.PdfDocument.Save(Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\StandardLetterPreviews\\preview-{user}.pdf"));

            if (!isPreview.GetValueOrDefault())
            {
                string edmsPath = _constantsData.GetConstant("PrintPathEDMS", 1);
                File.Copy($"wwwroot\\StandardLetterPreviews\\preview-{user}.pdf", $@"C:\CGU_DB\Letters\CaStdLetter-{pat.CGU_No}-{docCode}-{pat.MPI.ToString()}-0-{refid.ToString()}-0-{DateTime.Now.ToString("yyyyMMddHHmmSS")}-{diaryID.ToString()}.pdf");

            }           
        }
    }
}
