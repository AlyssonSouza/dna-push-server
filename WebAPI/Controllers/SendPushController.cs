using Infrastructure.Push;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using WebAPI.Model;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    public class SendPushController : ApiController
    {
        /// <summary>
        /// search offer groups
        /// </summary>
        /// <returns>offer id and status</returns>
        [BasicAuthentication]
        [HttpPost]
        [Route("api/Push/Send")]
        public IHttpActionResult Send([FromBody] SendRequest send)
        {
            try
            {
                PushHelper.CheckIosDeviceTokenAvailability(send.Client);
                return Ok(ReturnError(0, PushHelper.SendPush(send.Push, send.DeviceList, send.Client)));
            }
            catch (ApplicationException ex)
            {
                int resultError = Convert.ToInt32(ex.Message);
                return Content<object>(HttpStatusCode.BadRequest,ReturnError(resultError));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
            return null;
        }


        public object ReturnError(int Code, bool Status = false)
        {
            switch (Code)
            {
                case 0:
                    {
                        return new { Code, Message = "Suceifed!!", Data = Status };
                    }
                case 1:
                    {
                        return new { Code, Message = "Falha no envio da notificação!", Data = Status };
                    }
                case 2:
                    {
                        return new { Code, Message = "Falha no envio da notificação! \nProblemas com Certificado", Data = Status };
                    }
            }
            return null;

        }
    }
}