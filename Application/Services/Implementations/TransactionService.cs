using GDB.App.Application.Dtos;
using GDB.App.Application.Services.Contracts;
using GDB.App.Domain.Enums;
using GDB.App.Domain.Exceptions;
using GDB.App.Domain.Models;
using GDB.App.Infrastructure.Repositories;
using GDB.App.Infrastructure.Repositories.Contracts;
using GDB.App.Infrastructure.Repositories.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;


namespace GDB.App.Application.Services.Implementations
{
    internal class TransactionService : ITransactionService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;

        public TransactionService()
        {
            _accountRepository = AccountRepositoryFactory.Create("DB");
            _transactionRepository = TransactionRepositoryFactory.Create("DB");
        }

        public DepositResponseDto Deposit(string accountNumber, decimal amount)
        {
            IAccount account = _accountRepository.GetAccount(accountNumber);

            if (account == null)
            {
                throw new Exception("Account not found");
            }

            account.Deposit(amount);//method in Account.cs

            _accountRepository.UpdateBalance(
                accountNumber,
                account.Balance
            );
            _transactionRepository.SaveTransaction(
                null,
                accountNumber,
                TransactionType.Deposit,
                amount,
                TransactionStatus.Success,
                0,
                account.Balance
            );
            return new DepositResponseDto()
            {
                Balance = account.Balance,
                TransactionStat = TransactionStatus.Success
            };
        }

        public WithdrawResponseDto Withdraw(
            string accountNumber,
            string pin,
            decimal amount)
        {
            IAccount account = _accountRepository.GetAccount(accountNumber);

            if (account == null)
            {
                throw new Exception("Account not found");
            }

            account.Withdraw( amount, pin);//method in Account.cs

            _accountRepository.UpdateBalance(
                    accountNumber,
                    account.Balance
                );

            _transactionRepository.SaveTransaction(
                accountNumber,
                null,
                TransactionType.Withdraw,
                amount,
                TransactionStatus.Success,
                account.Balance,
                0
            );

            return new WithdrawResponseDto()
            {
                Balance = account.Balance,
                TransactionStat = TransactionStatus.Success
            };
        }

        public TranferFundsResponseDto TransferFunds(string fromAccountNumber,
                                string toAccountNumber,
                                string pin,
                                decimal amount)
        {
            TransactionStatus status = TransactionStatus.Pending;

            IAccount fromAccount = null;
            IAccount toAccount = null;

            try
            {
                // Get From Account
                fromAccount = GetAccount(fromAccountNumber);
                if (fromAccount == null)
                {
                    throw new Exception("From account not found");
                }

                // Check From Account
                CheckIfAccountIsActive(fromAccount);

                // Get To Account
                toAccount = GetAccount(toAccountNumber);

                if (toAccount == null)
                {
                    throw new Exception("To account not found");
                }

                // Check To Account
                CheckIfAccountIsActive(toAccount);

                // Check PIN
                CheckIfPinIsValid(fromAccount, pin);

                Console.WriteLine("BEFORE TRANSFER");

                DisplayAccount("FROM ACCOUNT", fromAccount);
                DisplayAccount("TO ACCOUNT", toAccount);

                // Withdraw from sender
                fromAccount.Withdraw(amount, pin);

                // Deposit into receiver
                toAccount.Deposit(amount);

                // Save both updated balances
                _accountRepository.SaveAccounts(
                    fromAccount,
                    toAccount
                );
                _transactionRepository.SaveTransaction(
                    fromAccountNumber,
                    toAccountNumber,
                    TransactionType.Transfer,
                    amount,
                    TransactionStatus.Success,
                    fromAccount.Balance,
                    toAccount.Balance
                );

                Console.WriteLine("AFTER TRANSFER");

                DisplayAccount("FROM ACCOUNT", fromAccount);
                DisplayAccount("TO ACCOUNT", toAccount);

                status = TransactionStatus.Success;
            }
            catch (InactiveAccountException)
            {
                throw new InactiveAccountException();
            }
            catch (InvalidPinException)
            {
                throw new InvalidPinException();
            }

            return new TranferFundsResponseDto()
            {
                FromAccountNumber = fromAccountNumber,
                ToAccountNumber = toAccountNumber,
                Amount = amount,
                FromAccountBalance = fromAccount.Balance,
                ToAccountBalance = toAccount.Balance,
                TransactionStat = status
            };
        }



        private bool CheckIfPinIsValid(IAccount account, string pinNumber)
        {



            if (!account.ValidatePin(pinNumber))
                throw new InvalidPinException();



            return true;

        }



        private bool CheckIfAccountIsActive(IAccount account)
        {



            if (account.Status != AccountStatus.Active)
                throw new InactiveAccountException();



            return true;

        }



        //SRP - Single Responsiblity Principle

        //To get the acount information only

        private IAccount GetAccount(string accountNumber)
        {

            // Get Account info from the database

            //Moment you create ab object of a class inside a method, then you are directly

            //dependent on the object. Tight Coupling

            //AccountfactoryRepository creatae method is returning an interace

            //therefore the TransferSerivce is programming to an interace and not implementation

            //Once - Use and Dispose - Uses Relationship

            //var accountRepository = AccountRepositoryFactory.Create();

            var account = _accountRepository.GetAccount(accountNumber);



            return account;

        }
        private void DisplayAccount(string message, IAccount account)
        {
            Console.WriteLine(message);
            Console.WriteLine("--------------------------------");
            Console.WriteLine($"Account Number : {account.AccountNumber}");
            Console.WriteLine($"Name           : {account.Name}");
            Console.WriteLine($"Balance        : {account.Balance}");
            Console.WriteLine();
        }
        public List<ViewRecentTransactionsResponseDto> GetRecentTransactions(string accountNumber)
        {
            IAccount account =
                _accountRepository.GetAccount(accountNumber);

            if (account == null)
                throw new Exception("Account not found.");

            return _transactionRepository.GetRecentTransactions(
                accountNumber);
        }

    }
}
