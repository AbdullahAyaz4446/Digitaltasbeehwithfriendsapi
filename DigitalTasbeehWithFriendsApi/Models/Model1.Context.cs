﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class DTWFEntities : DbContext
    {
        public DTWFEntities()
            : base("name=DTWFEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Quran_Tasbeeh> Quran_Tasbeeh { get; set; }
        public virtual DbSet<Tasbeeh_Detailes> Tasbeeh_Detailes { get; set; }
        public virtual DbSet<GroupUsers> GroupUsers { get; set; }
        public virtual DbSet<quran_text> quran_text { get; set; }
        public virtual DbSet<Sura> Sura { get; set; }
        public virtual DbSet<wazifa_Deatiles> wazifa_Deatiles { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<wazifa_text> wazifa_text { get; set; }
        public virtual DbSet<Groups> Groups { get; set; }
        public virtual DbSet<Tasbeeh> Tasbeeh { get; set; }
        public virtual DbSet<Chaintasbeehdeatiles> Chaintasbeehdeatiles { get; set; }
        public virtual DbSet<groupusertasbeehdeatiles> groupusertasbeehdeatiles { get; set; }
        public virtual DbSet<Notification> Notification { get; set; }
        public virtual DbSet<GroupTasbeeh> GroupTasbeeh { get; set; }
        public virtual DbSet<AssignToSingleTasbeeh> AssignToSingleTasbeeh { get; set; }
        public virtual DbSet<SingleTasbeeh> SingleTasbeeh { get; set; }
        public virtual DbSet<leavegroupusertasbeehdeatiles> leavegroupusertasbeehdeatiles { get; set; }
        public virtual DbSet<tasbeehlogs> tasbeehlogs { get; set; }
        public virtual DbSet<Request> Request { get; set; }
    }
}
