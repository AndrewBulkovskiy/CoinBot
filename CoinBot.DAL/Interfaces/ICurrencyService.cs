using CoinBot.DAL.DTO;
using CoinBot.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CoinBot.DAL.Services.CoinMarketCapService;

namespace CoinBot.DAL.Interfaces
{
    public interface ICurrencyService
    {
        List<CurrencyDTO> Portfolio { get; }

        event PortfolioEvent OnPortfolioGrowth;

        CurrencyDTO GetCurrencyByNameOrSymbol(string currencyNameOrSymbol);
        void AddCurrencyToPortfolio(CurrencyDTO currency);
        void RemoveCurrencyFromPortfolio(CurrencyDTO currency);
        bool IsCurrencyAvaliable(string currencyNameOrSymbol);
        bool IsCurrencyInPortfolio(CurrencyDTO currency);
        void RefreshPortfolio();
        void StartTrackingPortfolio(double percentageValue);
        void StopTrackingPortfolio();
    }
}
