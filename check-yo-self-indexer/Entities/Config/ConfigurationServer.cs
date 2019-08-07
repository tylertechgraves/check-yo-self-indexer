using System;

namespace check_yo_self_indexer.Entities.Config
{
    public class ConfigurationServer
    {
        public string Uri { get; set; }
        public int RetryCount { get; set; }
        public int RetryIntervalSec { get; set; }
    }
}