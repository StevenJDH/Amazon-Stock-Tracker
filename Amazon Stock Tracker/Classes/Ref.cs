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

namespace Amazon_Stock_Tracker.Classes
{
    class Ref<T> where T : struct
    {
        public T Value { get; set; }

        /// <summary>
        /// Default constructor that automatically sets the default value for the T type.
        /// </summary>
        public Ref() => Value = default(T);

        /// <summary>
        /// Constructor to initialize the T type with a specific starting value.
        /// </summary>
        /// <param name="value">Initial value for the T type</param>
        public Ref(T value) => this.Value = value;

        /// <summary>
        /// Shows a string representation of the T type.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString() => Value.ToString();

        /// <summary>
        /// Implicitly exposes the T type from the Ref<T> object.
        /// </summary>
        /// <param name="rhs">The Ref<T> object</param>
        public static implicit operator T(Ref<T> rhs) => rhs.Value;

        /// <summary>
        /// Implicitly creates a Ref<T> object from the T type.
        /// </summary>
        /// <param name="rhs">The T type</param>
        public static implicit operator Ref<T>(T rhs) => new Ref<T>(rhs);
    }
}
