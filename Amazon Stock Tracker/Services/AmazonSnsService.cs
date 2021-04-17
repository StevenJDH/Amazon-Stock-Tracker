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

        /// <summary>
        /// Constructs a new <see cref="AmazonSnsService"/> instance to send SMS notifications using the
        /// AWS SNS service.
        /// </summary>
        /// <param name="phoneNumber">The phone number that will receive the SMS notification.</param>
        /// <param name="smsSenderId">
        /// A custom ID that contains 3-11 alphanumeric characters, including at least one letter and
        /// no spaces. The sender ID is displayed as the message sender on the receiving device.
        /// </param>
        /// <param name="smsType">
        /// Defines the type of messages being sent. Recommended to set as 'Promotional'.
        /// </param>
        /// <param name="smsMaxPrice">
        /// The maximum amount in USD that you are willing to spend to send the SMS message.
        /// </param>
        /// <param name="smsMonthlySpendLimit">
        /// The maximum amount in USD that you are willing to spend each month to send SMS messages.
        /// The default limit in your AWS account is 1, and if you set it to a higher value, it will
        /// cause an error as it exceeds this hard limit unless a request is made to AWS for a raise.
        /// </param>
        /// <param name="serviceAccess">
        /// <see cref="AmazonServiceAccess"/> instance containing access details.
        /// </param>
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

        /// <summary>
        /// Sends a notification message to the AWS SNS service asynchronously.
        /// </summary>
        /// <param name="msg">Message to send.</param>
        /// <returns>Unique identifier assigned to the message sent.</returns>
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

            await SetDefaultSmsAttributesAsync().ConfigureAwait(false);

            var response = await _snsClient.PublishAsync(pubRequest);

            return response.MessageId;
        }

        /// <summary>
        /// Sets the default message attributes asynchronously.
        /// </summary>
        /// <remarks>
        /// The MonthlySpendLimit attribute will be set at the account level only if there is no
        /// explicitly defined value already set in the AWS account. All other attributes will be
        /// set at the message level.
        /// </remarks>
        /// <returns>A <see cref="Task"/> representing an async operation.</returns>
        private async Task SetDefaultSmsAttributesAsync()
        {
            var getResponse = await _snsClient.GetSMSAttributesAsync(new GetSMSAttributesRequest());

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

            var setResponse = await _snsClient.SetSMSAttributesAsync(setRequest);
            
            if (setResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                Debug.WriteLine($"Error: Got HTTP status code {(int)setResponse.HttpStatusCode} when setting MonthlySpendLimit.");
            }
        }

        /// <summary>
        /// Releases any unmanaged resources and disposes of the managed resources used
        /// by the <see cref="AmazonSnsService"/>.
        /// </summary>
        public void Dispose()
        {
            _snsClient?.Dispose();
        }
    }
}
