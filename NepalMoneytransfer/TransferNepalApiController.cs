using DMTServices.Models.Common;
using DMTServices.Models.NepalMoney;
using Newtonsoft.Json;
using Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using TMC.Controllers;

namespace DMTServices.Controllers
{
    public class TransferNepalController : ApiController
    {
        // GET api/values
      
        string CID = string.Empty;
        string requestString = string.Empty;
        string requestContentType = string.Empty;
        StringBuilder sLogs = new StringBuilder();
        string _companyId = string.Empty;
        string apiKey = String.Empty;
        string DMTAPIuserid = string.Empty;
        string DMTAPIpassword = string.Empty;
        string CompanyId = "";
        string requestIP = "";
        string Usertype = "0";
        // ClsCommon_DAL_Hotel _objCmnmDH = null;
        void AddLogs(string sLogString)
        {
            sLogs.Append(Environment.NewLine + DateTime.Now.ToString() + Environment.NewLine + sLogString + Environment.NewLine);
        }
        //Error objError = new Error();
        //string LogDir = "Common";


        #region Transfer Amount
        [AcceptVerbs("POST")]
        [HttpPost()]
        [ActionName("Transfer")]
        public HttpResponseMessage Transfer(HttpRequestMessage request)
        {
            var req = this.Request.Content;
            requestContentType = req.Headers.ContentType.ToString();
            requestString = req.ReadAsStringAsync().Result;

            apiKey = Convert.ToString(request.Headers.GetValues("apikey").First());
            try
            {
                CompanyId = Convert.ToString(request.Headers.GetValues("CompanyID").First());

                Usertype = Convert.ToString(request.Headers.GetValues("Usertype").First());
            }
            catch (Exception ex)
            {
            }
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                var ctx = request.Properties["MS_HttpContext"] as HttpContextWrapper;
                if (ctx != null)
                {
                    requestIP = ctx.Request.UserHostAddress;
                    //do stuff with IP
                }
            }
            return GetTransfer(requestString);
        }

      
        [AcceptVerbs("POST")]
        [HttpPost()]
        public HttpResponseMessage GetTransfer(string request)
        {
            string response = string.Empty;
            try
            {
                // objError.IsWritelog = true;
                AddLogs("---Transfer---");
                if (request != null)
                {
                    AddLogs(string.Format("Common RQ in {0}:", requestContentType));
                    AddLogs(requestString);
                    clsNepalRequestProcessor objProcessor = new clsNepalRequestProcessor();
                    if (string.Compare(requestContentType, "application/json") == 0)
                        objProcessor.IsJson = true;
                    Models.NepalMoney.TransferRequest objRQ = new Models.NepalMoney.TransferRequest();
                    Models.NepalMoney.TransferResponse obj = new Models.NepalMoney.TransferResponse();
                    if (objProcessor.IsJson)
                    {
                        objRQ = JsonConvert.DeserializeObject<Models.NepalMoney.TransferRequest>(requestString);
                        Log.Error.LogLogin(Convert.ToString(CompanyId), Convert.ToString(requestString), "DMTNepalTransferRequest");
                    }
                    else
                    {
                        objRQ = XmlSerializers.DeserializeFromXml<Models.NepalMoney.TransferRequest>(requestString);
                    }
                    bool validate = ValidateApiuser(objRQ.TokenId, requestIP);
                    if (validate && Usertype=="0")
                    {
                        obj = objProcessor.btnRegister_Click_transfer(objRQ);
                        if (objProcessor.IsJson)
                        {
                            response = JsonConvert.SerializeObject(obj);
                            string json = Convert.ToString(response);
                            Log.Error.LogLogin(Convert.ToString(CompanyId), Convert.ToString(json), "DMTNepalTransferResponse");
                            return new HttpResponseMessage { Content = new StringContent("{\"Response\": { \"Status\": \"Success\",\"Message\": \"successfully\" },\"Data\":" + json + "}", System.Text.Encoding.UTF8, "application/json") };

                        }
                    }
                    else
                    {
                        return new HttpResponseMessage { Content = new StringContent("{\"Response\": { \"Status\": \"Fail\",\"Message\": \"Ooops!! Login again.\" }}", System.Text.Encoding.UTF8, "application/json") };

                    }

                    AddLogs("Auth Response:" + response);
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                AddLogs(string.Format("In catch block(GetHotels):{0}{1}:{2} ", ex.Message, Environment.NewLine, ex.StackTrace));
                return null;
            }
            finally
            {
                // objError.LogError(_companyId, "", sLogs.ToString(), HotelMethods.GetHotels.ToString());
            }

            //   return new HttpResponseMessage { Content = new StringContent("{\"GetStatus\": { \"Status\": \"Success\",\"Message\": \"successfully\" },\"Data\":" + json + "}", System.Text.Encoding.UTF8, "application/json") };
            return null;
        }



        #endregion
        #region ValidateApiuser
        public bool ValidateApiuser(string tokenId, string IPAddress)
        {
            bool validate = false;

            clsRequestProcessor objProcessor = new clsRequestProcessor();
            validate = objProcessor.ValidateToken(tokenId, IPAddress, CompanyId);

            return validate;


        }
        #endregion

    }
}
