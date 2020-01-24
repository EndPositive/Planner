using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Planner.Models
{
    public class Shift
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public string Details { get; set; }

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

        public string Users { get; set; }

        public int Series { get; set; }
    }
}
