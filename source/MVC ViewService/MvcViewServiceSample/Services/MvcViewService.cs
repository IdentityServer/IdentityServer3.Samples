// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MvcViewService.cs" company="Enhanced Coding">
//   Copyright (c) 2012-14 Enhanced Coding
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EnhancedCoding.Samples.IdSvrServices
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    using IdentityServer3.Core.Models;
    using IdentityServer3.Core.Services;
    using IdentityServer3.Core.Services.Default;
    using IdentityServer3.Core.Validation;
    using IdentityServer3.Core.ViewModels;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    using Encoder = Microsoft.Security.Application.Encoder;
    using Uri = System.Uri;

    /// <summary>
    /// Plugged into the IdentityServerServiceFactory.ViewService, this 
    ///     generates renderable streams of data representing specific screens in IdentityServer.
    /// Generally these are simply MemoryStreams generated from StreamWriters containing HTML.
    /// For each type of screen requested, this class calls a corresponding controller action 
    ///     in the specified MVC Controller to produce the HTML to render.
    /// Where this is not possible, the controller gracefully falls back to straight Razor rendering
    ///     of the relevantly named view, or finally (if this is not possible) - 
    ///     to the default IdentityServer ViewService implementation 
    /// </summary>
    /// <typeparam name="TController">The MVC Controller that handles the various Logon screens</typeparam>
    public class MvcViewService<TController> : IViewService
        where TController : ControllerBase
    {
        /// <summary>
        ///     We will use the DefaultViewService to do the brunt of our work
        /// </summary>
        private readonly DefaultViewService defaultViewService;

        private readonly DefaultViewServiceOptions config;

        private readonly ResourceCache cache = new ResourceCache();

        private readonly HttpContextBase httpContext;

        private readonly IControllerFactory controllerFactory;

        private readonly ViewEngineCollection viewEngineCollection;

        #region Constructors

        public MvcViewService(HttpContextBase httpContext)
            : this(
                httpContext,
                new DefaultViewServiceOptions(),
                new FileSystemWithEmbeddedFallbackViewLoader(),
                ControllerBuilder.Current.GetControllerFactory(),
                ViewEngines.Engines)
        {
        }

        public MvcViewService(
            HttpContextBase httpContext,
            DefaultViewServiceOptions config,
            IViewLoader viewLoader,
            IControllerFactory controllerFactory,
            ViewEngineCollection viewEngineCollection)
        {
            this.httpContext = httpContext;
            this.config = config;
            this.defaultViewService = new DefaultViewService(this.config, viewLoader);
            this.controllerFactory = controllerFactory;
            this.viewEngineCollection = viewEngineCollection;
        }

        #endregion

        #region Override

        /// <summary>
        ///     Loads the HTML for the login page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="message">The message.</param>
        /// <returns>
        ///     Stream for the HTML
        /// </returns>
        public Task<Stream> Login(LoginViewModel model, SignInMessage message)
        {
            return this.GenerateStream(
                model, 
                message, 
                "login", 
                () => this.defaultViewService.Login(model, message));
        }

        /// <summary>
        /// Loads the HTML for the logout prompt page.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <returns>
        /// Stream for the HTML
        /// </returns>
        public Task<Stream> Logout(LogoutViewModel model, SignOutMessage message)
        {
            return this.GenerateStream(
                model, 
                "logout",
                () => this.defaultViewService.Logout(model, message));
        }

        /// <summary>
        ///     Loads the HTML for the logged out page informing the user that they have successfully logged out.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="message"></param>
        /// <returns>
        ///     Stream for the HTML
        /// </returns>
        public Task<Stream> LoggedOut(LoggedOutViewModel model, SignOutMessage message)
        {
           return this.GenerateStream(
                model, 
                "loggedOut",
                () => this.defaultViewService.LoggedOut(model, message));
        }

        /// <summary>
        ///     Loads the HTML for the user consent page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="authorizeRequest"></param>
        /// <returns>
        ///     Stream for the HTML
        /// </returns>
        public Task<Stream> Consent(ConsentViewModel model, ValidatedAuthorizeRequest authorizeRequest)
        {
            return this.GenerateStream(
                model, 
                "consent",
                () => this.defaultViewService.Consent(model, authorizeRequest));
        }

        /// <summary>
        ///     Loads the HTML for the client permissions page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>
        ///     Stream for the HTML
        /// </returns>
        public Task<Stream> ClientPermissions(ClientPermissionsViewModel model)
        {
            // For some reason, this is referred to as "Permissions" instead of "ClientPermissions" in Identity Server.
            // This is hardcoded into their CSS so cannot be changed (unless you are overriding all their CSS)
            // This must also be in lower case for the same reason
            return this.GenerateStream(
                model, 
                "permissions",
                () => this.defaultViewService.ClientPermissions(model));
        }

        /// <summary>
        ///     Loads the HTML for the error page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>
        ///     Stream for the HTML
        /// </returns>
        public Task<Stream> Error(ErrorViewModel model)
        {
            return this.GenerateStream(
                model, 
                "error", 
                () => this.defaultViewService.Error(model));
        }

        #endregion

        #region Generate Stream

        /// <summary>
        ///     Generate a Stream to stream the HTML for this page where a Model and Message have both been supplied.
        /// </summary>
        /// <typeparam name="TViewModel">The Type of the model to pass to the Controller.Action</typeparam>
        /// <typeparam name="TMessage">The Type of the message to pass to the Controller.Action</typeparam>
        /// <param name="model">The model to pass to the Controller Action</param>
        /// <param name="message">The messsage to pass to the Controller Action</param>
        /// <param name="actionName">The name of the Action on the Controller that should be called to generate the HTML for this page</param>
        /// <param name="fallbackFunc">An alternate function to generate the Stream with if the Controller.Action cannot be found, or does not work</param>
        /// <returns>A Stream that will stream the HTML for this page</returns>
        private Task<Stream> GenerateStream<TViewModel, TMessage>(
            TViewModel model,
            TMessage message,
            string actionName,
            Func<Task<Stream>> fallbackFunc)
            where TViewModel : CommonViewModel
            where TMessage : Message
        {
            var result = this.GenerateHtml(actionName, model, message);

            // If we've not been able to generate the HTML, use the fallbackFunc to do so
            if (string.IsNullOrWhiteSpace(result))
            {
                if (fallbackFunc != null)
                {
                    return fallbackFunc();
                }

                return Task.FromResult(this.StringToStream(string.Empty));
            }

            return this.Render(model, actionName, result);
        }

        /// <summary>
        /// Generate a Stream to stream the HTML for this page where only a Model has been supplied.
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="model"></param>
        /// <param name="actionName"></param>
        /// <param name="fallbackFunc"></param>
        /// <returns></returns>
        private Task<Stream> GenerateStream<TViewModel>(
            TViewModel model,
            string actionName,
            Func<Task<Stream>> fallbackFunc)
            where TViewModel : CommonViewModel
        {
            return GenerateStream<TViewModel, Message>(model, null, actionName, fallbackFunc);
        }

        /// <summary>
        ///     Generate the Html for the requested View using MVC controllers or Razor views
        /// </summary>
        /// <param name="actionName">The name of the Action in the Controller that we are looking to execute/render</param>
        /// <param name="model">The Model that should be passed to the Action if possible</param>
        /// <param name="message">The Message that should be passed to the Action if possible</param>
        /// <returns></returns>
        private string GenerateHtml(
            string actionName,
            object model = null,
            object message = null)
        {
            // We want to use Razor to render the HTML since that will allow us to reuse components accross the IdentityServer & root website
            // This is based on code found here:
            // http://weblog.west-wind.com/posts/2012/May/30/Rendering-ASPNET-MVC-Views-to-String

            // Find the controller in question
            var controllerName = typeof(TController).Name.ToLower().Replace("controller", String.Empty);
            var controller = this.FindController(controllerName) as TController;
            
            // If we were unable to find one
            if (controller == null)
            {
                // Stop processing
                return null;
            }

            // Create storage for the Html result
            var generatedContent = string.Empty;

            // Find the Action in Question
            var actionDescriptor = this.FindAction(controller, actionName, model, message);

            // If we were able to find one
            if (actionDescriptor != null)
            {
                // Try to initiate an Action to generate the HTML
                // this is never cached since the Action may render different HTML based on the model/message
                generatedContent = this.ExecuteAction(controller, actionDescriptor, model, message);
            }

            // If we either haven't found an action that was useable,
            //  or the action did not return something we can use
            //  try rendering the Razor view directly
            if (string.IsNullOrWhiteSpace(generatedContent))
            {
                // If caching is disabled, generate every time
                if (!this.config.CacheViews)
                {
                    generatedContent = this.LoadRazorViewDirect(controller, actionName);
                }
                else
                {
                    // Otherwise, load the Razor view from the cache
                    generatedContent = this.cache.Read(actionName);

                    // If we've not found this in the cache...
                    if (string.IsNullOrWhiteSpace(generatedContent))
                    {
                        // generate now
                        generatedContent = this.LoadRazorViewDirect(controller, actionName);

                        // And store in the cache for next time
                        this.cache.Write(actionName, generatedContent);
                    }
                }
            }
            
            // Regardless, release the controller now we're done
            ControllerBuilder.Current.GetControllerFactory().ReleaseController(controller);

            // And return any HTML we might have generated
            return generatedContent;
        }

        /// <summary>
        /// Locate a Controller with the given name in the current MVC context
        /// </summary>
        /// <param name="controllerName"></param>
        /// <returns></returns>
        private ControllerBase FindController(string controllerName)
        {
            // Create the appropriate Route Data
            var routeData = new RouteData();
            routeData.Values.Add("controller", controllerName);

            // FUTURE: Fake HttpContext
            //    We need to to generate a different httpContext every time so that when we execute
            //    the controller, it cannot accidentally manipulate or close this current (outer) request.
            //    However, by creating a new empty request we loose all the context information that
            //    the Controller will need to make its decisions.
            //    There are therefore only 2 options:
            //    (1) Uses a fake HttpContext
            //    THEN
            //        Calls to controller.Execute are always OK
            //    BUT
            //        Anything that requires context information, such as
            //            context.GetOverriddenUserAgent() OR
            //            fakeHttpContext.GetOwinContext()
            //        fails since the fake Request has lost all the info of the genuine Request
            //    (2) Use the current (real) HttpContext
            //    THEN
            //        All Headers, User Agents, Session etc can be accessed by the controller
            //    BUT
            //        If anything doesn't work during the Controller.Execute (i.e. the authentication 
            //        fails with a wrong password) then the UserService does something that closes the
            //        Request (presumably because it's controller thinks it has completed the request
            //        and issues a redirect to the Error page).  This results in the following error
            //        when the Error page tries to process:
            //         "This method or property is not supported after HttpRequest.GetBufferlessInputStream has been invoked."
            //    For now, we'll use a fake HttpContext for now, and copy all the
            //        the pertinent information from the genuine Request into the fake one.
            //        It's worth noting that (A) it uses reflection and will 
            //        therefore be slow, and (B) only the information I know that we need has been 
            //        copied accross - making it not future proof.  What if we change the code and 
            //        need access to a different Request property that we haven't copied to our fake context?
            Debug.Assert(this.httpContext.Request.Url != null, "httpContext.Request.Url != null");
            var fakeHttpRequest = new HttpRequest(
                null,
                this.httpContext.Request.Url.ToString(),
                this.httpContext.Request.Url.Query);
            var fakeHttpResponse = new HttpResponse(null);
            var fakeHttpContext = new HttpContext(fakeHttpRequest, fakeHttpResponse);
            var fakeHttpContextWrapper = new HttpContextWrapper(fakeHttpContext);
            var fakeRequestContext = new RequestContext(fakeHttpContextWrapper, routeData);

            // Needed for authentication
            foreach (var key in this.httpContext.Request.Cookies.AllKeys)
            {
                var cookie = this.httpContext.Request.Cookies[key];
                if (cookie != null)
                {
                    fakeHttpRequest.Cookies.Set(cookie);
                }
            }

            // Needed for "owin.environment"
            foreach (var key in this.httpContext.Items.Keys)
            {
                fakeHttpContext.Items[key] = this.httpContext.Items[key];
            }

            // Needed for "User-Agent" in out DisplayModeProvider
            // From: http://stackoverflow.com/a/13307238
            var t = this.httpContext.Request.Headers.GetType();
            t.InvokeMember(
                "MakeReadWrite",
                BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                fakeHttpRequest.Headers,
                null);
            t.InvokeMember(
                "InvalidateCachedArrays",
                BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                fakeHttpRequest.Headers,
                null);
            foreach (var key in this.httpContext.Request.Headers.AllKeys)
            {
                var item = new ArrayList { this.httpContext.Request.Headers[key] };
                t.InvokeMember(
                    "BaseAdd",
                    BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    fakeHttpRequest.Headers,
                    new object[] { key, item });
            }

            t.InvokeMember(
                "MakeReadOnly",
                BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                fakeHttpRequest.Headers,
                null);

            // Use the Controller Factory to find the relevant controller
            var controller = this.controllerFactory.CreateController(fakeRequestContext, controllerName) as ControllerBase;

            if (controller != null)
            {
                // Setup the context
                controller.ControllerContext = new ControllerContext(fakeHttpContextWrapper, routeData, controller);
            }

            return controller;
        }

        /// <summary>
        /// Find a specific Action in the located Controller based on the name supplied.
        /// Where multiple Actions of that name exist, use the version that matches the maximum number of available/applicable parameters.
        /// </summary>
        /// <param name="controller">The controller containing the action</param>
        /// <param name="actionName">The Action to find in the controller</param>
        /// <param name="model">The Model that should be passed to the Action if possible</param>
        /// <param name="message">The Message that should be passed to the Action if possible</param>
        /// <returns></returns>
        private ActionDescriptor FindAction(
            ControllerBase controller,
            string actionName,
            object model = null,
            object message = null)
        {
            // Now get the Metadata about the controller
            var controllerDescriptor = new ReflectedControllerDescriptor(controller.GetType());

            // List all matching actions
            var actionDescriptor = controllerDescriptor.GetCanonicalActions()
                .Where(
                    ad =>

                        // that have the correct name AND
                        ad.ActionName.ToLower() == actionName.ToLower() &&
                        this.HasAcceptableParameters(ad, model, message))
                
                // Now put the largest number of parameters first
                .OrderByDescending(ad => ad.GetParameters().Count())
                
                // And that the top one
                .FirstOrDefault();

            // If we were able to find it
            if (actionDescriptor != null)
            {
                // Add the action name into the RouteData
                controller.ControllerContext.RouteData.Values.Add("action", actionName);
            }

            return actionDescriptor;
        }

        /// <summary>
        /// Inject the Model &amp; Message into the parameters that will be passed to this Action (if appropriate parameters are available).
        /// </summary>
        /// <param name="controller">The controller that contains this action</param>
        /// <param name="actionDescriptor">The Action in the Controller that is going to be executed</param>
        /// <param name="model">The Model that should be passed to the Action if possible</param>
        /// <param name="message">The Message that should be passed to the Action if possible</param>
        private void PopulateActionParameters(
            ControllerBase controller,
            ActionDescriptor actionDescriptor,
            object model = null,
            object message = null)
        {
            if (actionDescriptor.ControllerDescriptor.ControllerType != controller.GetType())
            {
                throw new ArgumentException("actionDescriptor does not describe a valid action for the controller supplied");
            }

            if (!this.HasAcceptableParameters(actionDescriptor, model, message))
            {
                throw new ArgumentException("actionDescriptor does not have valid parameters that can be populated");
            }

            // Extract the Actions Parameters
            var parameters = actionDescriptor.GetParameters();

            // Extract the parameters we're likely to be filling in
            var firstParam = actionDescriptor.GetParameters().FirstOrDefault();
            var lastParam = actionDescriptor.GetParameters().LastOrDefault();

            // If we're expecting 1, assign either the model or message
            if (parameters.Count() == 1 && (model != null || message != null) && firstParam != null)
            {
                // If it's assignable from Model
                if (model != null && firstParam.ParameterType.IsAssignableFrom(model.GetType()))
                {
                    controller.ControllerContext.RouteData.Values[firstParam.ParameterName] = model;
                }
                else if (message != null)
 
                // Don't need to double check this because the HasAcceptableParameters method has already done this
                /* if (message != null && firstParam.ParameterType.IsAssignableFrom(message.GetType())) */
                {
                    controller.ControllerContext.RouteData.Values[firstParam.ParameterName] = message;
                }
            }

            // If we're expecting 2, assign both the correct way round
            else if (parameters.Count() == 2 && model != null && message != null && firstParam != null &&
                     lastParam != null)
            {
                if (
                    firstParam.ParameterType.IsAssignableFrom(model.GetType()) &&
                    lastParam.ParameterType.IsAssignableFrom(message.GetType()))
                {
                    controller.ControllerContext.RouteData.Values[firstParam.ParameterName] = model;
                    controller.ControllerContext.RouteData.Values[lastParam.ParameterName] = message;
                }
                else

                // Don't need to double check this because the HasAcceptableParameters method has already done this
                /* 
                    if (
                    firstParam.ParameterType.IsAssignableFrom(message.GetType()) &&
                    lastParam.ParameterType.IsAssignableFrom(model.GetType()))
                */
                {
                    controller.ControllerContext.RouteData.Values[firstParam.ParameterName] = message;
                    controller.ControllerContext.RouteData.Values[lastParam.ParameterName] = model;
                }
            }
        }

        /// <summary>
        /// Execute this Action (injecting appropriate parameters are available).
        /// </summary>
        /// <param name="controller">
        /// The controller that contains this action
        /// </param>
        /// <param name="actionDescriptor">
        /// The Action in the Controller that is going to be executed
        /// </param>
        /// <param name="model">
        /// The Model that should be passed to the Action if possible
        /// </param>
        /// <param name="message">
        /// The Message that should be passed to the Action if possible
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string ExecuteAction(
            ControllerBase controller,
            ActionDescriptor actionDescriptor,
            object model = null,
            object message = null)
        {
            // Populate the Action Parameters
            this.PopulateActionParameters(controller, actionDescriptor, model, message);

            // Whilst Capturing the output ...
            using (var it = new ResponseCapture(controller.ControllerContext.RequestContext.HttpContext.Response))
            {
                // Execute the Action (this will automatically invoke any attributes)
                (controller as IController).Execute(controller.ControllerContext.RequestContext);

                // Check for valid status codes
                // EDFUTURE: For now, we only accept methods that produce standard HTML,
                // 302 Redirects and other possibly valid responses are ignored since we 
                // don't need them at the moment and aren't coding to cope with them
                if ((HttpStatusCode)controller.ControllerContext.RequestContext.HttpContext.Response.StatusCode == HttpStatusCode.OK)
                {
                    // And return the generated HTML
                    return it.ToString();
                }

                return null;
            }

            // FUTURE: Fake HttpContext (continued...)
            // The code above assumes that a fake HttpContext is in use for this controller
            //      (controller as IController).Execute(controller.ControllerContext.RequestContext);
            // otherwise we have the problem described in my initial "Fake HttpContext" comments.
            // 
            // It possible instead to execute the Action directly to avoid the problem
            //  of filter performing unexpected manipulations to the Response and Request,
            //  by using this code:
            //      actionDescriptor.Execute(controller.ControllerContext, parameters);
            //  but of course, this means that our filters (such as our [SetDisplayMode] attribute
            //  are never run and thus, the result is not as expected.
            // 
            // As an alternative to, it may be possible to recreate the (controller as IController).Execute(...)
            //  ourselves, but bypass the specific filters that cause unexpected manipulations to the Response and Request.
            // 
            // from Controller.Execute:
            //      ActionInvoker.InvokeAction(ControllerContext, actionName)
            //  from ControllerActionInvoker.InvokeAction:
            //      FilterInfo filterInfo = GetFilters(controllerContext, actionDescriptor);
            //      
            //      // Check no authentication issues
            //      AuthenticationContext authenticationContext = InvokeAuthenticationFilters(controllerContext, filterInfo.AuthenticationFilters, actionDescriptor);
            //      if (authenticationContext.Result == null)
            //      {
            //      
            //          // Check no authorization issues
            //          AuthorizationContext authorizationContext = InvokeAuthorizationFilters(controllerContext, filterInfo.AuthorizationFilters, actionDescriptor);
            //          if (authorizationContext.Result == null)
            //          {
            //      
            //              // Validate the Request
            //              if (controllerContext.Controller.ValidateRequest)
            //              {
            //                  ValidateRequest(controllerContext);
            //              }
            //      
            //              // Run the Action
            //              IDictionary<string, object> parameters = GetParameterValues(controllerContext, actionDescriptor);
            //              ActionExecutedContext postActionContext = InvokeActionMethodWithFilters(controllerContext, filterInfo.ActionFilters, actionDescriptor, parameters);
            //      
            //              // The action succeeded. Let all authentication filters contribute to an action result (to
            //              // combine authentication challenges; some authentication filters need to do negotiation
            //              // even on a successful result). Then, run this action result.
            //              AuthenticationChallengeContext challengeContext = InvokeAuthenticationFiltersChallenge(controllerContext, filterInfo.AuthenticationFilters, actionDescriptor, postActionContext.Result);
            //              InvokeActionResultWithFilters(controllerContext, filterInfo.ResultFilters, challengeContext.Result ?? postActionContext.Result);
            // 
            //              ...
        }

        /// <summary>
        ///     Check that the Action has parameters that are acceptable, i.e. one of these:
        ///     No parameter
        ///     1 parameter that matches the model
        ///     1 parameter that matches the message
        ///     2 parameters matching the model &amp; message (or vice versa)
        /// </summary>
        /// <param name="actionDescriptor">The Action in the Controller that is going to be executed</param>
        /// <param name="model">The Model that should be passed to the Action if possible</param>
        /// <param name="message">The Message that should be passed to the Action if possible</param>
        /// <returns></returns>
        [System.Diagnostics.Contracts.Pure]
        private bool HasAcceptableParameters(
            ActionDescriptor actionDescriptor,
            object model = null,
            object message = null)
        {
            var parameters = actionDescriptor.GetParameters();

            // Has either no parameters OR
            if (!parameters.Any())
            {
                return true;
            }

            // Has 1 parameter ...
            if (parameters.Count() == 1
                && ( // ...that accepts either the Model or the Message (and we have a model or a message)
                (model != null && parameters.First().ParameterType.IsAssignableFrom(model.GetType())) ||
                (message != null && parameters.First().ParameterType.IsAssignableFrom(message.GetType()))))
            {
                return true;
            }

            // Has 2 parameters ...
            if (parameters.Count() == 2 && model != null && message != null
                && (( // ... where one accepts the Model and the other accepts the Message
                        parameters.First().ParameterType.IsAssignableFrom(model.GetType())
                        && parameters.Last().ParameterType.IsAssignableFrom(message.GetType()))
                    || (parameters.Last().ParameterType.IsAssignableFrom(model.GetType())
                        && parameters.First().ParameterType.IsAssignableFrom(message.GetType()))))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Temporarily replace the Response.Output with a StringWriter for the duration of the ResponseCaptures lifetime
        ///     Usage:
        ///     using (var rc = new ResponseCapture(controller.ControllerContext.RequestContext.HttpContext.Response))
        ///     {
        ///     ...
        ///     return rc.ToString();
        ///     }
        ///     From: http://approache.com/blog/render-any-aspnet-mvc-actionresult-to/
        /// </summary>
        private class ResponseCapture : IDisposable
        {
            private readonly HttpResponseBase response;

            private readonly TextWriter originalWriter;

            private StringWriter localWriter;

            public ResponseCapture(HttpResponseBase response)
            {
                this.response = response;
                this.originalWriter = response.Output;
                this.localWriter = new StringWriter();
                response.Output = this.localWriter;
            }

            public override string ToString()
            {
                this.localWriter.Flush();
                return this.localWriter.ToString();
            }

            public void Dispose()
            {
                if (this.localWriter != null)
                {
                    this.localWriter.Dispose();
                    this.localWriter = null;
                    this.response.Output = this.originalWriter;
                }
            }
        }

        #endregion

        #region Load Razor view direct

        /// <summary>
        ///     Renders the "/views/controllername/actionname.cshtml" view (if it exists)
        ///     using the Razor ViewEngine but without passing it through the Controller action
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="viewName"></param>
        /// <returns></returns>
        private string LoadRazorViewDirect(ControllerBase controller, string viewName)
        {
            // Find the appropriate View
            var viewResult = this.viewEngineCollection.FindView(controller.ControllerContext, viewName, null);

            // If we've been able to find one
            if (viewResult != null && viewResult.View != null)
            {
                // Store the result in a StringWriter
                using (var sw = new StringWriter())
                {
                    // Setup the ViewContext
                    var viewContext = new ViewContext(
                        controller.ControllerContext, 
                        viewResult.View, 
                        controller.ViewData, 
                        controller.TempData, 
                        sw);

                    // Render the View
                    viewResult.View.Render(viewContext, sw);

                    // Dispose of the View
                    viewResult.ViewEngine.ReleaseView(controller.ControllerContext, viewResult.View);

                    // Output the resultant HTML string
                    return sw.GetStringBuilder().ToString();
                }
            }

            return null;
        }

        #endregion

        #region Taken from IdentityServer3.Core.Services.Default.DefaultViewService

        /// <summary>
        ///     Render the Html in the same way that the DefaultViewService does
        /// </summary>
        /// <param name="model"></param>
        /// <param name="page"></param>
        /// <param name="html"></param>
        /// <param name="clientName"></param>
        /// <returns></returns>
        private Task<Stream> Render(
            CommonViewModel model,
            string page,
            string html,
            string clientName = null)
        {
            Uri uriSiteUrl;
            var applicationPath = string.Empty;
            if (Uri.TryCreate(model.SiteUrl, UriKind.RelativeOrAbsolute, out uriSiteUrl))
            {
                if (uriSiteUrl.IsAbsoluteUri)
                {
                    applicationPath = uriSiteUrl.AbsolutePath;
                }
                else
                {
                    applicationPath = uriSiteUrl.OriginalString;
                    if (applicationPath.StartsWith("~/"))
                    {
                        applicationPath = applicationPath.TrimStart('~');
                    }
                }

                if (applicationPath.EndsWith("/"))
                {
                    applicationPath = applicationPath.Substring(0, applicationPath.Length - 1);
                }
            }

            var json = JsonConvert.SerializeObject(
                model, 
                Formatting.None, 
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            var additionalStylesheets = this.BuildTags(
                "<link href='{0}' rel='stylesheet'>", 
                applicationPath, 
                this.config.Stylesheets);
            var additionalScripts = this.BuildTags("<script src='{0}'></script>", applicationPath, this.config.Scripts);

            var variables = new
            {
                siteName = Encoder.HtmlEncode(model.SiteName), 
                applicationPath, 
                model = Encoder.HtmlEncode(json), 
                page, 
                stylesheets = additionalStylesheets, 
                scripts = additionalScripts, 
                clientName
            };

            html = Replace(html, variables);

            return Task.FromResult(this.StringToStream(html));
        }

        /// <summary>
        /// A helper method to repeat the generation of a formatted string for every value supplied
        /// </summary>
        /// <param name="tagFormat"></param>
        /// <param name="basePath"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        private string BuildTags(
            string tagFormat,
            string basePath,
            IEnumerable<string> values)
        {
            if (values == null)
            {
                return String.Empty;
            }

            var enumerable = values as string[] ?? values.ToArray();
            if (!enumerable.Any())
            {
                return String.Empty;
            }

            var sb = new StringBuilder();
            foreach (var value in enumerable)
            {
                var path = value;
                if (path.StartsWith("~/"))
                {
                    path = basePath + path.Substring(1);
                }

                sb.AppendFormat(tagFormat, path);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        #endregion

        #region Modified from SampleApp.CustomViewService
        // More helper methods to allow placeholders in the rendered Html

        /// <summary>
        /// Replace placeholders in the string that correspond to the keys in the dictionary with the values of those keys.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        private static string Replace(string value, IDictionary<string, object> values)
        {
            foreach (var key in values.Keys)
            {
                var val = values[key];
                val = val ?? String.Empty;
                if (val != null)
                {
                    value = value.Replace("{" + key + "}", val.ToString());
                }
            }

            return value;
        }

        /// <summary>
        /// Replace placeholders in the string that correspond to the names of properties in the object with the values of those properties.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        private string Replace(string value, object values)
        {
            return Replace(value, this.Map(values));
        }

        /// <summary>
        /// Convert an object into a Dictionary by enumerating and listing its properties
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private IDictionary<string, object> Map(object values)
        {
            var dictionary = values as IDictionary<string, object>;

            if (dictionary == null)
            {
                dictionary = new Dictionary<string, object>();
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(values))
                {
                    dictionary.Add(descriptor.Name, descriptor.GetValue(values));
                }
            }

            return dictionary;
        }

        /// <summary>
        /// Convert a stringto a Stream (containing the string)
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private Stream StringToStream(string s)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            sw.Write(s);
            sw.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        #endregion
    }
}