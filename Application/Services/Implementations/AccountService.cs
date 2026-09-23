using GDB.App.Application.Dtos;
using GDB.App.Application.Services.Contracts;
using GDB.App.Domain;
using GDB.App.Domain.Enums;
using GDB.App.Domain.Models;
using GDB.App.Infrastructure.Repositories;
using GDB.App.Infrastructure.Repositories.Contracts;
using GDB.App.Infrastructure.Repositories.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GDB.App.Application.Services.Implementations
{
    internal class AccountService : IAccountService
    {
        //Business Logic 
        //composition
        private readonly IAccountRepository _accountRepository;

        public AccountService()
        {
            // When TransferService is constructed, Repository is created via factory
            _accountRepository = AccountRepositoryFactory.Create("DB");
        }
        public IAccount GetAccount(string accNo)
        {
            //Service-> Repository
            //var account = new AccountRepositoryInMemory().GetAccount(accNo);
            var account = _accountRepository.GetAccount(accNo);
            return account;
        }
        //public List<IAccount> GetAllAccounts()
        //{
        //    return _accountRepository.GetAllAccounts();
        //}

        public List<ViewAllAccountsResponseDto> GetAllAccounts()
        {
            List<IAccount> accounts= _accountRepository.GetAllAccounts();

            List<ViewAllAccountsResponseDto> dto = new List<ViewAllAccountsResponseDto>();

            foreach(var account in accounts)
            {
                dto.Add(new ViewAllAccountsResponseDto()
                {
                    Name = account.Name,
                    AccountNumber = account.AccountNumber,
                    Balance = account.Balance,
                    AccountPrivilege = account.Privilege,
                    AccountType = account.AccountType,
                    Age=account.Age

                }); 
                
            }

            return dto;

            
        }




        //public void ChangePin(string accountNumber,string oldPin,string newPin)
        //{
        //    IAccount account = _accountRepository.GetAccount(accountNumber);

        //    if (account == null)
        //    {
        //        throw new Exception("Account not found");
        //    }

        //    account.ChangePin(oldPin, newPin);//method in Account.cs

        //    _accountRepository.ChangePin(accountNumber, oldPin, newPin);
        //}

        public ViewBalanceResponseDto GetBalance(string accNo)
        {
            var account = _accountRepository.GetAccount(accNo);
            return new ViewBalanceResponseDto()
            {
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,

            };
        }

        public ViewAccountResponseDto ViewAccount(string accNo)
        {
            var account = _accountRepository.GetAccount(accNo);
            return new ViewAccountResponseDto()
            {
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                Name = account.Name
            };
        }
        public CloseAccountResponseDto CloseAccount(CloseAccountRequestDto request)
        {
            IAccount account = _accountRepository.GetAccount(request.AccountNumber);

            if (account == null)
                throw new Exception("Account not found.");

            if (account.Status == AccountStatus.Closed)
                throw new Exception("Account is already closed.");

            _accountRepository.CloseAccount(request.AccountNumber);

            return new CloseAccountResponseDto()
            {
                AccountNumber = request.AccountNumber,
                Status = AccountStatus.Closed,
                Message = "Account closed successfully."
            };
        }

        public CreateAccountResponseDto CreateAccount(CreateAccountRequestDto request)
        {
            Account account = AccountFactory.CreateAccount(
                request.AccountType,
                request.AccountNumber,
                request.Name,
                request.Age,
                request.Balance,
                request.Status,
                request.Pin,
                request.Privilege,
                request.OverdraftLimit,
                request.TenureMonths,
                request.InterestRate,
                request.MinimumBalance,
                request.EmployerName
            );

            _accountRepository.SaveAccount(account, request.Pin);

            return new CreateAccountResponseDto()
            {
                AccountNumber = account.AccountNumber,
                Name = account.Name,
                Age = account.Age,
                Balance = account.Balance,

                AccountType = account.AccountType,
                Status = account.Status,
                Privilege = account.Privilege,

                OverdraftLimit = account is CurrentAccount current
        ? current.OverdraftLimit
        : 0,

                TenureMonths = account is FixedDepositAccount fixedDeposit
        ? fixedDeposit.TenureMonths
        : 0,

                InterestRate = account is SavingsAccount savings
        ? savings.InterestRate
        : account is FixedDepositAccount fd
            ? fd.InterestRate
            : 0,

                MinimumBalance = account is SavingsAccount savingsAccount
        ? savingsAccount.MinBalance
        : 0,

                EmployerName = account is SalaryAccount salary
        ? salary.EmployerName
        : null

            };
        }
    }
}
