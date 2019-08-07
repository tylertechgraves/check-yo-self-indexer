namespace check_yo_self_indexer.Entities.Config
{
    public class AppConfig
    {
        public AppConfig()
        {
        }

        private string _baseUri;
        private string _baseUriService;

        public string Title { get; set; }
        public string DefaultLanguage { get; set; }
        public string BaseUri
        {
            get { return UrlAdjuster.ReplaceHostAndSetTrailingSlash(this._baseUri); }
            set { this._baseUri = value; }
        }
        public string BaseUriService
        {
            get { return UrlAdjuster.ReplaceHostAndSetTrailingSlash(this._baseUriService); }
            set { this._baseUriService = value; }
        }
        public string Version { get; set; }
        public ConfigurationServer ConfigurationServer { get; set; }
        public Elasticsearch Elasticsearch { get; set; }
    }
}