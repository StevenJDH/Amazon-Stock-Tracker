﻿/*
 * This file is part of Amazon Stock Tracker <https://github.com/StevenJDH/Amazon-Stock-Tracker>.
 * Copyright (C) 2021-2022 Steven Jenkins De Haro.
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

namespace Amazon_Stock_Tracker.Services;

interface IAmazonServiceAccess
{
    /// <summary>
    /// Get the region endpoint to use for AWS services.
    /// </summary>
    /// <returns>The region endpoint for service connections.</returns>
    RegionEndpoint GetRegion();

    /// <summary>
    /// Gets the AWS credentials for the specified profile, or the default profile if available.
    /// </summary>
    /// <returns>AWS credentials for service access.</returns>
    AWSCredentials GetCredentials();
}
