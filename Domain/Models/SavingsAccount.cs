// Domain/SavingsAccount.cs
using GDB.App.Domain;
using GDB.App.Domain.Exceptions;
using System;
using GDB.App.Domain.Enums;

namespace GDB.App.Domain.Models
{
    public class SavingsAccount : Account
    {
        private decimal _minBalance;
        private double _interestRate;

        public SavingsAccount(string accountNumber, string name, int age, decimal balance,AccountType accountType, AccountStatus status, string pin,AccountPrivilege privilege, decimal minBalance = 1000.0m, double interestRate = 4.0)
            : base(accountNumber, name, age, balance, accountType, status, pin, privilege)
        {
            this._minBalance = minBalance;
            this._interestRate = interestRate;
        }

        public override void ProcessDebit(decimal amount)
        {
            if ((_balance - amount) < _minBalance)
                throw new MinimumBalanceViolationException($"Cannot breach minimum balance of Rs {_minBalance:F2}");
            _balance -= amount;
        }

        public void ApplyInterest() => _balance += _balance * (decimal)(_interestRate / 100.0);
        public decimal MinBalance => _minBalance;
        public double InterestRate => _interestRate;
    }
}
