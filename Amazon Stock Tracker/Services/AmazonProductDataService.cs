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
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Amazon_Stock_Tracker.Models;
using Polly;
using static Amazon_Stock_Tracker.Models.ProductDetails;

namespace Amazon_Stock_Tracker.Services
{
    sealed class AmazonProductDataService : IAmazonProductDataService
    {
        private readonly TimeSpan _timeoutSeconds;
        private HttpClient _httpClient;
        private HttpClientHandler _httpClientHandler;
        private static readonly IReadOnlyDictionary<string, string> InStockPhrases = 
            new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
            {
                ["amazon.com.au"] = "Ships from and sold by Amazon",
                ["amazon.com.br"] = "Enviado de e vendido por Amazon",
                ["amazon.ca"] = "Ships from and sold by Amazon",
                ["amazon.cn"] = "直接销售和发货",
                ["amazon.fr"] = "Expédié et vendu par Amazon",
                ["amazon.de"] = "Verkauf und Versand durch Amazon",
                ["amazon.in"] = "Sold by Cloudtail India and ships from Amazon Fulfillment",
                ["amazon.it"] = "Venduto e spedito da Amazon",
                ["amazon.co.jp"] = "この商品は、Amazon.co.jpが販売および発送します",
                ["amazon.com.mx"] = "Vendido y enviado por Amazon",
                ["amazon.nl"] = "Verzonden en verkocht door Amazon",
                ["amazon.pl"] = "Wysyłka i sprzedaż przez Amazon",
                ["amazon.sa"] = "يُشحن ويُباع من Amazon",
                ["amazon.sg"] = "Ships from and sold by Amazon",
                ["amazon.es"] = "Vendido y enviado por Amazon",
                ["amazon.se"] = "Fraktas från och säljs av Amazon",
                ["amazon.com.tr"] = "Amazon.com.tr tarafından satılır ve gönderilir",
                ["amazon.ae"] = "Ships from and sold by Amazon",
                ["amazon.co.uk"] = "Dispatched from and sold by Amazon",
                ["amazon.com"] = "Ships from and sold by Amazon"
            };

        public AmazonProductDataService(int timeoutSeconds = 90)
        {
            _timeoutSeconds = TimeSpan.FromSeconds(timeoutSeconds);
            CreateHttpClient();
        }

        private async Task<string> GetHtmlAsync(string url)
        {
            var response = await Policy
                .HandleResult<HttpResponseMessage>(m => !m.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(3), (result, timeSpan, retryCount, context) =>
                {
                    Debug.WriteLine($"Request failed with {result.Result.StatusCode}. Waiting {timeSpan} before next retry. Retry attempt {retryCount}");
                })
                .ExecuteAsync(() => _httpClient.GetAsync(url));

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public Task<ProductDetails> GetProductDetailsAsync(string store, string asin)
        {
            string inStockPhrase = InStockPhrases.GetValueOrDefault(store);
            
            if (inStockPhrase == null)
            {
                throw new ArgumentException($"The store '{store}' for ASIN '{asin}' is invalid or not yet supported.");
            }

            const string NAME_PATTERN = "<span id=\"productTitle\" class=\"a-size-large product-title-word-break\">([^>]*?)</span>";
            const string PRICE_PATTERN = "PriceString\">(.*?)</span>"; // Majority uses priceBlockBuyingPriceString, but some use priceBlockDealPriceString.
            const string PRICE_PATTERN_RTL = "PriceString\" dir=\"rtl\">(.*?)</span>"; // For RTL languages like Saudi Arabia.

            // Rule S4457: Parameter check and async logic are separated so that an exception thrown works as intended.
            async Task<ProductDetails> AsyncImpl()
            {
                string html = await GetHtmlAsync($"https://www.{store}/dp/{asin}").ConfigureAwait(false);
                bool inStock = html.Contains(inStockPhrase);
                bool isRedirected = !html.Contains($"'winningAsin': '{asin}',");
                bool hasCaptcha = html.Contains("opfcaptcha.amazon");
                StockStatus status;
                
                if (hasCaptcha)
                {
                    status = StockStatus.HasCaptcha;
                }
                else if (isRedirected)
                {
                    status = StockStatus.IsRedirected;
                }
                else if (inStock)
                {
                    status = StockStatus.InStock;
                }
                else
                {
                    status = StockStatus.OutOfStock;
                }

                var details = new ProductDetails
                {
                    Name = HttpUtility.HtmlDecode(Regex.Match(html, NAME_PATTERN).Groups[1].Value.Trim()),
                    PriceTag = Regex.Match(html, !store.Equals("amazon.sa") ? PRICE_PATTERN : PRICE_PATTERN_RTL).Groups[1].Value,
                    Asin = asin,
                    Status = status,
                    Store = store
                };

                return details;
            }

            return AsyncImpl();
        }

        private void CreateHttpClient()
        {
            _httpClientHandler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            };

            _httpClient = new HttpClient(_httpClientHandler, false)
            {
                Timeout = _timeoutSeconds
            };

            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; rv:87.0) Gecko/20100101 Firefox/87.0");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            _httpClientHandler?.Dispose();
        }
    }
}
