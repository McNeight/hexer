// <copyright file="DataFragment.cs" company="Random Developers on the Internet">
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
    public class DataFragment
    {
        /// <summary>
        /// Maximum length of a <see cref="DataFragment"/>.
        /// </summary>
        public const int MaxLength = 128;

        /// <summary>
        /// Gets the data.
        /// </summary>
        public byte[] Data { get; internal set; }

        /// <summary>
        /// Gets the address.
        /// </summary>
        public int Address { get; internal set; }

        /// <summary>
        /// Gets the length.
        /// </summary>
        public int Length { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataFragment"/> class.
        /// </summary>
        public DataFragment()
        {
            this.Address = 0;
            this.Length = 0;
            this.Data = new byte[MaxLength];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataFragment"/> class.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="source"></param>
        /// <param name="posInSource"></param>
        /// <param name="argLength"></param>
        public DataFragment(int address, byte[] source, int posInSource = 0, int argLength = MaxLength)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.Address = address;
            var remainder = source.Length - posInSource;
            var length = Math.Min(argLength, remainder);
            this.Length = length;
            this.Data = new byte[MaxLength];
            Array.Copy(source, posInSource, this.Data, 0, length);
        }
    }
}
