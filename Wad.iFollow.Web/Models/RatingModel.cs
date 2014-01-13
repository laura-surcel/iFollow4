using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wad.iFollow.Web.Models
{
    public class RatingModel
    {
        public long? PostId;
        public List<RatingNotificationModel> RatingsNotification;

        public RatingModel()
        {
            RatingsNotification = new List<RatingNotificationModel>();
        }
    }
}