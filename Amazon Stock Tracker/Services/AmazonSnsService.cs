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
using Amazon;
using Amazon.Runtime.CredentialManagement;
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

        public AmazonSnsService(string phoneNumber, string awsRegion, string smsSenderId, string smsType, 
            string smsMaxPrice, string smsMonthlySpendLimit, string awsProfile = "default") // TODO: maybe make different class for profile code.
        {
            var chain = new CredentialProfileStoreChain();

            _phoneNumber = phoneNumber;
            _snsClient = chain.TryGetAWSCredentials(awsProfile, out var awsCredentials) ?
                new AmazonSimpleNotificationServiceClient(credentials: awsCredentials, region: RegionEndpoint.GetBySystemName(awsRegion)) : 
                new AmazonSimpleNotificationServiceClient(region: RegionEndpoint.GetBySystemName(awsRegion));
            _smsSenderId = smsSenderId;
            _smsType = smsType;
            _smsMaxPrice = smsMaxPrice;
            _smsMonthlySpendLimit = smsMonthlySpendLimit;
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

            await SetDefaultSmsAttributesAsync(_snsClient);

            var response = await _snsClient.PublishAsync(pubRequest);

            return response.MessageId;
        }

        private async Task SetDefaultSmsAttributesAsync(AmazonSimpleNotificationServiceClient snsClient)
        {
            var setRequest = new SetSMSAttributesRequest
            {
                Attributes =
                {
                    ["MonthlySpendLimit"] = _smsMonthlySpendLimit, // TODO: check if previous value exists before setting.
                }
            };

            SetSMSAttributesResponse setResponse = await snsClient.SetSMSAttributesAsync(setRequest); // TODO: Handle the response.
        }

        public void Dispose()
        {
            _snsClient?.Dispose();
        }
    }
}
