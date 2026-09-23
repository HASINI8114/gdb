using GDB.App.Application.Dtos;
using GDB.App.Application.Services;
using GDB.App.Application.Services.Contracts;
using GDB.App.Application.Services.Implementations;
using GDB.App.Domain.Enums;
using GDB.App.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GDB.App.Application.Controllers
{
    internal class AccountController
    {
        private IAccountService _accountService;
        public AccountController()
        {
            _accountService = AccountServiceFactory.Create();
        }

        //Boundary Class 
        public IAccount GetAccount(string accNo)
        {

            IAccount account = null;

            //Controller->Service
            account = _accountService.GetAccount(accNo);


            return account;
        }
        public List<ViewAllAccountsResponseDto> GetAllAccounts()
        {
            return _accountService.GetAllAccounts();
        }

        //public void ChangePin(string accountNumber, string oldPin, string newPin)
        //{
        //    _accountService.ChangePin(accountNumber, oldPin, newPin);
        //}

        public ViewBalanceResponseDto GetBalance(string accNo)
        {
            return _accountService.GetBalance(accNo);
        }
        public ViewAccountResponseDto ViewAccount(string accNo)
        {
            return _accountService.ViewAccount(accNo);
        }


        public CreateAccountResponseDto CreateAccount(CreateAccountRequestDto request)
        {
            return _accountService.CreateAccount(request);
        }

        public CloseAccountResponseDto CloseAccount(CloseAccountRequestDto request)
        {
            return _accountService.CloseAccount(request);
        }
    }
}
