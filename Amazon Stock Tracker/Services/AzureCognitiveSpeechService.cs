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
using Microsoft.CognitiveServices.Speech;

namespace Amazon_Stock_Tracker.Services
{
    sealed class AzureCognitiveSpeechService : INotificationService
    {
        private readonly SpeechSynthesizer _synthesizer;

        public AzureCognitiveSpeechService(string subscriptionKey, string serviceRegion, string voiceName)
        {
            var config = SpeechConfig.FromSubscription(subscriptionKey, serviceRegion);

            if (!voiceName.Equals("default", StringComparison.InvariantCultureIgnoreCase))
            {
                config.SpeechSynthesisVoiceName = voiceName;
            }
            
            _synthesizer = new SpeechSynthesizer(config);
        }

        public async Task<string> SendNotificationAsync(string msg)
        {
            var response = await _synthesizer.SpeakTextAsync(msg);
            
            return response.ResultId;
        } 
        
        public void Dispose()
        {
            _synthesizer?.Dispose();
        }
    }
}
