// <copyright file="MarkerRepository.cs" company="Random Developers on the Internet">
// Copyright © 2015 Peter Thoman. Copyright © 2020 Neil McNeight.
// All rights reserved. Licensed under the GPLv3 license.
// See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

using Hexer.Data;

namespace Hexer.HexViewLibrary
{
    public class MarkerRepository
    {
        public static readonly MarkerRepository Instance = new MarkerRepository();

        private MarkerRepository()
        {
        }

        private readonly SortedList<int, DataMarker> markers = new SortedList<int, DataMarker>();
        private List<DataMarker> serializedMarkers = new List<DataMarker>();

        public void AddMarker(int addr, DataType dt)
        {
            this.markers.Add(addr, new DataMarker(addr, dt));
        }

        internal void RemoveMarker(int selectedAddress)
        {
            this.markers.Remove(selectedAddress);
        }

        public DataMarker GetMarker(int addr)
        {
            if (this.markers.ContainsKey(addr))
            {
                return this.markers[addr];
            }

            return null;
        }

        public void EditMarker(int selectedAddress, HexView hexview)
        {
            new MarkerEditor(this.GetMarker(selectedAddress), hexview).Show();
        }

        public bool isMarker(int addr)
        {
            return this.markers.ContainsKey(addr);
        }

        public DataMarker GetMarkerCovering(int addr)
        {
            // This is terrible. Should really use something like an interval tree for marker storage.
            // On the other hand, it's probably easily fast enough. Doesn't make it any less terrible though.
            const int MAX_MARKER_BYTES = 16;

            for (var i = 0; i < MAX_MARKER_BYTES; ++i)
            {
                if (this.markers.ContainsKey(addr))
                {
                    var m = this.markers[addr];
                    if (m.NumBytes > i)
                    {
                        return m;
                    }
                    else
                    {
                        return null;
                    }
                }

                addr -= 8;
            }

            return null;
        }

        // --------------------------------------------------------------------- File Handling
        private string fileName = string.Empty;

        public bool HasFileName()
        {
            return this.fileName.Length > 0;
        }

        public void SaveToFile(string fn = "")
        {
            if (fn == string.Empty)
            {
                fn = this.fileName;
            }

            var serializer = new XmlSerializer(typeof(List<DataMarker>));
            using (TextWriter writer = new StreamWriter(fn))
            {
                this.serializedMarkers = this.markers.Values.ToList();
                serializer.Serialize(writer, this.serializedMarkers);
            }

            this.fileName = fn;
        }

        public void LoadFromFile(string fn)
        {
            var serializer = new XmlSerializer(typeof(List<DataMarker>));
            using (TextReader reader = new StreamReader(fn))
            {
                this.serializedMarkers = serializer.Deserialize(reader) as List<DataMarker>;
                foreach (var marker in this.serializedMarkers)
                {
                    this.markers.Add(marker.Address, marker);
                }
            }

            this.fileName = fn;
        }
    }
}
