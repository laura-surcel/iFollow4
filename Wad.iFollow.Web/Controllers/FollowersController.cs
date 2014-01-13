using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wad.iFollow.Web.Models;
using System.Data.Entity.Validation;
using System.Diagnostics;

namespace Wad.iFollow.Web.Controllers
{
    public class FollowersController : Controller
    {
        [HttpPost]
        public ActionResult Follow(string submit, int follow)
        {
            user currentUser = Session["user"] as user;
            long uid = (long)Convert.ToDouble(submit);

            if (currentUser == null)
            {
                return Json(new { id = submit, follow = follow});
            }

            using (var entities = new ifollowdatabaseEntities4())
            {
                currentUser = entities.users.First(u => u.id == currentUser.id);

                if (follow == 1)
                {
                    follower fol = new follower();
                    fol.followedId = uid;
                    fol.followerId = currentUser.id;
                    entities.followers.Add(fol);
                }
                else if (follow == 0 && entities.followers.Any(f => f.followedId == uid && f.followerId == currentUser.id))
                {
                    follower fol = entities.followers.First(f => f.followedId == uid && f.followerId == currentUser.id);
                    entities.followers.Remove(fol);
                }

                try
                {
                    entities.SaveChanges();
                }
                catch (DbEntityValidationException dbEx)
                {
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                        }
                    }
                }
            }

            return Json(new { id = submit, follow = follow });
        }

        [HttpPost]
        public ActionResult Block(string submit, int block)
        {
            user currentUser = Session["user"] as user;
            long uid = (long)Convert.ToDouble(submit);

            if (currentUser == null)
            {
                return Json(new { id = submit, block = block });
            }

            using (var entities = new ifollowdatabaseEntities4())
            {
                currentUser = entities.users.First(u => u.id == currentUser.id);
                follower blocked;

                if(entities.followers.Any(f => f.followedId == currentUser.id && f.followerId == uid))
                {
                    blocked = entities.followers.First(f => f.followedId == currentUser.id && f.followerId == uid);
                    if (block == 1)
                    {
                        blocked.isBlocked = true;
                    }
                    else if (block == 0)
                    {
                        blocked.isBlocked = false;
                    }

                    try
                    {
                        entities.SaveChanges();
                    }
                    catch (DbEntityValidationException dbEx)
                    {
                        foreach (var validationErrors in dbEx.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                            }
                        }
                    }
                }
            }

            return Json(new { id = submit, block = block });
        }
    }
}
