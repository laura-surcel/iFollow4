﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Wad.iFollow.Web.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class ifollowdatabaseEntities4 : DbContext
    {
        public ifollowdatabaseEntities4()
            : base("name=ifollowdatabaseEntities4")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<comment> comments { get; set; }
        public DbSet<email> emails { get; set; }
        public DbSet<follower> followers { get; set; }
        public DbSet<image> images { get; set; }
        public DbSet<notification> notifications { get; set; }
        public DbSet<post> posts { get; set; }
        public DbSet<rating> ratings { get; set; }
        public DbSet<user> users { get; set; }
    }
}
