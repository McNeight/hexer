﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace hexer
{
    [Serializable()]
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
            Address = addr;
            Type = dt;
            NumBytes = dt.NumBytes;
            Note = "Unnamed";
        }

        // For serialization
        private DataMarker() { }

        public int CompareTo(DataMarker other)
        {
            return Address.CompareTo(other.Address);
        }
    }
    
    public class MarkerRepository
    {
        public static readonly MarkerRepository Instance = new MarkerRepository();

        private MarkerRepository() { }

        private SortedList<int, DataMarker> markers = new SortedList<int, DataMarker>();
        private List<DataMarker> serializedMarkers = new List<DataMarker>();

        public void AddMarker(int addr, DataType dt)
        {
            markers.Add(addr, new DataMarker(addr, dt));
        }

        public DataMarker GetMarker(int addr)
        {
            if (markers.ContainsKey(addr))
            {
                return markers[addr];
            }
            return null;
        }

        // --------------------------------------------------------------------- File Handling
        #region File Handling
        private string fileName = "";
        public bool HasFileName()
        {
            return fileName.Length > 0;
        }
        public void SaveToFile(string fn = "")
        {
            if (fn == "") fn = fileName;
            var serializer = new XmlSerializer(typeof(List<DataMarker>));
            using (TextWriter writer = new StreamWriter(fn))
            {
                serializedMarkers = markers.Values.ToList();
                serializer.Serialize(writer, serializedMarkers);
            }
            fileName = fn;
        }
        public void LoadFromFile(string fn)
        {
            var serializer = new XmlSerializer(typeof(List<DataMarker>));
            using (TextReader reader = new StreamReader(fn))
            {
                serializedMarkers = serializer.Deserialize(reader) as List<DataMarker>;
                foreach (DataMarker marker in serializedMarkers) markers.Add(marker.Address, marker);
            }
            fileName = fn;
        }
        #endregion
    }
}
