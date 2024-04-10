using DMTServices.Models.Recharge;
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
    public class BillFetchController : ApiController
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
        // ClsCommon_DAL_Hotel _objCmnmDH = null;
        void AddLogs(string sLogString)
        {
            sLogs.Append(Environment.NewLine + DateTime.Now.ToString() + Environment.NewLine + sLogString + Environment.NewLine);
        }
        //Error objError = new Error();
        //string LogDir = "Common";
        

        #region Search Nummer
        [AcceptVerbs("POST")]
        [HttpPost()]
        [ActionName("BillFetch")]
        public HttpResponseMessage BillFetch(HttpRequestMessage request)
        {
            var req = this.Request.Content;
            requestContentType = req.Headers.ContentType.ToString();
            requestString = req.ReadAsStringAsync().Result;
            apiKey = Convert.ToString(request.Headers.GetValues("apikey").First());
            CompanyId = Convert.ToString(request.Headers.GetValues("CompanyID").First());
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                var ctx = request.Properties["MS_HttpContext"] as HttpContextWrapper;
                if (ctx != null)
                {
                    requestIP = ctx.Request.UserHostAddress;
                    //do stuff with IP
                }
            }
            return GetBillFetch(requestString);
        }


        // public HttpResponseMessage GetHotels([FromBody]Hotel_RQ request)
        [AcceptVerbs("POST")]
        [HttpPost()]
        public HttpResponseMessage GetBillFetch(string request)
        {
            string response = string.Empty;
            try
            {
                // objError.IsWritelog = true;
                AddLogs("---Create Token---");
                if (request != null && (apiKey != null || apiKey != "") && requestIP != "")
                {
                    AddLogs(string.Format("Common RQ in {0}:", requestContentType));
                    AddLogs(requestString);
                    clsRechargeProcessor objProcessor = new clsRechargeProcessor();
                    if (string.Compare(requestContentType, "application/json") == 0)
                        objProcessor.IsJson = true;
                    SearchRequest objRQ = new SearchRequest();
                    SearchResponse obj = new SearchResponse();
                    if (objProcessor.IsJson)
                    {
                        objRQ = JsonConvert.DeserializeObject<SearchRequest>(requestString);
                        Log.Error.LogLogin(Convert.ToString(CompanyId), Convert.ToString(requestString), "BILLFETCHRequest");
                    }
                    else
                    {
                        objRQ = XmlSerializers.DeserializeFromXml<SearchRequest>(requestString);
                    }

                    bool validate = true;// ValidateApiuser(objRQ.TokenId, requestIP);
                    if (validate)
                    {
                        ClsMobileRecharge _objRecharge = new ClsMobileRecharge();
                        
                        RechargeBillFatch _objResp = new RechargeBillFatch();
                        _objRecharge.Amount = objRQ.Amount;
                        _objRecharge.bbpsrefid = objRQ.udf2;
                        _objRecharge.BillUitPay = objRQ.udf3;
                        _objRecharge.CompanyId = objRQ.CompanyID;
                        _objRecharge.customername = objRQ.udf4;
                        _objRecharge.duedate = objRQ.udf5;
                        _objRecharge.Operator_Code = objRQ.operatorcode;
                        _objRecharge.Operator_Name = objRQ.operatorname;
                        _objRecharge.ServiceType = objRQ.billertype;                       
                        _objRecharge.Recharge_Number = objRQ.rechargenumber;
                        _objRecharge.emailid = objRQ.emailid;
                        _objRecharge.RemitterMobile = objRQ.RemitterMobile;

                        if (_objRecharge.ServiceType.ToUpper() == "INSURANCE")
                            _objResp = objProcessor.GetMplanLIC(_objRecharge);
                        else if (objRQ.CompanyID != "DMT781708rk")
                            _objResp = objProcessor.GetInvoiceBBPSPAYU(_objRecharge);
                        else if (objRQ.CompanyID != "DMT781708rk")
                            _objResp = objProcessor.GetPJBILLFetch(_objRecharge);
                        else
                            _objResp = objProcessor.GetInvoiceBBPSLevin(_objRecharge);



                        //_objResp = objProcessor.GetInvoiceBBPS(_objRecharge.Operator_Code, "1", _objRecharge.Recharge_Number);
                        if (objProcessor.IsJson)
                        {
                            response = JsonConvert.SerializeObject(_objResp);
                            string json = Convert.ToString(response);
                            Log.Error.LogLogin(Convert.ToString(CompanyId), Convert.ToString(json), "BILLFETCHResponse");
                            return new HttpResponseMessage { Content = new StringContent("{\"Response\": { \"Status\": \"Success\",\"Message\": \"successfully\" },\"Data\":" + json + "}", System.Text.Encoding.UTF8, "application/json") };

                        }
                        else {
                            return new HttpResponseMessage { Content = new StringContent("{\"Response\": { \"Status\": \"Fail\",\"Message\": \"S\" }}", System.Text.Encoding.UTF8, "application/json") };

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
                return new HttpResponseMessage { Content = new StringContent("{\"Response\": { \"Status\": \"Fail\",\"Message\": \"Error in Getting Reponse Please try after some time\" }}", System.Text.Encoding.UTF8, "application/json") };

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
        public bool ValidateApiuser(string tokenId,string IPAddress)
        {
            bool validate = false;

            clsRechargeProcessor objProcessor = new clsRechargeProcessor();
            validate = objProcessor.ValidateToken(tokenId, IPAddress, CompanyId);

            return validate;


        }
        #endregion

    }
}
