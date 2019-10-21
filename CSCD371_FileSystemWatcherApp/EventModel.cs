using System;

namespace CSCD371_FileSystemWatcherApp
{
    public class EventModel
    {

        public string FileName { get; set; }
        public string AbsolutePath { get; set; }

        public string EventOccurred { get; set; }

        public DateTime DateTime { get; set; }

        public string FullToString
        {
            get
            {
                return $"[File Name: {FileName}, PATH: {AbsolutePath}, EVENT: {EventOccurred}, Date/Time: {{{DateTime}}}]";
            }
        }
    }
}
