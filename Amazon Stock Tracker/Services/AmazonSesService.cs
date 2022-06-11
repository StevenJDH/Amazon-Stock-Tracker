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
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Message = Amazon.SimpleEmailV2.Model.Message;

namespace Amazon_Stock_Tracker.Services
{
    sealed class AmazonSesService : INotificationService
    {
        private readonly string _email;
        private readonly AmazonSimpleEmailServiceV2Client _sesClient;

        /// <summary>
        /// Constructs a new <see cref="AmazonSesService"/> instance to send notifications
        /// using the AWS SES service.
        /// </summary>
        /// <param name="email">
        /// Email address of sender/recipient. Must be verified when AWS SES account is in
        /// Sandbox mode.
        /// </param>
        /// <param name="serviceAccess">
        /// <see cref="AmazonServiceAccess"/> instance containing access details.
        /// </param>
        public AmazonSesService(string email, IAmazonServiceAccess serviceAccess)
        {
            _email = email;
            _sesClient = new AmazonSimpleEmailServiceV2Client(credentials: serviceAccess.GetCredentials(), 
                region: serviceAccess.GetRegion());
        }

        /// <summary>
        /// Sends a notification message to the AWS SES service asynchronously.
        /// </summary>
        /// <param name="msg">Message to send.</param>
        /// <returns>Unique identifier assigned to the message sent.</returns>
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

        /// <summary>
        /// Releases any unmanaged resources and disposes of the managed resources used
        /// by the <see cref="AmazonSesService"/>.
        /// </summary>
        public void Dispose()
        {
            _sesClient?.Dispose();
        }
    }
}
