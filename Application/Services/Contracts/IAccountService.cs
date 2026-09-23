using GDB.App.Application.Dtos;
using GDB.App.Domain.Enums;
using GDB.App.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GDB.App.Application.Services.Contracts
{
    internal interface IAccountService
    {
        IAccount GetAccount(string accNo);
        //List<IAccount> GetAllAccounts();
        
        //void ChangePin(string accountNumber, string oldPin, string newPin);

        ViewBalanceResponseDto GetBalance(string accNo);

        ViewAccountResponseDto ViewAccount(string accNo);

        List<ViewAllAccountsResponseDto> GetAllAccounts();
        CreateAccountResponseDto CreateAccount(CreateAccountRequestDto request);

        CloseAccountResponseDto CloseAccount( CloseAccountRequestDto request);
    }
}
