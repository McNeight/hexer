// <copyright file="Interval.cs" company="Random Developers on the Internet">
// Copyright © 2015 Peter Thoman. Copyright © 2020 Neil McNeight.
// All rights reserved. Licensed under the GPLv3 license.
// See LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace Hexer.Data
{
    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Interval<T> where T : IComparable<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Interval{T}"/> class.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public Interval(T min, T max)
        {
            this.Min = min;
            this.Max = max;
        }

        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        public T Min { get; set; }

        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        public T Max { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("[{0} - {1}]", this.Min, this.Max);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return this.Min.CompareTo(this.Max) <= 0;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(T value)
        {
            return (this.Min.CompareTo(value) <= 0) && (value.CompareTo(this.Max) <= 0);
        }

        public bool IsInside(Interval<T> range)
        {
            if (range == null)
            {
                throw new ArgumentNullException(nameof(range));
            }

            return this.IsValid() && range.IsValid() && range.Contains(this.Min) && range.Contains(this.Max);
        }
    }
}
