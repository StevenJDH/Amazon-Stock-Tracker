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
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Amazon_Stock_Tracker.Models;

public class Product
{
    private readonly string? _name;
    private readonly string? _asin;
    private readonly string? _store;

    public string Name
    {
        get => String.IsNullOrEmpty(_name) ? "Invalid Name" : _name;
        init => _name = value.Trim();
    }

    public string Asin
    {
        get => String.IsNullOrEmpty(_asin) ? "Invalid ASIN" : _asin; 
        init => _asin = value.Trim();
    }

    public string Store
    {
        get => String.IsNullOrEmpty(_store) ? "Invalid Store" : _store;
        init => _store = value.Trim();
    }

    [JsonIgnore]
    public bool WasNotified { get; set; }
}