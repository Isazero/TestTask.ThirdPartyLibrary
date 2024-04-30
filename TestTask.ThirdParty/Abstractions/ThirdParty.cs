namespace TestTask.ThirdParty
{
    public interface IRestClient 
    {
        public Task<TModel?> Get<TModel>(string url);
        public Task<TModel?> Put<TModel>(string url, TModel model);
        public Task<TModel?> Post<TModel>(string url, TModel model);
        public Task<TModel?> Delete<TModel>(int id);
    }
}