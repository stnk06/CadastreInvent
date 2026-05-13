using System;
using System.Collections.Generic;
using System.Linq;

namespace CadastreInvent.Registry.Domain.Entities
{
    public class PartyGroup : DomainEntity
    {
        public string Name { get; private set; }

        private readonly List<PartyGroupMember> _members = new();
        public IReadOnlyCollection<PartyGroupMember> Members => _members.AsReadOnly();

        protected PartyGroup() { }

        public PartyGroup(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            Id = Guid.NewGuid();
            Name = name;
            CreatedAt = DateTime.UtcNow;
        }

        public void AddMember(Party party, decimal numerator, decimal denominator)
        {
            if (party == null) throw new ArgumentNullException(nameof(party));

            var existingMember = _members.FirstOrDefault(m => m.PartyId == party.Id);
            if (existingMember != null) throw new InvalidOperationException($"Субъект с ID {party.Id} уже является участником группы.");

            _members.Add(new PartyGroupMember(Id, party.Id, numerator, denominator));

            ValidateShares();

            UpdateTimestamp();
        }

        public void RemoveMember(Guid partyId)
        {
            var member = _members.FirstOrDefault(m => m.PartyId == partyId);
            if (member == null) throw new InvalidOperationException($"Субъект с ID {partyId} не найден в группе.");

            _members.Remove(member);

            UpdateTimestamp();
        }

        private void ValidateShares()
        {
            decimal totalShare = 0m;

            foreach (var member in _members)
            {
                if (member.ShareDenominator > 0)
                {
                    totalShare += member.ShareNumerator / member.ShareDenominator;
                }
            }

            if (totalShare > 1.0m)
            {
                throw new InvalidOperationException($"Регистрация отклонена. Сумма долей участников группы превышает 1 (100%). Текущая рассчитанная сумма: {totalShare * 100:0.##}%.");
            }
        }
    }
}