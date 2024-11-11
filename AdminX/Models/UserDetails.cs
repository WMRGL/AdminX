using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace AdminX.Models
{
    [Table("STAFF", Schema = "dbo")]
    public class UserDetails //used for user login, has no function beyond this.
    {
        [Required]
        [DisplayName("User ID")]
        public string EMPLOYEE_NUMBER { get; set; }
        [Required]
        [DisplayName("Password")]
        public string PASSWORD { get; set; }
    }
}
