#nullable enable
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
            cancellationToken.ThrowIfCancellationRequested();

            user.Id = await repositorioUsuarios.CrearUsuario(user);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(UsuarioApp user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await repositorioUsuarios.EliminarUsuario(user.Id);
            return IdentityResult.Success;
        }

        public void Dispose()
        {
        }

        private static UsuarioApp? MapearUsuario(UserViewModel? usuario)
        {
            if (usuario == null)
            {
                return (UsuarioApp?)null;
            }

            return new UsuarioApp
            {
                Id = usuario.IdUsuario,
                Usuario = usuario.Nombre,
                Email = usuario.Correo,
                EmailNormalizado = usuario.Correo?.ToUpperInvariant(),
                PasswordHash = usuario.Clave
            };
        }

        public async Task<UsuarioApp?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            // throw new NotImplementedException();

            return await repositorioUsuarios.BuscarUsuarioPorEmail(normalizedEmail);

        }

        public Task<UsuarioApp?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!int.TryParse(userId, out var idUsuario))
            {
                return Task.FromResult<UsuarioApp?>(null);
            }

            return BuscarPorIdAsync(idUsuario);
        }

        public async Task<UsuarioApp?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            //throw new NotImplementedException();
            return await repositorioUsuarios.BuscarUsuarioPorEmail(normalizedUserName);
        }

        public Task<string?> GetEmailAsync(UsuarioApp user, CancellationToken cancellationToken)
        {
            // throw new NotImplementedException();
            return Task.FromResult((string?)user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(UsuarioApp user, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public Task<string?> GetNormalizedEmailAsync(UsuarioApp user, CancellationToken cancellationToken)
        {
            return Task.FromResult<string?>(user.EmailNormalizado ?? user.Email?.ToUpperInvariant());
        }

        public Task<string?> GetNormalizedUserNameAsync(UsuarioApp user, CancellationToken cancellationToken)
        {
            return Task.FromResult<string?>(user.EmailNormalizado ?? user.Usuario?.ToUpperInvariant());
        }

        public Task<string?> GetPasswordHashAsync(UsuarioApp user, CancellationToken cancellationToken)
        {
            // throw new NotImplementedException();
            return Task.FromResult((string?)user.PasswordHash);

        }

        public Task<string> GetUserIdAsync(UsuarioApp user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id.ToString());
        }

        public Task<string?> GetUserNameAsync(UsuarioApp user, CancellationToken cancellationToken)
        {
            return Task.FromResult<string?>(user.Usuario);
        }

        public Task<bool> HasPasswordAsync(UsuarioApp user, CancellationToken cancellationToken)
        {
            return Task.FromResult(!string.IsNullOrWhiteSpace(user.PasswordHash));
        }

        public Task SetEmailAsync(UsuarioApp user, string? email, CancellationToken cancellationToken)
        {
            user.Email = email;
            return Task.CompletedTask;
        }

        public Task SetEmailConfirmedAsync(UsuarioApp user, bool confirmed, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
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
            user.Usuario = userName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(UsuarioApp user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var existente = await repositorioUsuarios.ObtenerUsuarioPorId(user.Id);
            if (existente == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Usuario no encontrado." });
            }

            existente.Correo = user.Email;
            existente.Clave = user.PasswordHash;
            existente.Nombre = user.Usuario;

            var actualizado = await repositorioUsuarios.ActualizarUsuario(existente);
            return actualizado
                ? IdentityResult.Success
                : IdentityResult.Failed(new IdentityError { Description = "No fue posible actualizar el usuario." });
        }

        private async Task<UsuarioApp?> BuscarPorIdAsync(int idUsuario)
        {
            var usuario = await repositorioUsuarios.ObtenerUsuarioPorId(idUsuario);
            return MapearUsuario(usuario);
        }

    }
}
