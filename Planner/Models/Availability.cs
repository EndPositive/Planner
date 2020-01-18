using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Planner.Models
{
    public class Availability
    {
        public long Id { get; set; }

        [Display(Name = "Dag")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        [DataType(DataType.Date)]
        [Required]
        public DateTime Date { get; set; }

        [Display(Name = "Vanaf")]
        [DataType(DataType.Time)]
        [Required]
        public TimeSpan StartTime { get; set; }

        [Display(Name = "Tot")]
        [DataType(DataType.Time)]
        [Required]
        public TimeSpan EndTime { get; set; }

        [Display(Name = "Gebruikersnaam")]
        [HiddenInput]
        public string Username { get; set; }
    }
}
