using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;
using NSIE.Models;

namespace NSIE.Servicios
{
    public class UsuarioStore : IUserStore<UsuarioApp>, IUserEmailStore<UsuarioApp>, IUserPasswordStore<UsuarioApp>
    {
        private readonly IRepositorioUsuarios repositorioUsuarios;

        public UsuarioStore(IRepositorioUsuarios repositorioUsuarios)
        {
            this.repositorioUsuarios = repositorioUsuarios;
        }

        public async Task<IdentityResult> CreateAsync(UsuarioApp user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
            //user.id = await repositorioUsuarios.CrearUsuario(user);
            //return IdentityResult.Success;
        }

        public Task<IdentityResult> DeleteAsync(UsuarioApp user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public async Task<UsuarioApp?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            // throw new NotImplementedException();

            return await repositorioUsuarios.BuscarUsuarioPorEmail(normalizedEmail);

        }

        public Task<UsuarioApp?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<UsuarioApp?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            //throw new NotImplementedException();
            return await repositorioUsuarios.BuscarUsuarioPorEmail(normalizedUserName);
        }

        public Task<string?> GetEmailAsync(UsuarioApp user, CancellationToken cancellationToken)
        {
            // throw new NotImplementedException();
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(UsuarioApp user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetNormalizedEmailAsync(UsuarioApp user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetNormalizedUserNameAsync(UsuarioApp user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetPasswordHashAsync(UsuarioApp user, CancellationToken cancellationToken)
        {
            // throw new NotImplementedException();
            return Task.FromResult(user.PasswordHash);

        }

        public Task<string> GetUserIdAsync(UsuarioApp user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();

            //return Task.FromResult(user.id.ToString());
        }

        public Task<string?> GetUserNameAsync(UsuarioApp user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasPasswordAsync(UsuarioApp user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetEmailAsync(UsuarioApp user, string? email, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetEmailConfirmedAsync(UsuarioApp user, bool confirmed, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetNormalizedEmailAsync(UsuarioApp user, string? normalizedEmail, CancellationToken cancellationToken)
        {
            //throw new NotImplementedException();
            user.EmailNormalizado = normalizedEmail;
            return Task.CompletedTask;
        }
        public Task SetNormalizedUserNameAsync(UsuarioApp user, string? normalizedName, CancellationToken cancellationToken)
        {
            // throw new NotImplementedException();
            return Task.CompletedTask;
        }

        public Task SetPasswordHashAsync(UsuarioApp user, string? passwordHash, CancellationToken cancellationToken)
        {
            //throw new NotImplementedException();
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(UsuarioApp user, string? userName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(UsuarioApp user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

    }
}
