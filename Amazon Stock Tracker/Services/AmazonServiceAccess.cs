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
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;

namespace Amazon_Stock_Tracker.Services
{
    class AmazonServiceAccess : IAmazonServiceAccess
    {
        private readonly RegionEndpoint _region;
        private readonly AWSCredentials _awsCredentials;

        /// <summary>
        /// Constructs a new <see cref="AmazonServiceAccess"/> instance to work with AWS services.
        /// </summary>
        /// <param name="awsRegion">The region to use for the connection.</param>
        /// <param name="awsProfile">The name of the profile to get credentials from if not the 'default' one.</param>
        /// <exception cref="AmazonServiceException">
        /// Thrown when a custom and a default profile can't be found in <see cref="CredentialProfileStoreChain"/>CredentialProfileStoreChain.
        /// </exception>
        public AmazonServiceAccess(string awsRegion, string awsProfile = "default")
        {
            _region = RegionEndpoint.GetBySystemName(awsRegion);

            var chain = new CredentialProfileStoreChain();

            // Attempts to use a default profile if a custom profile is not found.
            if (!chain.TryGetAWSCredentials(awsProfile, out _awsCredentials) &&
                !chain.TryGetAWSCredentials("default", out _awsCredentials))
            {
                throw new AmazonServiceException("Unable to find AWS service credentials.");
            }
        }

        /// <summary>
        /// Get the region endpoint to use for AWS services.
        /// </summary>
        /// <returns>The region endpoint for service connections.</returns>
        public RegionEndpoint GetRegion() => _region;

        /// <summary>
        /// Gets the AWS credentials for the specified profile, or the default profile if available.
        /// </summary>
        /// <returns>AWS credentials for service access.</returns>
        public AWSCredentials GetCredentials() => _awsCredentials;
    }
}
