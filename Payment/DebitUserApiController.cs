using DMTServices.Models.Bus;
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
    public class DebitUserController : ApiController
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
        string requestIP = "";
        string CompanyId = "";
        string Password = "";
        string deviceID = "";
        string Channel = "";
        // ClsCommon_DAL_Hotel _objCmnmDH = null;
        void AddLogs(string sLogString)
        {
            sLogs.Append(Environment.NewLine + DateTime.Now.ToString() + Environment.NewLine + sLogString + Environment.NewLine);
        }
        //Error objError = new Error();
        //string LogDir = "Common";
        #region genrateToken
        [AcceptVerbs("POST")]
        [HttpPost()]
        [ActionName("DebitUser")]
        public HttpResponseMessage DebitUser(HttpRequestMessage request)
        {
            var req = this.Request.Content;
            requestContentType = req.Headers.ContentType.ToString();
            requestString = req.ReadAsStringAsync().Result;
            apiKey = Convert.ToString(request.Headers.GetValues("apikey").First());           
            try
            {
                Channel = Convert.ToString(request.Headers.GetValues("Channel").First());
                deviceID = Convert.ToString(request.Headers.GetValues("DeviceID").First());
            }
            catch (Exception ex)
            {
            }
            Log.Error.LogLogin(Convert.ToString(CompanyId), Convert.ToString(requestString), "DMTLoginRequest");
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                var ctx = request.Properties["MS_HttpContext"] as HttpContextWrapper;
                if (ctx != null)
                {
                    requestIP = ctx.Request.UserHostAddress;
                    //do stuff with IP
                }
            }
            return DebitUser(requestString);
        }


        // public HttpResponseMessage GetHotels([FromBody]Hotel_RQ request)
        [AcceptVerbs("POST")]
        [HttpPost()]
        public HttpResponseMessage DebitUser(string request)
        {
            string response = string.Empty;
            try
            {
                // objError.IsWritelog = true;
                AddLogs("---Create Token---");
                if ((apiKey != null || apiKey != "") && requestIP != "")
                {
                    bool validate = ValidateApiuser(apiKey, requestIP);

                    AddLogs(string.Format("Common RQ in {0}:", requestContentType));
                    AddLogs(requestString);
                    clsPaymentRequestProcessor objProcessor = new clsPaymentRequestProcessor();
                    if (string.Compare(requestContentType, "application/json") == 0)
                        objProcessor.IsJson = true;


                    DebitUserRequest objRQ = new DebitUserRequest();
                    DebitUserReponse obj = new DebitUserReponse();
                    if (objProcessor.IsJson)
                    {
                         objRQ = JsonConvert.DeserializeObject<DebitUserRequest>(requestString);
                    }
                    else
                    {
                         objRQ = XmlSerializers.DeserializeFromXml<DebitUserRequest>(requestString);
                    }
                    if(Channel.ToLower()=="app")
                         objRQ.IPAddress = deviceID;
                    else
                        objRQ.IPAddress = requestIP;
                   
                    objRQ.EmailID = objRQ.EmailID;
                    // objRQ.ApiPassword
                    if (validate)
                    {
                        obj = objProcessor.DebitAPI(objRQ);
                        if (obj != null && validate == true)
                        {
                            response = JsonConvert.SerializeObject(obj);
                            string json = Convert.ToString(response);
                            Log.Error.LogLogin(Convert.ToString(CompanyId), Convert.ToString(json), "DMTLoginResponse");
                            return new HttpResponseMessage { Content = new StringContent("{\"Response\": { \"Status\": \"Success\",\"Message\": \"successfully\" },\"Data\":" + json + "}", System.Text.Encoding.UTF8, "application/json") };

                        }
                        else
                        {
                            return new HttpResponseMessage { Content = new StringContent("{\"Response\": { \"Status\": \"Fail\",\"Message\": \"User Not Found\" },\"Data\":" + "{ \"errorMsg\": \"Invalid User Credentials\", \"errorCode\": \"02\" }" + "}", System.Text.Encoding.UTF8, "application/json") };
                        }
                    }
                    else
                    {
                        return new HttpResponseMessage { Content = new StringContent("{\"Response\": { \"Status\": \"Fail\",\"Message\": \"Ooops!! Login again.\" },\"Data\":" + "{ \"errorMsg\": \"Invalid User Credentials\", \"errorCode\": \"02\" }" + "}", System.Text.Encoding.UTF8, "application/json") };
                    }

                    AddLogs("Auth Response:" + response);
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                AddLogs(string.Format("In catch block(GETTOKEN):{0}{1}:{2} ", ex.Message, Environment.NewLine, ex.StackTrace));
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
        public bool ValidateApiuser(string tokenId,string ipaddress)
        {
            bool validate = false;
            if(tokenId == "Njk1MjAyMC0wMi0xMSAxMTozNDoyMQ==")
            {

                
                validate = true;
            }


            return validate;


        }
        #endregion    

    }
}
