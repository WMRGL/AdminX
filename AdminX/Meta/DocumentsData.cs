using AdminX.Data;
using AdminX.Models;
using System.Data;

namespace AdminX.Meta
{
    interface IDocumentsData
    {
        public DocumentsContent GetDocumentDetails(int id);
        public DocumentsContent GetDocumentDetailsByDocCode(string docCode);
        public List<Document> GetDocumentsList();
    }
    public class DocumentsData : IDocumentsData
    {       
        private readonly DocumentContext? _docContext;
        
        public DocumentsData(DocumentContext docContext)
        {            
            _docContext = docContext;
        }        
        
        
        public DocumentsContent GetDocumentDetails(int id) //Get content for a type of standard letter by its ID
        {
            DocumentsContent item = _docContext.DocumentsContent.FirstOrDefault(d => d.DocContentID == id);
            return item;
        }

        public DocumentsContent GetDocumentDetailsByDocCode(string docCode) //Get content for a type of standard letter by its ID
        {
            DocumentsContent item = _docContext.DocumentsContent.FirstOrDefault(d => d.DocCode == docCode);
            return item;
        }

        public List<Document> GetDocumentsList() 
        {
            IQueryable<Document> docs = from d in _docContext.Documents
                       where d.TemplateInUseNow == true
                       select d;

            return docs.ToList();
        }
        
    }
}
