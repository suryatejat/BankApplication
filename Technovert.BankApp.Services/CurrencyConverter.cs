﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Technovert.BankApp.Services
{
    public class CurrencyConverter
    {
        public decimal Converter(decimal amount,decimal exchangeRate)
        {
            decimal actualAmount = amount * exchangeRate;
            return actualAmount;
        }
    }
}
