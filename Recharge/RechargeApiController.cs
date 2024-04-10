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
    public class RechargeController : ApiController
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
        string Channel = "";
        string Usertype = "0";
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
        [ActionName("Recharge")]
        public HttpResponseMessage Recharge(HttpRequestMessage request)
        {
            var req = this.Request.Content;
            requestContentType = req.Headers.ContentType.ToString();
            requestString = req.ReadAsStringAsync().Result;
            Log.Error.LogLogin(Convert.ToString("finalcheck"), Convert.ToString(requestString), "DMTRechargeRequest");
            apiKey = Convert.ToString(request.Headers.GetValues("apikey").First());
            CompanyId = Convert.ToString(request.Headers.GetValues("CompanyID").First());
            try
            {
                Channel = Convert.ToString(request.Headers.GetValues("Channel").First());
                Usertype = Convert.ToString(request.Headers.GetValues("Usertype").First());
                Log.Error.LogLogin(Convert.ToString("Usertype recharge"), Convert.ToString(" Usertype:" + Usertype), "DMTTransRequest");
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
            return GetRecharge(requestString);
        }


        // public HttpResponseMessage GetHotels([FromBody]Hotel_RQ request)
        [AcceptVerbs("POST")]
        [HttpPost()]
        public HttpResponseMessage GetRecharge(string request)
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
                        Log.Error.LogLogin(Convert.ToString(objRQ.CompanyID), Convert.ToString(requestString), "RechargeRequest");
                    }
                    else
                    {
                        objRQ = XmlSerializers.DeserializeFromXml<SearchRequest>(requestString);
                    }
                    CompanyId = objRQ.CompanyID;
                    bool validate = ValidateApiuser(objRQ.TokenId, requestIP);
                    //validate = true;
                    if (validate)
                    {
                        bool checkbalance = false;
                        bool service = true;
                        checkbalance = clsRechargeProcessor.GetAgentAvalable(objRQ.Amount, objRQ.CompanyID);
                        if (service)
                        {
                            if (checkbalance && objRQ.CompanyID.ToUpper() != "DMT43670802")
                            {
                                ClsMobileRecharge _objRecharge = new ClsMobileRecharge();
                                _objRecharge.Channel = Channel;// Convert.ToString(Checkrequest.Headers.GetValues("Channel").First());
                                _objRecharge.Amount = objRQ.Amount;
                                _objRecharge.bbpsrefid = objRQ.udf3;
                                _objRecharge.BillUitPay = objRQ.udf3;
                                _objRecharge.CompanyId = objRQ.CompanyID;
                                _objRecharge.customername = objRQ.udf3;
                                _objRecharge.duedate = objRQ.udf3;
                                _objRecharge.Operator_Code = objRQ.operatorcode.Split('_')[0];
                                _objRecharge.Operator_Name = objRQ.operatorname;
                                _objRecharge.ServiceType = objRQ.billertype;
                                _objRecharge.Recharge_Number = objRQ.rechargenumber;
                                _objRecharge.SessionId = objRQ.TokenId;
                                _objRecharge.RemitterMobile = objRQ.RemitterMobile;
                                _objRecharge.emailid = objRQ.emailid;
                                _objRecharge.mode = objRQ.operatorcode;
                                _objRecharge = objProcessor.Recharge(_objRecharge);
                                obj.Amount = _objRecharge.Amount;
                                obj.udf2 = _objRecharge.bbpsrefid;
                                obj.udf3 = _objRecharge.BillUitPay;
                                obj.CompanyID = _objRecharge.CompanyId;
                                obj.udf4 = _objRecharge.customername;
                                obj.udf5 = _objRecharge.duedate;
                                obj.operatorcode = _objRecharge.Operator_Code;
                                obj.operatorname = _objRecharge.Operator_Name;
                                obj.billertype = _objRecharge.ServiceType;
                                obj.client_ref_no = objRQ.client_ref_no;
                                obj.dmt_ref_no = _objRecharge.Request_Ref;
                                obj.rechargenumber = _objRecharge.Recharge_Number;
                                obj.DateTime = _objRecharge.TransactionTime;
                                obj.errorCode = _objRecharge.errorCode;
                                obj.errorMsg = _objRecharge.errorMsg;
                            }
                            else
                            {
                                obj.errorCode = "2";
                                obj.errorMsg = "Insufficient Fund";
                            }
                        }
                        else
                        {
                            obj.errorCode = "2";
                            obj.errorMsg = "This Service is down for 2 hours from Supplier End.";
                        }
                        if (objProcessor.IsJson)
                        {
                            response = JsonConvert.SerializeObject(obj);
                            string json = Convert.ToString(response);
                            Log.Error.LogLogin(Convert.ToString(objRQ.CompanyID), Convert.ToString(json), "RechargeResponse");
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
