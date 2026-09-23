using GDB.App.Domain.Enums;
using GDB.App.Domain.Exceptions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using GDB.App.Infrastructure.Repositories.Implementations;
using GDB.App.Infrastructure.Repositories.Contracts;
using GDB.App.Domain.Models;

namespace GDB.App.Infrastructure.Repositories
{
    class AccountRepositoryFactory
    {



        public static IAccountRepository Create(string choice)
        {


            IAccountRepository repository =  null;


            if (choice.Equals("DB"))

                repository = new AccountRepositoryDB();

            else if (choice.Equals("InMemory"))

                repository = new AccountRepositoryInMemory();


            return repository;

        }

    }
}







