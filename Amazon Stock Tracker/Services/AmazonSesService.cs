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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime.CredentialManagement;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Amazon.SimpleNotificationService;

namespace Amazon_Stock_Tracker.Services
{
    class AmazonSesService : INotificationService
    {
        private readonly string _email; // Must be verified with Amazon SES in Sandbox mode.
        private readonly AmazonSimpleEmailServiceV2Client _sesClient;

        public AmazonSesService(string email, string awsRegion, string awsProfile = "default") // TODO: maybe make different class for profile code.
        {
            var chain = new CredentialProfileStoreChain();

            _email = email;
            _sesClient = chain.TryGetAWSCredentials(awsProfile, out var awsCredentials) ?
                new AmazonSimpleEmailServiceV2Client(credentials: awsCredentials, region: RegionEndpoint.GetBySystemName(awsRegion)) :
                new AmazonSimpleEmailServiceV2Client(region: RegionEndpoint.GetBySystemName(awsRegion));
        }

        public async Task<string> SendNotificationAsync(string msg)
        {
            var sendRequest = new SendEmailRequest
            {
                FromEmailAddress = _email,
                Destination = new Destination
                {
                    ToAddresses = new List<string> { _email } // Sender and Recipient are the same in this use case.
                },
                Content = new EmailContent
                {
                    Simple = new Message
                    {
                        Subject = new Content{ Charset = "UTF-8", Data = "Amazon Stock Tracker Notification" },
                        Body = new Body
                        {
                            Text = new Content
                            {
                                Charset = "UTF-8",
                                Data = msg
                            }
                        }
                    }
                }
            };

            var response = await _sesClient.SendEmailAsync(sendRequest);

            return response.MessageId;
        }

        public void Dispose()
        {
            _sesClient?.Dispose();
        }
    }
}
