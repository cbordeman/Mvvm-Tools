using System.Web.Mvc;

namespace MvvmTools.Web.Extensions
{
    public static class UrlHelperExtensions
    {
        public static string ActionSecureOnRelease(this UrlHelper self, string actionName, string controllerName)
        {
#if DEBUG
            return self.Action(actionName, controllerName);
#else
            return self.Action(actionName, controllerName, null, "https");
#endif
        }

        //private static bool ActionHasAttribute<T>(
        //    UrlHelper urlHelper,
        //    string action,
        //    string controller) where T : Attribute
        //{
        //    var actionName = action;
        //    var controllerName = controller;
        //    var controllerFactory = ControllerBuilder.Current.GetControllerFactory();

        //    var rd = urlHelper.RouteCollection.GetRouteData(urlHelper.RequestContext.HttpContext);

        //    if (rd == null) return false;

        //    var otherController = (ControllerBase) controllerFactory
        //        .CreateController(
        //            new RequestContext(urlHelper.RequestContext.HttpContext, rd),
        //            controllerName);

        //    // Check controller first.
        //    var cType = otherController.GetType();
        //    var attributes = cType.GetCustomAttributes(typeof(T), false);
        //    if (attributes.Any())
        //        return true;

        //    // Then check action method.
        //    var controllerDescriptor = new ReflectedControllerDescriptor(
        //        otherController.GetType());

        //    var controllerContext2 = new ControllerContext(
        //        new MockHttpContextWrapper(
        //            urlHelper.RequestContext.HttpContext.ApplicationInstance.Context,
        //            urlHelper.RequestContext.HttpContext.Request.HttpMethod),
        //        rd,
        //        otherController);

        //    var actionDescriptor = controllerDescriptor
        //        .FindAction(controllerContext2, actionName);

        //    attributes = actionDescriptor.GetCustomAttributes(typeof(T), false);

        //    return attributes.Any();
        //}

        //class MockHttpContextWrapper : HttpContextWrapper
        //{
        //    public MockHttpContextWrapper(HttpContext httpContext, string method)
        //        : base(httpContext)
        //    {
        //        Request = new MockHttpRequestWrapper(httpContext.Request, method);
        //    }

        //    public override HttpRequestBase Request { get; }

        //    class MockHttpRequestWrapper : HttpRequestWrapper
        //    {
        //        public MockHttpRequestWrapper(HttpRequest httpRequest, string httpMethod)
        //            : base(httpRequest)
        //        {
        //            HttpMethod = httpMethod;
        //        }

        //        public override string HttpMethod { get; }
        //    }
        //}
    }
}
