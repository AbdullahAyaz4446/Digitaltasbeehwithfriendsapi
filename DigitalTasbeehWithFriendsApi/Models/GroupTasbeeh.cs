//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DigitalTasbeehWithFriendsApi.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class GroupTasbeeh
    {
        public int ID { get; set; }
        public int Group_id { get; set; }
        public int Tasbeeh_id { get; set; }
        public int Goal { get; set; }
        public int Achieved { get; set; }
        public System.DateTime Start_date { get; set; }
        public Nullable<System.DateTime> End_date { get; set; }
        public string Status { get; set; }
    }
}
