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

namespace Amazon_Stock_Tracker.Services;

sealed class AmazonProductDataService : IAmazonProductDataService
{
    private readonly HttpClient _httpClient;
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
            ["amazon.es"] = "Envíos desde y vendidos por Amazon",
            ["amazon.se"] = "Fraktas från och säljs av Amazon",
            ["amazon.com.tr"] = "Amazon.com.tr tarafından satılır ve gönderilir",
            ["amazon.ae"] = "Ships from and sold by Amazon",
            ["amazon.co.uk"] = "Dispatched from and sold by Amazon",
            ["amazon.com"] = "Ships from and sold by Amazon"
        };

    /// <summary>
    /// Constructs a new <see cref="AmazonProductDataService"/> instance to get Amazon product data.
    /// </summary>
    /// <param name="timeoutSeconds">
    /// Number of seconds to wait before a request times out. Default is 90 seconds.
    /// </param>
    public AmazonProductDataService(int timeoutSeconds = 90)
    {
        _httpClient = CreateHttpClient(TimeSpan.FromSeconds(timeoutSeconds));
    }

    /// <summary>
    /// Gets the HTML source code for the specified web page.
    /// </summary>
    /// <param name="url">URL of web page.</param>
    /// <returns>HTML source code of web page.</returns>
    /// <exception cref="HttpRequestException">The HTTP response is unsuccessful.</exception>
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

    /// <summary>
    /// Gets product details for specified item being sold on Amazon for any supported country
    /// asynchronously.
    /// </summary>
    /// <param name="store">Amazon root domain for country where item is sold.</param>
    /// <param name="asin">ASIN of item from URL or product page.</param>
    /// <returns>Details such as full product title, price, stock status, etc.</returns>
    public Task<ProductDetails> GetProductDetailsAsync(string store, string asin)
    {
        string? inStockPhrase = InStockPhrases.GetValueOrDefault(store);
            
        if (inStockPhrase == null)
        {
            throw new ArgumentException($"The store '{store}' for ASIN '{asin}' is invalid or not yet supported.");
        }

        const string TITLE_PATTERN = "<span id=\"productTitle\" class=\"a-size-large product-title-word-break\">([^>]*?)</span>"; // TODO: double check if this still works with our test product list.
        const string PRICE_PATTERN = "price\"><span class=\"a-offscreen\">(.*?)</span>"; // Works also for RTL languages.

        // Rule S4457: Parameter check and async logic are separated so that an exception thrown works as intended.
        async Task<ProductDetails> AsyncImpl()
        {
            string html = await GetHtmlAsync($"https://www.{store}/dp/{asin}").ConfigureAwait(false);
            bool inStock = html.Contains(inStockPhrase);
            bool isRedirected = !html.Contains("'winningAsin': '',") && !html.Contains($"'winningAsin': '{asin}',");  // TODO: maybe redirection means that 'winningAsin': '' is populated with different id but when empty it is not redirected. need to catch a scenario to prove this theory.
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
                Title = HttpUtility.HtmlDecode(Regex.Match(html, TITLE_PATTERN).Groups[1].Value.Trim()),
                PriceTag = Regex.Match(html, PRICE_PATTERN).Groups[1].Value,
                Asin = asin,
                Status = status,
                Store = store
            };

            return details;
        }

        return AsyncImpl();
    }

    /// <summary>
    /// Creates a new instance of <see cref="HttpClient"/> with configuration needed to interact
    /// with Amazon's website.
    /// </summary>
    /// <param name="timeoutSeconds">Number of seconds to wait before a request times out.</param>
    /// <returns>New <see cref="HttpClient"/>.</returns>
    private static HttpClient CreateHttpClient(TimeSpan timeoutSeconds)
    {
        var httpClientHandler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
        };

        var httpClient = new HttpClient(httpClientHandler, disposeHandler: true)
        {
            Timeout = timeoutSeconds
        };

        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; rv:87.0) Gecko/20100101 Firefox/87.0");
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

        return httpClient;
    }

    /// <summary>
    /// Releases any unmanaged resources and disposes of the managed resources used
    /// by the <see cref="AmazonProductDataService"/>.
    /// </summary>
    public void Dispose()
    {
        _httpClient.Dispose();
    }
}