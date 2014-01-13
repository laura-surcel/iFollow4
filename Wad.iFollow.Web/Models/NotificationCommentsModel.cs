using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wad.iFollow.Web.Models
{
    public class NotificationCommentsModel
    {
        public long? PostId;
        public List<CommentModel> Comm;

        public NotificationCommentsModel()
        {
            Comm = new List<CommentModel>();
        }
    }
}