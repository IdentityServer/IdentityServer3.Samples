// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogonWorkflowController.cs" company="Enhanced Coding">
//   Copyright (c) 2012-14 Enhanced Coding
// </copyright>
// <summary>
//   Defines the LogonWorkflowController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MvcViewServiceSample.Controllers
{
    using System.Web.Mvc;
    using IdentityServer3.Core.Models;
    using IdentityServer3.Core.ViewModels;

    /// <summary>
    /// Providing the necessary screens that form part of the logon process.
    /// These screens are initiated either automatically by the <see cref="EnhancedCoding.Samples.IdSvrServices.MvcViewService{TController}"/>
    /// </summary>
    public class LogonWorkflowController : Controller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogonWorkflowController"/> class.
        /// </summary>
        public LogonWorkflowController()
        {
        }

        #region Login

        /// <summary>
        /// Loads the HTML for the login page.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="message">
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Login(LoginViewModel model, SignInMessage message)
        {
            return this.View(model);
        }

        #endregion

        #region Logout

        /// <summary>
        /// Loads the HTML for the logout prompt page.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Logout(LogoutViewModel model)
        {
            return this.View(model);
        }

        #endregion

        #region LoggedOut

        /// <summary>
        /// Loads the HTML for the logged out page informing the user that they have successfully logged out.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult LoggedOut(LoggedOutViewModel model)
        {
            return this.View(model);
        }

        #endregion

        #region Consent

        /// <summary>
        /// Loads the HTML for the user consent page.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Consent(ConsentViewModel model)
        {
            return this.View(model);
        }

        #endregion

        #region Permissions

        /// <summary>
        /// Loads the HTML for the client permissions page.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Permissions(ClientPermissionsViewModel model)
        {
            return this.View(model);
        }

        #endregion

        #region Error

        /// <summary>
        /// Loads the HTML for the error page.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public virtual ActionResult Error(ErrorViewModel model)
        {
            return this.View(model);
        }

        #endregion
    }
}