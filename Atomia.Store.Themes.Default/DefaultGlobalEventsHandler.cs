﻿using Atomia.Store.AspNetMvc.Controllers;
using Atomia.Store.AspNetMvc.Infrastructure;
using Atomia.Store.Core;
using Atomia.Web.Base.Configs;
using Atomia.Web.Plugin.Validation.ValidationAttributes;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using System;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Unity.Mvc5;


namespace Atomia.Store.Themes.Default
{
    public class DefaultGlobalEventsHandler : GlobalEventsHandler
    {
        protected virtual void RegisterConfiguration(UnityContainer container)
        {
            
        }

        public override void  Application_Start(object sender, EventArgs e)
        {
            AreaRegistration.RegisterAllAreas();

            var container = new UnityContainer();

            UnityConfig.RegisterComponents(container);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            RegisterConfiguration(container);

            container.LoadConfiguration();

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));

            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(CustomerValidationAttribute), typeof(CustomerValidationAttribute.CustomerValidator));
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(AtomiaRegularExpressionAttribute), typeof(AtomiaRegularExpressionValidator));
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(AtomiaRequiredAttribute), typeof(AtomiaRequiredValidator));
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(AtomiaStringLengthAttribute), typeof(AtomiaStringLengthValidator));
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(AtomiaRangeAttribute), typeof(AtomiaRangeValidator));
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(AtomiaUsernameAttribute), typeof(AtomiaUsernameAttribute.AtomiaUsernameValidator));

            foreach (GlobalSetting globalSetting in AppConfig.Instance.GlobalSettingsList)
            {
                if (HttpContext.Current != null)
                {
                    HttpContext.Current.Application[globalSetting.Name] = globalSetting.Value;
                }
            }
        }

        public override void Session_Start(object sender, EventArgs e)
        {
            if (HttpContext.Current != null)
            {
                if (HttpContext.Current.Session != null)
                {
                    /* We are making an exception to using DI / service location of adapters here, since they have not yet
                       been registered with the Unity container. */
                    var themeNamesProvider = new Atomia.Store.AspNetMvc.Adapters.ThemeNamesProvider();

                    HttpContext.Current.Session["theme"] = themeNamesProvider.GetMainThemeName();
                }
            }
        }

        public override void Application_Error(object sender, EventArgs e)
        {
            if (!HttpContext.Current.IsCustomErrorEnabled)
            {
                return;
            }

            try
            {
                var ex = HttpContext.Current.Server.GetLastError();
                var httpException = ex as HttpException;

                var logger = DependencyResolver.Current.GetService<ILogger>();
                logger.LogException(ex, string.Format("Caught unhandled exception in Order Page v2.\r\n {0}", ex.Message + "\r\n" + ex.StackTrace));

                var routeData = new System.Web.Routing.RouteData();
                routeData.Values.Add("controller", "Error");

                if (httpException != null)
                {
                    switch (httpException.GetHttpCode())
                    {
                        case 401:
                            routeData.Values.Add("action", "Forbidden");
                            break;
                        case 404:
                            routeData.Values.Add("action", "NotFound");
                            break;
                        default:
                            routeData.Values.Add("action", "InternalServerError");
                            break;
                    }
                }
                else
                {
                    routeData.Values.Add("action", "InternalServerError");
                }

                routeData.Values.Add("error", ex);

                HttpContext.Current.Server.ClearError();

                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.TrySkipIisCustomErrors = true;
                
                IController errorController = new ErrorController();
                errorController.Execute(new RequestContext(new HttpContextWrapper(HttpContext.Current), routeData));
            }
            catch
            {
                // This is a last ditch effort in case something goes wrong with the regular error handling.
                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                HttpContext.Current.Response.ContentType = "text/html";
                HttpContext.Current.Response.WriteFile("~/Content/Error.html");
            }
        }
    }
}
