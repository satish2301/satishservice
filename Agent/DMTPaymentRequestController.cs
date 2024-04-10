using DMTServices.Models.Common;
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
    public class DMTPaymentRequestController : ApiController
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
        #region genrateToken
        [AcceptVerbs("POST")]
        [HttpPost()]
        [ActionName("PaymentRequest")]
        public HttpResponseMessage Token(HttpRequestMessage request)
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
            return GetTokens(requestString);
        }


        // public HttpResponseMessage GetHotels([FromBody]Hotel_RQ request)
        [AcceptVerbs("POST")]
        [HttpPost()]
        public HttpResponseMessage GetTokens(string request)
        {
            string response = string.Empty;
            try
            {
                // objError.IsWritelog = true;
                AddLogs("---Create Token---");
                if ((apiKey != null || apiKey != "") && requestIP != "")
                {

                    AddLogs(string.Format("Common RQ in {0}:", requestContentType));
                    AddLogs(requestString);
                    clsRequestProcessor objProcessor = new clsRequestProcessor();
                    if (string.Compare(requestContentType, "application/json") == 0)
                        objProcessor.IsJson = true;


                    PaymentDepositeRequest objRQ = new PaymentDepositeRequest();
                    PaymentDepositeResponse obj = new PaymentDepositeResponse();
                    if (objProcessor.IsJson)
                    {
                        objRQ = JsonConvert.DeserializeObject<PaymentDepositeRequest>(requestString);
                    }
                    else
                    {
                        objRQ = XmlSerializers.DeserializeFromXml<PaymentDepositeRequest>(requestString);
                    }
                    Log.Error.LogLogin(Convert.ToString(objRQ.CompanyID), Convert.ToString(requestString), "DMTPaymentreqRequest");


                    //objRQ.CompanyID = CompanyId;
                    bool validate = ValidateApiuser(objRQ.TokenId, requestIP, objRQ.CompanyID);
                    if (validate)
                    {
                        if (Convert.ToDouble( objRQ.DepositedAmount) > 999 && objRQ.DepositedAmount.Length<7)
                        {
                            if (objRQ.PaymentMode == "ADVANCE CREDIT")
                            {
                                objRQ.AccountNumber = "Partner" + "-" + "Partner";
                                objRQ.DepositedBank = "Select Bank";
                                objRQ.ChequeImgPath = "https://kyc.dialmytrip.com/DMT1MP257/A303FC572F.jpeg";// objProcessor.Base64ToImage(objRQ.ChequeImgPath, objRQ.CompanyID); // remove on live
                            }
                            else
                            {
                                objRQ.AccountNumber = objRQ.DepositedBank + "-" + objRQ.AccountNumber;
                                //objRQ.ChequeImgPath = "https://kyc.dialmytrip.com/DMT1MP257/slip0206202305210618A1444E-7E64-4801-9223-07A303FC572F.jpeg";// objProcessor.Base64ToImage(objRQ.ChequeImgPath, objRQ.CompanyID); // remove on live

                                objRQ.ChequeImgPath = objProcessor.Base64ToImage(objRQ.ChequeImgPath, objRQ.CompanyID); // remove on live
                            }
                            objRQ.Payment_status = "Pending";
                            objRQ.OurAccount = "yes";
                            objRQ.PaybackMode = "1";
                            objRQ.Created_Date = DateTime.Now.ToString();
                            objRQ.Modified_Date = DateTime.Now.ToString();
                            objRQ.Created_By = "";
                            objRQ.Modified_By = "";

                            

                            obj = objProcessor.InsertPaymentDeposite(objRQ);
                        }
                        else
                        {
                            return new HttpResponseMessage { Content = new StringContent("{\"Response\": { \"Status\": \"Fail\",\"Message\": \"Deposit Amount Cannnot be less than 1000 rs/-\" }}", System.Text.Encoding.UTF8, "application/json") };

                        }
                        if (obj != null)
                        {
                            response = JsonConvert.SerializeObject(obj);
                            string json = Convert.ToString(response);
                            Log.Error.LogLogin(Convert.ToString(objRQ.CompanyID), Convert.ToString(json), "DMTPaymentreqResponse");

                            return new HttpResponseMessage { Content = new StringContent("{\"Response\": { \"Status\": \"Success\",\"Message\": \"successfully\" },\"Data\":" + json + "}", System.Text.Encoding.UTF8, "application/json") };

                        }
                        else
                        {
                            return new HttpResponseMessage { Content = new StringContent("{\"Response\": { \"Status\": \"Fail\",\"Message\": \"Report Not genrated\" }}", System.Text.Encoding.UTF8, "application/json") };

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
        public bool ValidateApiuser(string tokenId, string IPAddress, string CompanyId)
        {
            bool validate = false;

            clsRequestProcessor objProcessor = new clsRequestProcessor();
            validate = objProcessor.ValidateToken(tokenId, IPAddress, CompanyId);

            return validate;


        }
        #endregion    

    }
}
