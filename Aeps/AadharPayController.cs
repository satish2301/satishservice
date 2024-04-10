using DMTServices.Models.AEPS;
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
using DMTServices.Models.Common;
namespace DMTServices.Controllers
{
    public class AadharPayController : ApiController
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
        // ClsCommon_DAL_Hotel _objCmnmDH = null;
        void AddLogs(string sLogString)
        {
            sLogs.Append(Environment.NewLine + DateTime.Now.ToString() + Environment.NewLine + sLogString + Environment.NewLine);
        }
        //Error objError = new Error();
        //string LogDir = "Common";
        #region AadharPay
        [AcceptVerbs("POST")]
        [HttpPost()]
        [ActionName("AepsSearch")]
        public HttpResponseMessage balanceinquiry(HttpRequestMessage request)
        {
            var req = this.Request.Content;
            requestContentType = req.Headers.ContentType.ToString();
            requestString = req.ReadAsStringAsync().Result;
            apiKey = Convert.ToString(request.Headers.GetValues("apikey").First());
            //CompanyId = Convert.ToString(request.Headers.GetValues("CompanyID").First());
           // Password = Convert.ToString(request.Headers.GetValues("ApiPassword").First());
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                var ctx = request.Properties["MS_HttpContext"] as HttpContextWrapper;
                if (ctx != null)
                {
                    requestIP = ctx.Request.UserHostAddress;
                    //do stuff with IP
                }
            }
            return withdrawal(requestString);
        }


        // public HttpResponseMessage GetHotels([FromBody]Hotel_RQ request)
        [AcceptVerbs("POST")]
        [HttpPost()]
        public HttpResponseMessage withdrawal(string request)
        {
            string response = string.Empty;
            try
            {
                // objError.IsWritelog = true;
                AddLogs("---Create Token---");
                if ((apiKey!=null || apiKey!="") && requestIP!="")
                {
                  
                    AddLogs(string.Format("Common RQ in {0}:", requestContentType));
                    AddLogs(requestString);
                    clsAEPSRequestProcessor objProcessor = new clsAEPSRequestProcessor();
                   
                    if (string.Compare(requestContentType, "application/json") == 0)
                        objProcessor.IsJson = true;


                    BalanceInquiryRequest objRQ = new BalanceInquiryRequest();
                    BalanceInquiryResponse obj = new BalanceInquiryResponse();
                    if (objProcessor.IsJson)
                    {
                        objRQ = JsonConvert.DeserializeObject<BalanceInquiryRequest>(requestString);
                    }
                    else
                    {
                       // objRQ = XmlSerializers.DeserializeFromXml<TokenRequest>(requestString);
                    }


                    // objRQ.ApiPassword
                    CompanyId = objRQ.CompanyID;
                    Log.Error.LogLogin(Convert.ToString(CompanyId), Convert.ToString(requestString), "DMTAEPSREQAadharPay");
                    bool validate = ValidateApiuser(objRQ.TokenID, requestIP);
                    validate = false;
                    if (validate)
                    {
                        if (Convert.ToDouble(objRQ.Amount) > 99)
                            obj = objProcessor.GetAadharPay(objRQ);
                        else
                        {
                            obj.errorcode = "2";
                            obj.errormessage = "Amount should not be less than Rs.100 ";
                            obj.Amount = objRQ.Amount;
                            obj.BankName = objRQ.bank;
                            obj.AadharNo = objRQ.aadhaar;
                            obj.Rembalance = "";
                            obj.Remark = "";
                            obj.BankRefNo = "";
                        }
                    }
                    else
                    {
                        obj.errorcode = "2";
                        obj.errormessage = "Try Later !! ";
                        obj.Amount = objRQ.Amount;
                        obj.BankName = objRQ.bank;
                        obj.AadharNo = objRQ.aadhaar;
                        obj.Rembalance = "";
                        obj.Remark = "";
                        obj.BankRefNo = "";
                    }
                    if (objProcessor.IsJson)
                    {
                        response = JsonConvert.SerializeObject(obj);
                        string json = Convert.ToString(response);
                        Log.Error.LogLogin(Convert.ToString(objRQ.CompanyID), Convert.ToString(json), "DMTAEPSRESAadharPay");
                        return new HttpResponseMessage { Content = new StringContent("{\"Response\": { \"Status\": \"Success\",\"Message\": \"successfully\" },\"Data\":" + json + "}", System.Text.Encoding.UTF8, "application/json") };

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
