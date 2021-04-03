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
using System.Threading;
using System.Threading.Tasks;

namespace Amazon_Stock_Tracker.Extensions
{
    /// <summary>
    /// Useful extensions to overcome some missing functionality and limitations while keeping the code clean.
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Concurrently Executes async actions for each item of <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of IEnumerable.</typeparam>
        /// <param name="source">Instance of <see cref="IEnumerable{T}"/>.</param>
        /// <param name="asyncAction">An async <see cref="Action" /> to execute.</param>
        /// <param name="maxDegreeOfParallelism">The maximum degree of parallelism, which must be greater than 0.</param>
        /// <returns>A <see cref="Task"/> representing an async operation.</returns>
        public static async Task ParallelForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> asyncAction, int maxDegreeOfParallelism)
        {
            // The AvailableWaitHandle is not used, so no need to call Dispose.
            var throttler = new SemaphoreSlim(initialCount: maxDegreeOfParallelism);

            var tasks = source.Select(async item =>
            {
                await throttler.WaitAsync();

                try
                {
                    await asyncAction(item).ConfigureAwait(false);
                }
                finally
                {
                    throttler.Release();
                }

            });

            await Task.WhenAll(tasks);
        }
    }
}
