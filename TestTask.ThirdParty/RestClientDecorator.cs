using System.Net;

namespace TestTask.ThirdParty;


// PROBLEM STATEMENT and DESIRED SOLUTION:
    // It's one of MANY classes within the solution that uses ThirdParty.IRestClient,
    // but unfortunately some of the API services we call are not very stable and we want to enhance all such usages with retry logic as below:
    // 1. By default we want to retry 3 times before failing, but the number should be configurable.
    // 2. There should be some timeout between retries(any rule works as for now, but ideally should be extensible).
    // 3. If System.Net.WebException is thrown, we want to retry, otherwise - we want to 'fail fast' without retrying.
    // 4. If System.Net.WebException is thrown after all attempts, we want to log it and return some 'null' or empty result, otherwise - we want to re-throw the original exception.
    // 5. We want to log the exception thrown, but we don't want to log 4 times if we retry.
    // 6. Ideally we don't want to change dozens of classes that use IRestClient to apply retry logic.
	// 7. Cover all the requirements by unit tests.
	// 8. Do not use some built-in or 3rd party solutions for retry policies like Polly

//Pattern Decorator perfectly solves this task
public class RestClientDecorator : IRestClient
{
    private readonly IRestClient _restClient;
    private readonly ILogger _logger;
    private readonly int _retryCounter;
    private readonly TimeSpan _retryDelay;

    public RestClientDecorator(IRestClient restClient, ILogger logger, int retryCounter = 3,
        TimeSpan? retryDelay = default)
    {
        _restClient = restClient;
        _logger = logger;
        _retryCounter = retryCounter;
        _retryDelay = retryDelay ?? TimeSpan.FromSeconds(5);
    }

    public async Task<TModel?> Get<TModel>(string url)
    {
        return await ExecuteWithRetry(() => _restClient.Get<TModel>(url));
    }

    public async Task<TModel?> Put<TModel>(string url, TModel model)
    {
        return await ExecuteWithRetry(() => _restClient.Put(url, model));
    }

    public async Task<TModel?> Post<TModel>(string url, TModel model)
    {
        return await ExecuteWithRetry(() => _restClient.Post(url, model));
    }

    public async Task<TModel?> Delete<TModel>(int id)
    {
        return await ExecuteWithRetry(() => _restClient.Delete<TModel>(id));
    }

    private async Task<TModel?> ExecuteWithRetry<TModel>(Func<Task<TModel>> operation)
    {
        for (var attempt = 0; attempt < _retryCounter; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (WebException ex)
            {
                // Retry if not the last attempt
                // Log only last error
                if (attempt < _retryCounter - 1)
                {
                    await Task.Delay(_retryDelay);
                }
                else
                {
                    _logger.Error(ex);
                    return default;
                }
            }
            catch (Exception ex) when (ex is not WebException)
            {
                _logger.Error(ex);
                throw;
            }
        }
        
        _logger.Error(new InvalidOperationException("Retry Failed"));
        return default;
    }
}