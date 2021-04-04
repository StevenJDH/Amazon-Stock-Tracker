/**
 * This file is part of Amazon Stock Tracker <https://github.com/StevenJDH/Amazon-Stock-Tracker>.
 * Copyright (C) 2021 Steven Jenkins De Haro.
 *
 * Amazon Stock Tracker is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * Amazon Stock Tracker is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Amazon Stock Tracker.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace Amazon_Stock_Tracker.Services
{
    sealed class AmazonSnsService : INotificationService
    {
        private readonly string _phoneNumber;
        private readonly AmazonSimpleNotificationServiceClient _snsClient;
        private readonly string _smsSenderId;
        private readonly string _smsType;
        private readonly string _smsMaxPrice;
        private readonly string _smsMonthlySpendLimit;

        public AmazonSnsService(string phoneNumber, string smsSenderId, string smsType, 
            string smsMaxPrice, string smsMonthlySpendLimit, IAmazonServiceAccess serviceAccess)
        {
            _phoneNumber = phoneNumber;
            _smsSenderId = smsSenderId;
            _smsType = smsType;
            _smsMaxPrice = smsMaxPrice;
            _smsMonthlySpendLimit = smsMonthlySpendLimit;
            _snsClient = new AmazonSimpleNotificationServiceClient(credentials: serviceAccess.GetCredentials(),
                region: serviceAccess.GetRegion());
        }

        public async Task<string> SendNotificationAsync(string msg)
        {
            var pubRequest = new PublishRequest
            {
                PhoneNumber = _phoneNumber,
                Message = msg,
                MessageAttributes =
                {
                    ["AWS.SNS.SMS.SMSType"] =
                        new MessageAttributeValue {StringValue = _smsType, DataType = "String"},
                    ["AWS.SNS.SMS.MaxPrice"] =
                        new MessageAttributeValue {StringValue = _smsMaxPrice, DataType = "Number"}
                }
            };

            if (!_smsSenderId.Equals("default", StringComparison.InvariantCultureIgnoreCase))
            {
                pubRequest.MessageAttributes["AWS.SNS.SMS.SenderID"] =
                    new MessageAttributeValue {StringValue = _smsSenderId, DataType = "String"};
            }

            await SetDefaultSmsAttributesAsync(_snsClient).ConfigureAwait(false);

            var response = await _snsClient.PublishAsync(pubRequest);

            return response.MessageId;
        }

        private async Task SetDefaultSmsAttributesAsync(AmazonSimpleNotificationServiceClient snsClient)
        {
            var getResponse = await snsClient.GetSMSAttributesAsync(new GetSMSAttributesRequest());

            if (getResponse.Attributes.TryGetValue("MonthlySpendLimit", out var value) &&
                !String.IsNullOrWhiteSpace(value))
            {
                return; // Use exiting value if one already exists instead of setting our own.
            }

            var setRequest = new SetSMSAttributesRequest
            {
                Attributes =
                {
                    ["MonthlySpendLimit"] = _smsMonthlySpendLimit
                }
            };

            var setResponse = await snsClient.SetSMSAttributesAsync(setRequest);
            
            if (setResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                Debug.WriteLine($"Error: Got HTTP status code {(int)setResponse.HttpStatusCode} when setting MonthlySpendLimit.");
            }
        }

        public void Dispose()
        {
            _snsClient?.Dispose();
        }
    }
}
