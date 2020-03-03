using System;
using System.Configuration;
using System.Text;

namespace WebAPI.Utils
{
    public class BasicAuthenticationAttribute : System.Web.Http.Filters.ActionFilterAttribute
    {
        /// <summary>
        /// verifies header autorization
        /// </summary>
        /// <param name="actionContext">action context</param>
        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            //base.OnActionExecuting(actionContext);
            if (null == actionContext.Request.Headers.Authorization)
                actionContext.Response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            else
            {
                string authToken = actionContext.Request.Headers.Authorization.Parameter;
                string decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(authToken));
                string identityGuid = decodedToken.Substring(0, decodedToken.IndexOf(":"));
                string secretKey = decodedToken.Substring(decodedToken.IndexOf(":") + 1);

                if (identityGuid.Equals(ConfigurationManager.AppSettings["IDGuid"]) &&
                    secretKey.Equals(ConfigurationManager.AppSettings["Key"]))
                    base.OnActionExecuting(actionContext);
                else
                    actionContext.Response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            }
        }
    }
}