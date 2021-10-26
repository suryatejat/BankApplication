﻿using System;
using System.Collections.Generic;
using System.Linq;
using Technovert.BankApp.Models;
using Technovert.BankApp.Models.Exceptions;
using Technovert.BankApp.Models.Enums;

namespace Technovert.BankApp.Services
{
    public class TransactionService
    {
        public int limit = 50000;
        DateTime today = DateTime.Today;
        private AccountHolderService accountHolder;
        private Data data;
        private CurrencyConverter currencyConverter;
        public TransactionService(Data data ,AccountHolderService accountHolder,CurrencyConverter currencyConverter)
        {
            this.data = data;
            this.accountHolder = accountHolder;
            this.currencyConverter = currencyConverter;
        }
        public string TransactionIdGenerator(string bankId, string accountId)
        {
            return "TXN" + bankId + accountId + today.ToString("dd") + today.ToString("MM") + today.ToString("yyyy") + today.ToString("hh") + today.ToString("mm")+ today.ToString("ss");
        }
        
        public void Deposit(string bankId, string accountHolderId, decimal amount,string code)
        {
            this.accountHolder.InputValidator(bankId, accountHolderId);

            Account userAccount = accountHolder.AccountFinder(bankId, accountHolderId);
            Currency currentCurrency = this.data.currencies.SingleOrDefault(x => x.code == code);
            if (currentCurrency == null)
            {
                throw new InvalidCurrencyException();
            }
            if (userAccount == null)
            {
                throw new InvalidAccountNameException();
            }
            amount = currencyConverter.Converter(amount, currentCurrency.exchangeRate);
            amount = Math.Round(amount, 2);
            userAccount.Balance += amount;
            userAccount.Transactions.Add(new Transaction()
            {
                Id = TransactionIdGenerator(bankId, accountHolderId),
                Amount = amount,
                TransactionType = TransactionType.Credit,
                On = today.ToString("g"),
                TaxType=TaxType.None
            });
        }

        public void Withdraw(string bankId, string accountHolderId, decimal amount)
        {
            this.accountHolder.InputValidator(bankId, accountHolderId);

            Account userAccount = accountHolder.AccountFinder(bankId, accountHolderId);

            if (userAccount == null)
            {
                throw new InvalidAccountNameException();
            }
            if (userAccount.Balance < amount)
            {
                throw new InsufficientFundsException();
            }
            amount = Math.Round(amount, 2);
            userAccount.Balance -= amount;
            userAccount.Transactions.Add(new Transaction()
            {
                Id = TransactionIdGenerator(bankId, accountHolderId),
                Amount = amount,
                TransactionType = TransactionType.Debit,
                On = today.ToString("g"),
                TaxType=TaxType.None
            });
        }
        public decimal TaxCalculator(string userBankId,string beneficiaryBankId,decimal amount,TaxType taxType)
        {
            decimal tax = 0;
            if (taxType == TaxType.IMPS)
            {
                if (userBankId == beneficiaryBankId)
                {
                    tax = amount * (int)IMPSCharges.SameBank;
                }
                else
                {
                    tax = amount * (int)IMPSCharges.DifferentBank;
                }
            }
            else
            {
                if (userBankId == beneficiaryBankId)
                {
                    tax = amount * (int)RTGSCharges.SameBank;
                }
                else
                {
                    tax = amount * (int)RTGSCharges.DifferentBank;
                }
            }
            return tax/100;
        }
        public void Transfer(string userBankId, string userAccountId, decimal amount, string beneficiaryBankId, string beneficiaryAccountId, TaxType taxType)
        {
            this.accountHolder.InputValidator(userAccountId, beneficiaryAccountId, userBankId, beneficiaryBankId);

            Account userAccount = this.accountHolder.AccountFinder(userBankId, userAccountId);

            Account beneficiaryAccount = this.accountHolder.AccountFinder(beneficiaryBankId, beneficiaryAccountId);
            
            if (userAccount == null || beneficiaryAccount == null)
                throw new InvalidAccountNameException();

            amount = Math.Round(amount, 2);
            decimal tax = TaxCalculator(userBankId, beneficiaryBankId, amount,taxType);
            tax = Math.Round(tax, 2);

            if (userAccount.Balance < amount+tax)
                throw new InsufficientFundsException();

            string transactionId = TransactionIdGenerator(userBankId, userAccountId);

            userAccount.Balance -= amount+tax;
            userAccount.Transactions.Add(new Transaction()
            {
                Id = transactionId,
                Amount = amount,
                TransactionType = TransactionType.Debit,
                On = today.ToString("g"),
                Tax = tax,
                TaxType = taxType,
                SourceBankId = userBankId,
                SourceAccountId = userAccountId,
                DestinationBankId = beneficiaryBankId,
                DestinationAccountId = beneficiaryAccountId
            });

            beneficiaryAccount.Balance += amount;
            beneficiaryAccount.Transactions.Add(new Transaction()
            {
                Id = transactionId,
                Amount = amount,
                TransactionType = TransactionType.Credit,
                On = today.ToString("g"),
                Tax = tax,
                TaxType = taxType,
                DestinationBankId = userBankId,
                DestinationAccountId = userAccountId,
                SourceBankId = beneficiaryBankId,
                SourceAccountId = beneficiaryAccountId
            });
        }

        public List<Transaction> TransactionHistory(string bankId,string accountId)
        {
            this.accountHolder.InputValidator(bankId, accountId);
            Account account = this.accountHolder.AccountFinder(bankId, accountId);
            if (account == null)
            {
                throw new InvalidAccountNameException();
            }
            return account.Transactions;
        }
        public decimal ViewBalance(string bankId, string accountId)
        {
            this.accountHolder.InputValidator(bankId, accountId);
            Account account = this.accountHolder.AccountFinder(bankId, accountId);
            if (account == null)
            {
                throw new InvalidAccountNameException();
            }
            return account.Balance;
        }
    }
}