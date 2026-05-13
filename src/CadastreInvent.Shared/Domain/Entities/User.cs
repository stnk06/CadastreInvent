using System;

namespace CadastreInvent.Shared.Domain.Entities
{
    public class User : DomainEntity
    {
        public string Username { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public Guid RoleId { get; private set; }
        public bool IsActive { get; private set; }

        protected User() { }

        public User(string username, string email, string passwordHash, Guid roleId)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentNullException(nameof(email));
            if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentNullException(nameof(passwordHash));
            if (roleId == Guid.Empty) throw new ArgumentException(nameof(roleId));

            Id = Guid.NewGuid();
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            RoleId = roleId;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            if (!IsActive) throw new InvalidOperationException();
            IsActive = false;
            UpdateTimestamp();
        }

        public void Activate()
        {
            if (IsActive) throw new InvalidOperationException();
            IsActive = true;
            UpdateTimestamp();
        }

        public void ChangeRole(Guid newRoleId)
        {
            if (newRoleId == Guid.Empty) throw new ArgumentException(nameof(newRoleId));
            RoleId = newRoleId;
            UpdateTimestamp();
        }

        public void ChangePassword(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash)) throw new ArgumentNullException(nameof(newPasswordHash));
            PasswordHash = newPasswordHash;
            UpdateTimestamp();
        }
    }
}