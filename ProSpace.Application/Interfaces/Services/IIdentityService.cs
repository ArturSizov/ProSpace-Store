using ProSpace.Contracts.Contracts.Response;
using ProSpace.Contracts.Responses;

namespace ProSpace.Application.Interfaces.Services
{
    /// <summary>
    /// Defines the contract for cross-cutting security operations, including user identity creation, 
    /// credential validation, authentication tokens issuance, and account lifecycle management.
    /// </summary>
    public interface IIdentityService
    {
        /// <summary>
        /// Registers a new user account profile within the persistent infrastructure security store and assigns a baseline role.
        /// </summary>
        /// <param name="email">The unique string identity email address reference descriptor to use as the primary account credential identifier.</param>
        /// <param name="password">The raw, unhashed plain-text password string validation sequence to lock the account.</param>
        /// <param name="role">The initial security workflow access level string role configuration token target to be assigned.</param>
        /// <returns>A unified tracking ID packet model containing the newly instantiated global database tracking Guid reference trace property.</returns>
        Task<BaseIdResponse> CreateAccountAsync(string email, string password, string role);

        /// <summary>
        /// Validates whether the provided plain-text credentials match the cryptographically encrypted password hashes stored for the target account.
        /// </summary>
        /// <param name="email">The unique identification email address string mapping variables to look up the membership file.</param>
        /// <param name="password">The raw validation sequence verification token input to match against security algorithms hashes.</param>
        /// <returns><see langword="true"/> if the identity is verified and parameters match successfully; otherwise, <see langword="false"/>.</returns>
        Task<bool> CheckPasswordAsync(string email, string password);

        /// <summary>
        /// Retrieves a complete list structure of all active system application workflow security roles assigned underneath a target user key identifier.
        /// </summary>
        /// <param name="userId">The unique internal string identity system tracking database key trace indicator.</param>
        /// <returns>A matrix index collection list tracking functional operational privilege strings mapped to the agent user context node.</returns>
        Task<IList<string>> GetUserRolesAsync(string userId);

        /// <summary>
        /// Authenticates a user based on credentials and constructs a signed cryptographic session transport payload data model upon success.
        /// </summary>
        /// <param name="email">The unique membership registration email string layout targeting system access authorization.</param>
        /// <param name="password">The secret security verification passphrase string sequence backing profile access controls.</param>
        /// <returns>A unified package envelope holding core token values, lifetime expiration metrics, and profile attributes configurations mappings.</returns>
        Task<BaseResponse<LoginResponse>> LoginAsync(string email, string password);

        /// <summary>
        /// Permanently purges a system user account credential record profile boundary layout entirely out of active security subsystem tables.
        /// </summary>
        /// <param name="appUserId">The underlying global unique identity database tracking key identifier targeting extraction operations.</param>
        /// <returns>A tracking confirmation layout validation metric token validating successful transaction execution parameters.</returns>
        Task<BaseIdResponse> DeleteAccountAsync(Guid appUserId);

        /// <summary>
        /// Administratively blocks and locks out a security membership identity account indefinitely.
        /// </summary>
        /// <remarks>
        /// This method acts as a high-level security containment operation:
        /// <list type="bullet">
        /// <item><description>Enforces a strict account lockout expiration timestamp extended to a distant future date (the year 2099).</description></item>
        /// <item><description>Forces an immediate security stamp update (<c>UpdateSecurityStampAsync</c>) to invalidate all actively circulating JWT tokens or sessions associated with this identity context.</description></item>
        /// </list>
        /// </remarks>
        /// <param name="appUserId">The unique infrastructure identity tracking key (<see cref="Guid"/>) of the target user stored inside the security database.</param>
        /// <returns>
        /// A standardized structural <see cref="BaseIdResponse"/> containing the locked account's <c>Id</c> on success, 
        /// or a collection of structured system failure descriptions if the Identity Provider rejects the modification.
        /// </returns>
        Task<BaseIdResponse> BlockAccountAsync(Guid appUserId);
    }
}
