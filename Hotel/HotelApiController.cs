using System;
using System.Net.Http;
using System.Web.Http;
//using Hotel.CommonEntity.RQRS;
//using Hotel.CommonEntity.RS;
using System.Text;
//using RequestProcessor;
using Newtonsoft.Json;
using TMC.Controllers;
using Serializers;

namespace DMTServices.Controllers
{
    public class HotelApiController : ApiController
    {
        string CID = string.Empty;
        string requestString = string.Empty;
        string requestContentType = string.Empty;
        StringBuilder sLogs = new StringBuilder();

        //  ClsCommon_DAL_Hotel _objCmnmDH = null;
        void AddLogs(string sLogString)
        {
            sLogs.Append(Environment.NewLine + DateTime.Now.ToString() + Environment.NewLine + sLogString + Environment.NewLine);
        }
        [HttpPost()]
        public HttpResponseMessage GetHotels(HttpRequestMessage request)
        {
            var req = this.Request.Content;
            requestContentType = req.Headers.ContentType.ToString();
            requestString = req.ReadAsStringAsync().Result;
            return GetHotels(requestString);
        }
        [AcceptVerbs("POST")]
        [HttpPost()]
        // public HttpResponseMessage GetHotels([FromBody]Hotel_RQ request)
        public HttpResponseMessage GetHotels(string request)
        {
            try
            {
                AddLogs("---Hotel Search Start---");
                if (request != null)
                {
                    AddLogs(string.Format("Common Search RQ in {0}:", requestContentType));
                    AddLogs(requestString);
                    Processor objProcessor = new Processor();
                    if (string.Compare(requestContentType, "application/json") == 0)
                        objProcessor.IsJson = true;
                    HotelRQ objRQ = new HotelRQ();
                    HotelResponse obj = new HotelResponse();
                    if (objProcessor.IsJson)
                    {
                        objRQ = JsonConvert.DeserializeObject<HotelRQ>(requestString);
                    }
                    else
                    {
                        objRQ = XmlSerializers.DeserializeFromXml<HotelRQ>(requestString);
                    }
                    //Hotel_RQ requestObj=
                    // return new HttpResponseMessage(HttpStatusCode.OK) { new StringContent(SerializedString, System.Text.Encoding.UTF8, "application/json") };
                   // obj = objProcessor.SearchHotel();
                    return new HttpResponseMessage { Content = new StringContent("[{\"Success\":\"Success\"},{\"Message\":\"Login successfully\"}],{\"Data\":" + obj + "}", System.Text.Encoding.UTF8, "application/json") };
                }
                else
                    return new HttpResponseMessage { Content = new StringContent("[{\"Success\":\"Fail\"},{\"Message\":\"Login Fail\"}]", System.Text.Encoding.UTF8, "application/json") };
            }
            catch (Exception ex)
            {
                AddLogs(string.Format("In catch block(GetHotels):{0}{1}:{2} ", ex.Message, Environment.NewLine, ex.StackTrace));
                return new HttpResponseMessage { Content = new StringContent("[{\"Success\":\"Fail\"},{\"Message\":\"Login Fail\"}]", System.Text.Encoding.UTF8, "application/json") };
            }
            finally
            {
                AddLogs("---Hotel Search End---");
            }

        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
