using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace wedding_planner.Models
{
     public class NoPastDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if ((DateTime)value <= DateTime.Now)
                return new ValidationResult("Date must be in the future");
            return ValidationResult.Success;
        }
    }

    [Table("weddings")]
    public class Wedding
    {
        [Key]
        public int WeddingId {get;set;}
        [Required]
        [Display(Name="Wedder One")]
        public string WedderOne {get;set;}
        [Required]
        [Display(Name="Wedder Two")]
        public string WedderTwo {get;set;}
        [Required]
        [NoPastDate(ErrorMessage="Date must be a future date")]
        public DateTime Date {get;set;}
        [Required]
        public string Address {get;set;}
        public DateTime CreatedAt {get;set;}
        public DateTime UpdatedAt {get;set;}
        public int UserId {get;set;}
        public User Planner {get;set;}
        public List<Response> Responses {get;set;}
    }
}