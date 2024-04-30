namespace TestTask.ThirdParty
{
    public class RestAppClassThatUsesRestClient
    {
        private RestClientDecorator _restClient;
        private ILogger _logger;

        public RestAppClassThatUsesRestClient(RestClientDecorator restClient, ILogger logger)
        {
            _restClient = restClient;
            _logger = logger;
        }

        public Task<TModel?> GetSomething<TModel>(string url)
        {
            return _restClient.Get<TModel>(url);
        }
    }
}
