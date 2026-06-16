namespace ProSpace.Application.Common.Interfaces
{
    public interface ITokenService
    {
        /// <summary>
        /// Token generated
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        (string Token, DateTime Expiration) GenerateJwtToken(string userId, string userName, IList<string> roles);
    }
}
