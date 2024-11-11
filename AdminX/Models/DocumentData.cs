using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AdminX.Models
{   

    [Table("ListDocumentsNEW", Schema = "wmfacs_user")]
    public class Document
    {
        [Key]
        public string DocCode { get; set; }
        public string? DocName { get; set; }
        public string? DocGroup { get; set; }
        public bool TemplateInUseNow { get; set; }
    }

    [Table("ListDocumentsContent", Schema = "wmfacs_user")]
    public class DocumentsContent
    {
        [Key]
        public int DocContentID { get; set; }
        public string DocCode { get; set; }
        public string LetterTo { get; set; }
        public string LetterFrom { get; set; }
        public string? Para1 { get; set; }
        public string? Para2 { get; set; }
        public string? Para3 { get; set; }
        public string? Para4 { get; set; }
        public string? Para5 { get; set; }
        public string? Para6 { get; set; }
        public string? Para7 { get; set; }
        public string? Para8 { get; set; }
        public string? Para9 { get; set; }
        public string? Para10 { get; set; }
        public string? OurAddress { get; set; }
        public string? DirectLine { get; set; }
        public string? OurEmailAddress { get; set; }
    }
     
    [Table("Constants", Schema = "dbo")]
    public class Constant
    {
        [Key]
        public string ConstantCode { get; set; }
        public string ConstantValue { get; set; }
        public string? ConstantValue2 { get; set; }
    }    
}