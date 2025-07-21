using AdminX.Models;
using ClinicalXPDataConnections.Models;

namespace AdminX.ViewModels
{
    public class EDMSVM
    {
        public string Message { get; set; }
        public bool isSuccess { get; set; }
        public Patient patient { get; set; }
        public IFormFile UploadedFile { get; set; }
        public List<DocumentKinds> docKinds { get; set; }
    }
}
