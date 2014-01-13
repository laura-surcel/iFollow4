using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wad.iFollow.Web.Models;
using Newtonsoft.Json;

namespace Wad.iFollow.Web.Controllers
{
    public class WallController : BaseController
    {
        //
        // GET: /Main/

        public ActionResult MainPage()
        {
            user currentUser = Session["user"] as user;

            if (currentUser == null)
            {
                Url.Action("LogOff", "Account");
                return RedirectToAction("Index", "Home");
            }

            WallPostsModel wpm = new WallPostsModel();

            //If there is no user, we are redirected to Login page
            try
            {
                currentUser = entities.users.First(u => u.id == currentUser.id);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }

            ICollection<post> posts = currentUser.posts;
            ICollection<image> images = currentUser.images;
            DbSet<follower> followers = entities.followers;
 
            foreach(follower f in followers)
            {
                if (f.followerId == currentUser.id && f.isBlocked == false)
                {
                    using (var conn = new ifollowdatabaseEntities4())
                    {
                        user ff = conn.users.First(u => u.id == f.followedId);
                        foreach (image i in ff.images)
                        {
                            images.Add(i);
                        }

                        foreach (post p in ff.posts)
                        {
                            posts.Add(p);
                        }
                    }
                }
            }

            wpm.BuildFromImagesAndPosts(currentUser.posts, currentUser.images, currentUser.id);

            return View(wpm);          
        }

        public ActionResult WallPost()
        {
            return PartialView("_WallPosts");
        }

        public JsonResult AutoCompleteSearch(string term)
        {
            var result = (from r in entities.users
                            where r.firstName.ToLower().Contains(term.ToLower())
                            select new { r.firstName }).Distinct();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public void Rate(string id, string value)
        {
            user currentUser = Session["user"] as user;
            rating newRating = new rating();
            newRating.userId = currentUser.id;
            newRating.postId = long.Parse(id);
            newRating.value = (int)Convert.ToDouble(value)/10;
            entities.ratings.Add(newRating);
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

            long pId = long.Parse(id);
            notification n = new notification();
            n.id = entities.notifications.Count() + 1;
            n.ownerId = entities.posts.FirstOrDefault(p => p.id == pId).ownerId;
            n.userId = currentUser.id;
            n.rating = (int)Convert.ToDouble(value)/10;
            n.IsRead = true;
            n.postId = Convert.ToInt64(id);
            try
            {
                entities.notifications.Add(n);
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

        public ActionResult SetComment(string currentComment, string postId)
        {
            user currentUser = Session["user"] as user;

            if (currentUser == null)
            {
                Url.Action("LogOff", "Account");
                return RedirectToAction("Index", "Home");
            }

            string username = currentUser.firstName + " " + currentUser.lastName;
            comment c = new comment();
            c.message = currentComment;
            c.postId = Convert.ToInt64(postId);
            c.userId = currentUser.id;
            c.dateCreated = DateTime.UtcNow;
            c.id = entities.comments.Count() + 1;
            entities.comments.Add(c);
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
            long pId = long.Parse(postId);
            notification n = new notification();
            n.id = entities.notifications.Count() + 1;
            n.ownerId = entities.posts.FirstOrDefault(p => p.id == pId).ownerId;
            n.userId = currentUser.id;
            n.commentId = c.id;
            n.IsRead = true;
            n.postId = Convert.ToInt64(postId);
            try
            {
                entities.notifications.Add(n);
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

            JsonResult json = Json(new { message = currentComment, id = postId, username = username });
            return json;
        }

        public string GetCommentsForPost(string postId)
        {
            List<WallComment> deliever = new List<WallComment>();

            long pId = Convert.ToInt64(postId);
            using (var entities = new ifollowdatabaseEntities4())
            {
                if (entities.comments.Any(c => c.postId == pId))
                {
                    List<comment> comms = entities.comments.ToList<comment>();
                    
                    foreach(var c in comms)
                    {
                        if (c.postId == pId)
                        {
                            WallComment cc = new WallComment();
                            cc.message = c.message;
                            cc.username = c.user.firstName + " " + c.user.lastName;
                            cc.dateCreated = c.dateCreated.ToString();
                            deliever.Add(cc);
                        }
                    }
                }
            }       
            return JsonConvert.SerializeObject(deliever, Formatting.Indented);
        }

        public ActionResult Settings()
        {
            ViewBag.Message = "Your contact page.";

            SettingsModel sm = new SettingsModel();
            user currentUser = Session["user"] as user;

            if (currentUser == null)
            {
                Url.Action("LogOff", "Account");
                return RedirectToAction("Index", "Home");
            }

            currentUser = entities.users.First(u => u.id == currentUser.id);

            if (currentUser != null)
            {
                sm.firstName = currentUser.firstName;
                sm.lastName = currentUser.lastName;
                sm.country = currentUser.country;
                sm.city = currentUser.city;
                //sm.birthDate = (System.DateTime)currentUser.birthdate;
            }

            return PartialView("_SettingsModal");
        }

        public ActionResult Post()
        {
            ViewBag.Message = "Your posting page.";
            return PartialView("_PostSettings");
        }

        public ActionResult Profile(string user)
        {
            long currentUserId;
            ProfileModel pm = new ProfileModel();
            pm.avatarPath = "";
            if (user == "current")
            {
                user cuser = Session["user"] as user;
                if (cuser == null)
                {
                    Url.Action("LogOff", "Account");
                    return RedirectToAction("Index", "Home");
                }
                currentUserId = cuser.id;
                pm.isCurrentUser = true;
            }
            else
            {
                user cuser = Session["user"] as user;
                if (cuser == null)
                {
                    Url.Action("LogOff", "Account");
                    return RedirectToAction("Index", "Home");
                }
                currentUserId = (long)Convert.ToDouble(user);
                pm.isCurrentUser = (cuser.id == currentUserId);
            }
                        
            user currentUser = entities.users.First(u => u.id == currentUserId);

            if (entities.images.Any(i => i.ownerId == currentUserId && i.isAvatar == true))
            {
                image avatar = entities.images.First(i => i.ownerId == currentUserId && i.isAvatar == true);
                pm.avatarPath = "~/Images/UserPhotos/" + avatar.url;
            }
            
            pm.userName = currentUser.firstName;
            pm.postsCount = currentUser.posts.Count();
            pm.followersCount = currentUser.followers.Count();
            pm.followedCount = currentUser.followers1.Count();
            pm.elements.BuildFromImagesAndPosts(currentUser.posts, currentUser.images, currentUserId);
            pm.userId = currentUser.id;

            ViewBag.Message = "Your profile page.";
            return PartialView("_ProfilePage", pm);
        }

        public ActionResult ViewPosts(long user)
        {
            ProfileModel pm = new ProfileModel();
            user thisUser = (Session["user"] as user);
            if (thisUser == null)
            {
                return null;
            }
            user currentUser = entities.users.First(u => u.id == user);
            pm.userName = currentUser.firstName;
            pm.postsCount = currentUser.posts.Count();
            pm.followersCount = currentUser.followers.Count();
            pm.followedCount = currentUser.followers1.Count();
            pm.elements.BuildFromImagesAndPosts(currentUser.posts, currentUser.images, thisUser.id);

            if (pm.elements.wallElements.Count() == 0)
            {
                return PartialView("_NoPosts");
            }

            return PartialView("_ProfilePagePosts", pm);
        }

        public ActionResult ViewFollowers(long user)
        {
            user currentUser = Session["user"] as user;
            if (currentUser == null)
            {
                Url.Action("LogOff", "Account");
                return RedirectToAction("Index", "Home");
            }
            FollowersModel fm = new FollowersModel();
            try
            {
                fm.BuildFollowersForUser(user, user == currentUser.id);
            }
            catch
            {
                return PartialView("_NoFollowers");
            }

            return PartialView("_Followers", fm);
        }

        public ActionResult ViewFollowed(long user)
        {
            user currentUser = Session["user"] as user;

            if (currentUser == null)
            {
                return null;
            }

            FollowersModel fm = new FollowersModel();
            try
            {
                fm.BuildFollowedForUser(user, currentUser.id == user);
            }
            catch
            {
                return PartialView("_NoUsersFollowed");
            }

            return PartialView("_Followers", fm);
        }

       public  ActionResult ResetNotifications()
       {
           user currentUser = Session["user"] as user;

           if (currentUser == null)
           {
                return null;
           }

           var notif = entities.notifications.Where(n => n.ownerId == currentUser.id && n.rating > 0 && n.IsRead == true).ToList();
           foreach (var i in notif)
           {
               i.IsRead = false ;
               entities.SaveChanges();
           }
           var comm = entities.notifications.Where(n => n.ownerId == currentUser.id && n.commentId !=null && n.IsRead == true).ToList();
           foreach (var c in comm)
           {
               c.IsRead = false;
               entities.SaveChanges();
           }
           return null;
       }
        public ActionResult Refresh()
        {
            var model = new WallPostsModel();   
            user currentUser = Session["user"] as user;

            if (currentUser == null)
            {
                return null;
            }

            List<RatingModel> Ratings = new List<RatingModel>();
            List<NotificationCommentsModel> Comments = new List<NotificationCommentsModel>();
            if (currentUser != null)
            {
                var postsRating = entities.notifications.Where(u => u.ownerId == currentUser.id && u.userId != currentUser.id && u.rating !=null && u.IsRead==true).Select(p => 
                    new PostNotifModel()
                    {
                    Id = p.postId,
                    UsernameId = p.userId
                    }).ToList();
                var postComm = entities.notifications.Where(u => u.ownerId == currentUser.id && u.userId != currentUser.id && u.commentId != null && u.IsRead == true).Select(p =>
                    new PostNotifModel()
                    {
                        Id = p.postId,
                        UsernameId = p.userId
                    }).Distinct().ToList();
                postsRating.ForEach(p =>
                    Ratings.Add(
                        new RatingModel()
                        {
                            RatingsNotification = entities.notifications.
                                Where(r => r.ownerId == currentUser.id && r.rating > 0  && r.userId != currentUser.id && r.post.id == p.Id && r.IsRead == true).
                                Select(pp => 
                                    new RatingNotificationModel(){
                                      Rate = pp.rating,
                                      Username = entities.users.FirstOrDefault(u => u.id == p.UsernameId).firstName
                                    }).ToList(),
                                PostId = p.Id,
                        }
                 ));
                postComm.ForEach(p =>
                        Comments.Add(
                             new NotificationCommentsModel
                            {
                                Comm = entities.notifications.Where(r => r.ownerId == currentUser.id && r.userId != currentUser.id && r.commentId != null && r.IsRead == true && p.Id == r.postId).
                                Select(pc => new CommentModel()
                                {
                                    UsernameComment = entities.users.FirstOrDefault(u => u.id == p.UsernameId).firstName,
                                    Content = entities.comments.FirstOrDefault(f => f.id == pc.commentId).message
                                }).ToList(),
                                PostId = p.Id
                            }
                        )
                );
                model.notifications.Ratings= Ratings;
                model.notifications.NotifComments = Comments;
                var sum = 0;
                foreach (var c in Comments)
                {
                    sum += c.Comm.Count();
                }
                model.notifications.NotificationCounter += Ratings.Count + sum;
            }

            return PartialView("_NotificationPartial", model);
        }

        public ActionResult SaveAvatar(ProfileModel fileModel)
        {
            JsonResult json = null;
            if (ModelState.IsValid)
            {
                image newImage = null;
                user currentUser = Session["user"] as user;

                if (currentUser == null)
                {
                    Url.Action("LogOff", "Account");
                    return RedirectToAction("Index", "Home");
                }

                if (fileModel != null && fileModel.File != null)
                {
                    string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssffff") + ".png";
                    var path = Path.Combine(Server.MapPath("~/Images/UserPhotos"), timestamp);
                    fileModel.File.SaveAs(path);
                    fileModel.avatarPath = path;

                    string oldFile = "";

                    if (entities.images.Any(i => i.ownerId == currentUser.id && i.isAvatar == true))
                    {
                        image avatar = entities.images.First(i => i.ownerId == currentUser.id && i.isAvatar == true);
                        oldFile = Path.Combine(Server.MapPath("~/Images/UserPhotos"), avatar.url);
                        avatar.url = timestamp;
                    }
                    else
                    {
                        newImage = new image();
                        newImage.isAvatar = true;
                        newImage.isDeleted = false;
                        newImage.url = timestamp;

                        int count = entities.images.Count();
                        newImage.id = count + 1;
                        newImage.ownerId = currentUser.id;

                        entities.images.Add(newImage);
                    }                    
                    
                    try
                    {
                        entities.SaveChanges();
                        json = Json(new { path = timestamp});
                        if (oldFile != "")
                        {
                            System.IO.File.Delete(oldFile);
                        }
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
            
            return json;
        }

        public ActionResult Followers()
        {
            ViewBag.Message = "Your followers page.";
            user currentUser = Session["user"] as user;
            
            FollowersModel fm = new FollowersModel();
            try
            {
                fm.BuildRecommendationsForUser(currentUser.id);
            }
            catch
            {
                Url.Action("LogOff", "Account");
                return RedirectToAction("Index", "Home");
            }           

            return PartialView("_Followers", fm);
        }

        [HttpPost]
        public ActionResult ViewSearchResults(string searchedName)
        {
            ViewBag.Message = "Your followers page.";
            user currentUser = Session["user"] as user;

            FollowersModel fm = new FollowersModel();
            try
            {
                fm.BuildSearchResultsForText(searchedName, currentUser.id);
            }
            catch
            {
                Url.Action("LogOff", "Account");
                return RedirectToAction("Index", "Home");
            }

            return PartialView("_Followers", fm);
        }
    }
}
