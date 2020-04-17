// <copyright file="DataMarker.cs" company="Random Developers on the Internet">
// Copyright © 2015 Peter Thoman. Copyright © 2020 Neil McNeight.
// All rights reserved. Licensed under the GPLv3 license.
// See LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace Hexer.Data
{
    [Serializable]
    public class DataMarker : IComparable<DataMarker>
    {
        public DataType Type
        {
            get;
            set;
        }

        public int Address
        {
            get;
            set;
        }

        public int NumBytes
        {
            get;
            set;
        }

        public string Note
        {
            get;
            set;
        }

        public DataMarker(int addr, DataType dt)
        {
            this.Address = addr;
            this.Type = dt;
            this.NumBytes = dt.NumBytes;
            this.Note = "Unnamed";
        }

        // For serialization
        private DataMarker()
        {
        }

        /// <inheritdoc/>
        public int CompareTo(DataMarker other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return this.Address.CompareTo(other.Address);
        }
    }
}
