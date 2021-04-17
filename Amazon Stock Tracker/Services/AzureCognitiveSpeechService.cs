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
        private readonly SpeechConfig _config;
        private SpeechSynthesizer _synthesizer;

        /// <summary>
        /// Constructs a new <see cref="AzureCognitiveSpeechService"/> instance to send notifications
        /// using the Azure Cognitive Speech service.
        /// </summary>
        /// <param name="subscriptionKey">Azure subscription key.</param>
        /// <param name="serviceRegion">Azure region identifier for the subscription key.</param>
        /// <param name="voiceName">Azure speech synthesis voice to use.</param>
        public AzureCognitiveSpeechService(string subscriptionKey, string serviceRegion, string voiceName)
        {
            _config = SpeechConfig.FromSubscription(subscriptionKey, serviceRegion);

            if (!voiceName.Equals("default", StringComparison.InvariantCultureIgnoreCase))
            {
                _config.SpeechSynthesisVoiceName = voiceName;
            }
            
            _synthesizer = new SpeechSynthesizer(_config);
        }

        /// <summary>
        /// Sends a notification message to the Azure Cognitive Speech service asynchronously.
        /// </summary>
        /// <param name="msg">Message to send.</param>
        /// <returns>Unique identifier assigned to the message sent.</returns>
        public async Task<string> SendNotificationAsync(string msg)
        {
            var response = await _synthesizer.SpeakTextAsync(msg);

            // These two lines are needed as a workaround to ensure audio output uses
            // the current default output device even after it changes during runtime.
            Dispose();
            _synthesizer = new SpeechSynthesizer(_config);

            return response.ResultId;
        }

        /// <summary>
        /// Releases any unmanaged resources and disposes of the managed resources used
        /// by the <see cref="AzureCognitiveSpeechService"/>.
        /// </summary>
        public void Dispose()
        {
            _synthesizer?.Dispose();
        }
    }
}
