﻿/**
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

namespace Amazon_Stock_Tracker.Models
{
    class ConfigSettings
    {
        public int CheckIntervalSeconds { get; set; }
        public string NotificationMessage { get; set; }
        public string LocalVoiceName { get; set; }
        public string AwsProfile { get; set; }
        public string AwsRegion { get; set; }
        public bool AwsSmsEnabled { get; set; }
        public string AwsSmsNumber { get; set; }
        public string AwsSmsSenderId { get; set; }
        public string AwsSmsType { get; set; }
        public string AwsSmsMaxPrice { get; set; }
        public string AwsSmsMonthlySpendLimit { get; set; }
        public bool AwsEmailEnabled { get; set; }
        public string AwsEmailAddress { get; set; }
        public bool AzureVoiceEnabled { get; set; }
        public string AzureVoiceName { get; set; }
        public string AzureVoiceKey { get; set; }
        public string AzureVoiceRegion { get; set; }
    }
}
