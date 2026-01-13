namespace SGS.Projects.Api.Services
{
    public interface ISapB1AuthService
    {
        Task<bool> ValidateCredentialsAsync(string userName, string password, CancellationToken cancellationToken = default);
    }
}


