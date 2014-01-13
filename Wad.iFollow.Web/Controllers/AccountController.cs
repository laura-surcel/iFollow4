using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using Wad.iFollow.Web.Models;
using System.Net.Mail;


using System.Text.RegularExpressions;

namespace Wad.iFollow.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(user model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                using (var entities = new ifollowdatabaseEntities4())
                {
                    string email = model.email;
                    string password = model.password;

                    // Now if our password was enctypted or hashed we would have done the
                    // same operation on the user entered password here, But for now
                    // since the password is in plain text lets just authenticate directly

                    user userValid = entities.users.FirstOrDefault(user => user.email == email && user.password == password);

                    // User found in the database
                    if (userValid != null)
                    {
                        if (userValid.isConfirmed == true)
                        {
                            FormsAuthentication.SetAuthCookie(email, false);
                            if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                                && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                            {
                                return Redirect(returnUrl);
                            }
                            else
                            {
                                Session["user"] = entities.users.First(e => e.email == model.email && e.password == model.password);
                                Session.Timeout = 180;
                                return RedirectToAction("MainPage", "Wall");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("", "Please confirm your email first !");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "The user name or password provided is incorrect.");
                    }
                }
            }
            return View("~/Views/Home/Index.cshtml", model);
        }

        //
        // POST: /Account/LogOff

        public ActionResult LogOff()
        {
            Session.Clear();
            Session.Abandon();

            //Signs out of WebSecurity and FormsAuthentication
            //WebSecurity.Logout();
            FormsAuthentication.SignOut();

            //Redirects
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                using (var entities = new ifollowdatabaseEntities4())
                {
                    user newUser = new user();
                    newUser.email = model.UserName;
                    newUser.password = model.Password;
                    newUser.registrationDate = DateTime.UtcNow.Date;
                    char[] delimiters = { '@' };
                    string[] nameComponents = model.UserName.Split(delimiters);
                    newUser.firstName = nameComponents[0];
                    newUser.lastName = @"";
                    newUser.isConfirmed = false;
                    int count = entities.users.Count();
                    newUser.id = count + 1;

                    Guid key;
                    key = Guid.NewGuid();
                    newUser.confirmationKey = key.ToString();

                    entities.users.Add(newUser);
                    try
                    {
                        entities.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("", e);
                    }
                    ResetModel r = new ResetModel();
                    r.UserName = model.UserName;
                    Reset(r, MailType.Register);
                    return RedirectToAction("Index", "Home", new { message = "An email has been sent !Please confirm your email before login!" });
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Reset()
        {
            return View();
        }

        //
        // POST: /Account/Reset Password
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Reset(ResetModel model, MailType type = MailType.ForgotPass)
        {
            if (ModelState.IsValid)
            {

                try
                {

                    SmtpClient ss = new SmtpClient("smtp.gmail.com", 587);
                    ss.EnableSsl = true;
                    ss.Timeout = 10000;
                    ss.DeliveryMethod = SmtpDeliveryMethod.Network;
                    ss.UseDefaultCredentials = false;
                    ss.Credentials = new System.Net.NetworkCredential("ifollow.info@gmail.com", "01012014");

                    // add from,to mailaddresses
                    MailAddress from = new MailAddress("administrator@iFollow.com", "Administrator");
                    MailAddress to = new MailAddress(model.UserName, model.UserName);
                    MailMessage myMail = new System.Net.Mail.MailMessage(from, to);

                    // set subject and encoding

                    myMail.SubjectEncoding = System.Text.Encoding.UTF8;
                    using (var entities = new ifollowdatabaseEntities4())
                    {
                        string guidKey = entities.users.First(u => u.email == model.UserName).confirmationKey;
                        // set body-message and encoding
                        if (type == MailType.ForgotPass)
                        {
                            myMail.Subject = "Reset your password";
                            myMail.Body = "<p>To change you password please click <a href=http://localhost:29330/Account/NewPassword/" + guidKey + "/> here !</p>";
                        }
                        if (type == MailType.Register)
                        {
                            myMail.Subject = "Registration";
                            myMail.Body = "<p>To confirm you account please click <a href=http://localhost:29330/Account/ConfirmRegistration/" + guidKey + "/> here !</p>";
                        }
                        myMail.BodyEncoding = System.Text.Encoding.UTF8;

                        // text or html
                        myMail.IsBodyHtml = true;

                        ss.Send(myMail);

                        myMail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                        ModelState.AddModelError("", "An email has been sent.");
                    }

                }

                catch (SmtpException ex)
                {
                    throw new ApplicationException
                      ("SmtpException has occured: " + ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "The format of the email that you have provided is incorrect.");
                }


            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }



        //
        // POST: /Account/Disassociate

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // Only disassociate the account if the currently logged in user is the owner
            if (ownerAccount == User.Identity.Name)
            {
                // Use a transaction to prevent the user from deleting their last login credential
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
                    {
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = ManageMessageId.RemoveLoginSuccess;
                    }
                }
            }

            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage

        [ActionName("ResetPassword")]
        [HttpGet]
        [AllowAnonymous]
        public ActionResult NewPassword(string Key)
        {
            LocalPasswordModel resPassw = new LocalPasswordModel();
            resPassw.key = Key;
            return View("~/Views/Account/ChangePassword.cshtml", resPassw);
        }

        [ActionName("MailRegister")]
        [HttpGet]
        [AllowAnonymous]
        public ActionResult MailRegistration(string Key)
        {
            user u = new user();
            using (var entities = new ifollowdatabaseEntities4())
            {
                u = entities.users.FirstOrDefault(p => p.confirmationKey == Key);
                u.isConfirmed = true;
                entities.SaveChanges();
            }
            return Login(u);
        }

        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : "";
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Manage(LocalPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                bool changePasswordSucceeded = false;
                using (var entities = new ifollowdatabaseEntities4())
                {
                    entities.users.First(m => m.confirmationKey == model.key).password = model.NewPassword;
                    try
                    {
                        entities.SaveChanges();
                        changePasswordSucceeded = true;
                    }
                    catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                    {
                        changePasswordSucceeded = false;
                    }
                }
                if (changePasswordSucceeded)
                {
                    return RedirectToAction("Index", "Home", new { message = "You Password has been successfully changed" });
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                    model.NewPassword = null;
                    model.ConfirmPassword = null;
                }
            }

            return View("~/Views/Account/ChangePassword.cshtml", model);
        }

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}

public enum MailType
{
    Register,
    ForgotPass
}