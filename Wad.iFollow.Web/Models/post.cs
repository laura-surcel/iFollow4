//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class post
    {
        public post()
        {
            this.comments = new HashSet<comment>();
            this.notifications = new HashSet<notification>();
            this.ratings = new HashSet<rating>();
            this.users = new HashSet<user>();
        }
    
        public long id { get; set; }
        public string message { get; set; }
        public long ownerId { get; set; }
        public Nullable<long> imageId { get; set; }
        public Nullable<System.DateTime> dateCreated { get; set; }
        public Nullable<float> rating { get; set; }
        public Nullable<bool> isDeleted { get; set; }
    
        public virtual ICollection<comment> comments { get; set; }
        public virtual image image { get; set; }
        public virtual ICollection<notification> notifications { get; set; }
        public virtual user user { get; set; }
        public virtual ICollection<rating> ratings { get; set; }
        public virtual ICollection<user> users { get; set; }
    }
}
