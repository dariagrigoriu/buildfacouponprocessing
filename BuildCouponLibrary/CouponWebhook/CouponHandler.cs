﻿using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;


namespace BuildCouponLibrary
{
    public static class CouponHandler
    {
        public static async Task<object> Run(HttpRequestMessage req, TraceWriter log, IAsyncCollector<String> eventOutput)
        {
            log.Info($"Coupon processing webhook was triggered!");

            string jsonContent = await req.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(jsonContent);

            String couponValue = null;
            if (data.time == null)
            {
                couponValue = "$5 off your purchase";
            }
            else
            {
                couponValue = getCoupon(Convert.ChangeType(data.time, typeof(DateTime)));
            }

            await eventOutput.AddAsync(
                JsonConvert.SerializeObject(getLogData(couponValue, data)));
            return req.CreateResponse(HttpStatusCode.OK, new { coupon = couponValue });
        }

        private static dynamic getLogData(String couponValue, dynamic requestData) {
            return new {
                type = "coupon",
                coupon = couponValue,
                payload = requestData
            };
        }

        private static string getCoupon(DateTime time)
        {
            if (time.Hour >= 22 || time.Hour < 11)
            {
                return "Morning special: free coffee with purchase";
            }
            else if (time.Hour <= 14)
            {
                return "Lunch special: buy 1 pizza get 1 pizza free";
            }
            else
            {
                return "Dinner special: buy 1 pizza get 1 salad free";
            }
        }
    }
}
