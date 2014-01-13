using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Data.Entity.Validation;
using System.Diagnostics;
using Wad.iFollow.Web.Models;

namespace Wad.iFollow.Web.Controllers
{
    public class PostController : Controller
    {
        //
        // GET: /Post/

        [HttpPost]
        public ActionResult Upload(UploadFileModel fileModel)
        {
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
                    fileModel.Path = timestamp;

                    using (var entities = new ifollowdatabaseEntities4())
                    {
                        newImage = new image();
                        newImage.isAvatar = false;
                        newImage.isDeleted = false;
                        newImage.url = timestamp;

                        newImage.id = entities.images.ToList().Last().id + 1;
                        newImage.ownerId = currentUser.id;

                        entities.images.Add(newImage);
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

                using (var entities = new ifollowdatabaseEntities4())
                {
                    post newPost = new post();

                    newPost.dateCreated = DateTime.UtcNow;
                    newPost.id = entities.posts.ToList().Last().id + 1;

                    if (newImage != null)
                    {
                        newPost.imageId = newImage.id;
                    }
                    newPost.ownerId = currentUser.id;
                    newPost.message = fileModel.Message;
                    newPost.rating = 0;
                    newPost.isDeleted = false;

                    entities.posts.Add(newPost);
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
                return RedirectToAction("MainPage", "Wall");
            }

            return RedirectToAction("MainPage", "Wall");
        }

        public ActionResult DeletePost(string postId)
        {
            long postIId = Convert.ToInt64(postId);
            using (var entities = new ifollowdatabaseEntities4())
            {
                post postToDelete = null;
                image imageToDelete = null;

                if(entities.posts.Any(p => p.id == postIId))
                {
                    postToDelete = entities.posts.First(p => p.id == postIId);
                    if (entities.images.Any(i => i.id == postToDelete.imageId))
                    {
                        imageToDelete = entities.images.First(i => i.id == postToDelete.imageId);
                    }
                }

                if(postToDelete != null)
                    entities.posts.Remove(postToDelete);

                if (imageToDelete != null)
                    entities.images.Remove(imageToDelete);

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

            return RedirectToAction("MainPage", "Wall");
        }
    }
}
