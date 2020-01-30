using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ApplicationInsightsFunctions
{
    public static class TrackEvent
    {
        [FunctionName("TrackEvent")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                dynamic data = await req.Content.ReadAsAsync<object>();
                string eventName = data?.Name;
                string eventProp = data?.Prop;
                var eventProperties = new Dictionary<string, string>();
                eventProperties.Add("Prop", eventProp);

                if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(eventProp))
                {
                    return req.CreateResponse(HttpStatusCode.BadRequest, "No Name or Prop");
                }

                string instrumentationKey = data?.InstrumentationKey;
                AIUtility.TrackEvent(eventName, eventProperties, instrumentationKey);
                return req.CreateResponse(HttpStatusCode.OK, "Succeeded");
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return req.CreateResponse(HttpStatusCode.InternalServerError, "Failed");
            }
        }
    }

    public static class TrackPageView
    {
        [FunctionName("TrackPageView")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                dynamic data = await req.Content.ReadAsAsync<object>();

                string pageName = data?.Name;

                if (string.IsNullOrEmpty(pageName))
                {
                    return req.CreateResponse(HttpStatusCode.BadRequest, "No Name");
                }

                string instrumentationKey = data?.InstrumentationKey;
                AIUtility.TrackPageView(pageName, instrumentationKey);
                return req.CreateResponse(HttpStatusCode.OK, "Succeeded");
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return req.CreateResponse(HttpStatusCode.InternalServerError, "Failed");
            }
        }
    }

    public static class TrackException
    {
        [FunctionName("TrackException")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                #region Get the data from the payload
                dynamic data = await req.Content.ReadAsAsync<object>();

                string message = data?.Message;
                string source = data?.Source;
                string correlationId = data?.CorrelationId;
                string appName = data?.AppName;
                string environment = data?.Environment;
                string userAlias = data?.UserAlias;
                string errorCode = data?.ErrorCode;
                #endregion

                #region Check all required information is present
                if (string.IsNullOrEmpty(message))
                {
                    return req.CreateResponse(HttpStatusCode.BadRequest, "No Message");
                }

                if (string.IsNullOrEmpty(source))
                {
                    return req.CreateResponse(HttpStatusCode.BadRequest, "No Source");
                }

                if (string.IsNullOrEmpty(correlationId))
                {
                    return req.CreateResponse(HttpStatusCode.BadRequest, "No CorrelationId");
                }

                if (string.IsNullOrEmpty(appName))
                {
                    return req.CreateResponse(HttpStatusCode.BadRequest, "No AppName");
                }

                if (string.IsNullOrEmpty(environment))
                {
                    return req.CreateResponse(HttpStatusCode.BadRequest, "No Environment");
                }

                if (string.IsNullOrEmpty(userAlias))
                {
                    return req.CreateResponse(HttpStatusCode.BadRequest, "No UserAlias");
                }

                if (string.IsNullOrEmpty(errorCode))
                {
                    return req.CreateResponse(HttpStatusCode.BadRequest, "No ErrorCode");
                }
                #endregion

                //Create the Dictionary of properties to add to the Exception
                var properties = new Dictionary<string, string> {
                    { "CorrelationId", correlationId},
                    { "PowerApp Name", appName},
                    { "Environment", environment},
                    { "User Alias", userAlias},
                    { "Error Code", errorCode}
                };

                //Get the Instrumentation Key
                string instrumentationKey = data?.InstrumentationKey;

                //Log the Exception to AI
                AIUtility.TrackException(source, message, instrumentationKey, properties);

                //Respond with a message indicating success
                return req.CreateResponse(HttpStatusCode.OK, "Succeeded");
            }
            catch (Exception ex)
            {
                //Log the error
                log.Error(ex.Message);
                //Respond with a message indicating failure
                return req.CreateResponse(HttpStatusCode.InternalServerError, "Failed");
            }
        }
    }
}
