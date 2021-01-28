using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFLibrary.Models
{
    public class Wheather
    { 
        [Key]
        public int ID { get; set; }

        [DataType(DataType.Date)] 
        [Column(TypeName = "Date")]          //To create column for Datatype Date from DateTime in database.
        public DateTime? Date { get; set; }

        [DataType(DataType.Time)]           //To create column for Datatype Time from DateTime in database. 
        public TimeSpan? Time { get; set; }

        public string Place { get; set; }
        public double? Temp { get; set; }
        public int? Humidity { get; set; }
    }
}