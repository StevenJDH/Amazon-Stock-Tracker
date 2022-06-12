/*
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
using Amazon_Stock_Tracker.Models;

namespace Amazon_Stock_Tracker.Services;

interface IAmazonProductDataService : IDisposable
{
    /// <summary>
    /// Gets product details for specified item being sold on Amazon for any supported country
    /// asynchronously.
    /// </summary>
    /// <param name="store">Amazon root domain for country where item is sold.</param>
    /// <param name="asin">ASIN of item from URL or product page.</param>
    /// <returns>Details such as full product title, price, stock status, etc.</returns>
    Task<ProductDetails> GetProductDetailsAsync(string store, string asin);
}
