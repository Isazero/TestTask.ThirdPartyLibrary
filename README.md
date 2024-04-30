Test task for Outsource company.


PROBLEM STATEMENT and DESIRED SOLUTION:
It's one of MANY classes within the solution that uses ThirdParty.IRestClient,
but unfortunately some of the API services we call are not very stable and we want to enhance all such usages with retry logic as below: 
1. By default we want to retry 3 times before failing, but the number should be configurable.
2. There should be some timeout between retries(any rule works as for now, but ideally should be extensible).
3. If System.Net.WebException is thrown, we want to retry, otherwise - we want to 'fail fast' without retrying.
4. If System.Net.WebException is thrown after all attempts, we want to log it and return some 'null' or empty result, otherwise - we want to re-throw the original exception.
5. We want to log the exception thrown, but we don't want to log 4 times if we retry.
6. Ideally we don't want to change dozens of classes that use IRestClient to apply retry logic.
7. Cover all the requirements by unit tests.
8. Do not use some built-in or 3rd party solutions for retry policies like Polly

I used a decorator pattern to solve this problem.
Also used moq and xunit libraries for writing tests
