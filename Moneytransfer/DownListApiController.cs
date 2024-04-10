﻿using DMTServices.Models.Common;
using Newtonsoft.Json;
using Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using TMC.Controllers;

namespace DMTServices.Controllers
{
    public class DownListApiController : ApiController
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
        // ClsCommon_DAL_Hotel _objCmnmDH = null;
        void AddLogs(string sLogString)
        {
            sLogs.Append(Environment.NewLine + DateTime.Now.ToString() + Environment.NewLine + sLogString + Environment.NewLine);
        }
        //Error objError = new Error();
        //string LogDir = "Common";


        #region GetDownBankList
        [AcceptVerbs("POST")]
        [HttpPost()]
        [ActionName("DownBankList")]
        public HttpResponseMessage DownBankList(HttpRequestMessage request)
        {
            var req = this.Request.Content;
            requestContentType = req.Headers.ContentType.ToString();
            requestString = req.ReadAsStringAsync().Result;
            return GetDownBankList(requestString);
        }


        [AcceptVerbs("POST")]
        [HttpPost()]
        public HttpResponseMessage GetDownBankList(string request)
        {
            string response = string.Empty;
            try
            {
                // objError.IsWritelog = true;
                AddLogs("---Create Token---");
                if (request != null)
                {
                    AddLogs(string.Format("Common RQ in {0}:", requestContentType));
                    AddLogs(requestString);
                    clsRequestProcessor objProcessor = new clsRequestProcessor();
                    if (string.Compare(requestContentType, "application/json") == 0)
                        objProcessor.IsJson = true;
                    TokenRequest objRQ = new TokenRequest();
                    TokenReponse obj = new TokenReponse();
                    if (objProcessor.IsJson)
                    {
                        objRQ = JsonConvert.DeserializeObject<TokenRequest>(requestString);
                    }
                    else
                    {
                        objRQ = XmlSerializers.DeserializeFromXml<TokenRequest>(requestString);
                    }
                    obj = objProcessor.TokenGenrat(objRQ);
                    if (objProcessor.IsJson)
                    {
                        response = JsonConvert.SerializeObject(obj);
                        string json = Convert.ToString(response);
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


    }
}
