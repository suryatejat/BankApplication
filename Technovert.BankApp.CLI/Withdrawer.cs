﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Technovert.BankApp.Services;
using Technovert.BankApp.Models;

namespace Technovert.BankApp.CLI
{
    public class Withdrawer
    {
        public void Withdraw(string bankName,string accountName,BankService service)
        {
            decimal amount;
            Console.Write("Please Enter the amount to be Withdrawn : ");
            amount = Convert.ToDecimal(Console.ReadLine());
            try
            {
                service.Withdraw(bankName, accountName, amount);
                Printer printer = new Printer();
                printer.ResponsePrinter("Withdraw");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}