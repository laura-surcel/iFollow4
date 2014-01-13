using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wad.iFollow.Web.Models
{
    public class NotificationModel
    {
        public List<NotificationCommentsModel> NotifComments;
        public List<RatingModel> Ratings;
        public int NotificationCounter;

        public NotificationModel()
        {
            NotifComments = new List<NotificationCommentsModel>();
            Ratings = new List<RatingModel>();
        }
    }
}