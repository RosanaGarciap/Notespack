using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notespack.Models;

public class Session
{
    //Primary Key
    [Key]
    public int Id { get; set; }

    //Session State
    [AllowedValues(0, 1, ErrorMessage = "Error in Session state " )]
    public byte Session_state { get; set; }

    //Session Date
    
    [Display(Name = "Starting Date")]
    [DataType(DataType.Date)]
    [Required]
    public DateTime Session_Start_Date { get; set; }

    [Display(Name = "Ending Date")]
    [DataType(DataType.Date)]
    [Required]
    public DateTime Session_End_Date { get; set; }

    //foreighn key the id of the user that owns the session
    [ForeignKey("User")]
    
    public int Fk_user_id { get; set; }
    
}