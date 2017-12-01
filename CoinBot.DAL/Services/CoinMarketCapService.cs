using CoinBot.DAL.DTO;
using CoinBot.DAL.Entities;
using CoinBot.DAL.Infrastructure;
using CoinBot.DAL.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CoinBot.DAL.Services
{
    [Serializable]
    public class CoinMarketCapService : ICurrencyService
    {
        public List<CurrencyDTO> Portfolio { get; }

        public delegate void PortfolioEvent();

        //In real life Portfolio will be stored in DB or  Azure Table i think
        public event PortfolioEvent OnPortfolioGrowth;

        // This variable (_avaliableCurrencies) saves network requests (for example in Add currency command)
        // We do not need to send network request just to check if currency name is right
        private List<CurrencyDTO> _avaliableCurrencies;
        private string _apiUrl = "https://api.coinmarketcap.com/v1/ticker/";
        private int _timerInterval = 60000;
        private decimal _portfolioGrowthPercentage;

        public CoinMarketCapService()
        {
            Portfolio = new List<CurrencyDTO>();
            RefreshAvaliableCurrenciesData();
        }

        // Returns fresh updated currency
        public CurrencyDTO GetCurrencyByNameOrSymbol(string currencyNameOrSymbol)
        {
            if (currencyNameOrSymbol == null || string.IsNullOrWhiteSpace(currencyNameOrSymbol))
                return null;

            string filteredName = char.ToUpper(currencyNameOrSymbol[0]) + currencyNameOrSymbol.Substring(1);
            string filteredSymbol = currencyNameOrSymbol.ToUpper();

            var currency = _avaliableCurrencies.Where(c => c.Name == filteredName || c.Symbol == filteredSymbol)
                    .FirstOrDefault();

            if (currency != null)
                return this.GetCurrencyById(currency.Id);
            else
                return null;
        }

        public void AddCurrencyToPortfolio(CurrencyDTO currency)
        {
            if (currency != null)
                Portfolio.Add(currency);
        }

        public void RemoveCurrencyFromPortfolio(CurrencyDTO currency)
        {
            if (currency != null)
                Portfolio.Remove(currency);
        }

        public bool IsCurrencyAvaliable(string currencyNameOrSymbol)
        {
            if (currencyNameOrSymbol == null || string.IsNullOrWhiteSpace(currencyNameOrSymbol))
                return false;

            string filteredName = char.ToUpper(currencyNameOrSymbol[0]) + currencyNameOrSymbol.Substring(1);
            string filteredSymbol = currencyNameOrSymbol.ToUpper();

            var currency = _avaliableCurrencies.Where(c => c.Name == filteredName || c.Symbol == filteredSymbol)
                    .FirstOrDefault();

            return (currency != null) ? true : false;
        }

        public bool IsCurrencyInPortfolio(CurrencyDTO currency)
        {
            var contains = Portfolio.Where(c => c.Equals(currency)).FirstOrDefault();
            if (contains != null)
                return true;
            else
                return false;
        }

        // This method should be synchronous because we can`t work with half-updated potfolio
        public void RefreshPortfolio()
        {
            for (int i = 0; i < Portfolio.Count; i++)
            {
                var tempCurrency = this.GetCurrencyById(Portfolio[i].Id);
                if (tempCurrency != null)
                {
                    tempCurrency.Multiplier = Portfolio[i].Multiplier;
                    Portfolio[i] = tempCurrency;
                }
            }
        }

        public void StartTrackingPortfolio(decimal percentage)
        {
            this._portfolioGrowthPercentage = percentage;
            Ticker.Start(_timerInterval);
            Ticker.OnTick += CheckPortfolioGrowth;
        }

        public void StopTrackingPortfolio()
        {
            Ticker.Stop();
        }

        // This method should be synchronous because we can`t work with half-updated list of avaliable currencies
        private void RefreshAvaliableCurrenciesData()
        {
            _avaliableCurrencies = this.GetAllCurrencies();
        }

        private void CheckPortfolioGrowth()
        {
            decimal oldTotalValue = 0.0m;
            decimal newTotalValue = 0.0m;

            for (int i = 0; i < Portfolio.Count; i++)
            {
                var tempCurrency = this.GetCurrencyById(Portfolio[i].Id);
                if (tempCurrency != null)
                {
                    oldTotalValue += Portfolio[i].Price * Portfolio[i].Multiplier;
                    newTotalValue += tempCurrency.Price * tempCurrency.Multiplier;
                }
            }

            decimal percentageGrowth = ((newTotalValue - oldTotalValue) / Math.Truncate((newTotalValue + oldTotalValue) / 2.0m)) * 100.0m;

            if (percentageGrowth >= _portfolioGrowthPercentage) 
            {
                OnPortfolioGrowth?.Invoke();
            }
        }


        #region Getting data form Api

        public CurrencyDTO GetCurrencyById(string currencyId)
        {
            Currency currency = new Currency();
            string urlPath = _apiUrl + currencyId;

            using (var client = new HttpClient())
            using (var response = client.GetAsync(urlPath).Result)
            using (var content = response.Content)
            {
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var apiResponse = content.ReadAsStringAsync().Result;
                        var serializedResponse = JsonConvert.DeserializeObject<List<Currency>>(apiResponse);
                        currency = serializedResponse[0];
                    }
                    catch (JsonSerializationException)
                    {
                        // Add handling
                    }
                }
            }

            return CurrencyDTO.ConvertToCurrencyDTO(currency);
        }


        public async Task<CurrencyDTO> GetCurrencyByIdAsync(string currencyId)
        {
            Currency currency = null;
            string urlPath = _apiUrl + currencyId;

            using (var client = new HttpClient())
            using (var response = await client.GetAsync(urlPath))
            using (var content = response.Content)
            {
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var apiResponse = await content.ReadAsStringAsync();
                        var serializedResponse = JsonConvert.DeserializeObject<List<Currency>>(apiResponse);
                        currency = serializedResponse[0];
                    }
                    catch (JsonSerializationException)
                    {
                        // Add handling
                    }
                }
            }

            return CurrencyDTO.ConvertToCurrencyDTO(currency);
        }

        public List<CurrencyDTO> GetAllCurrencies()
        {
            List<Currency> currencies = null;
            string urlPath = _apiUrl;

            using (var client = new HttpClient())
            using (var response = client.GetAsync(urlPath).Result)
            using (var content = response.Content)
            {
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var apiResponse = content.ReadAsStringAsync().Result;
                        currencies = JsonConvert.DeserializeObject<List<Currency>>(apiResponse);
                    }
                    catch (JsonSerializationException)
                    {
                        // Add handling
                    }
                }
            }

            List<CurrencyDTO> result = new List<CurrencyDTO>();
            foreach (var currency in currencies)
            {
                if (currency != null)
                    result.Add(CurrencyDTO.ConvertToCurrencyDTO(currency));
            }

            return result;
        }

        public async Task<List<CurrencyDTO>> GetAllCurrenciesAsync()
        {
            List<Currency> currencies = null;
            string urlPath = _apiUrl;

            using (var client = new HttpClient())
            using (var response = await client.GetAsync(urlPath))
            using (var content = response.Content)
            {
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var apiResponse = await content.ReadAsStringAsync();
                        currencies = JsonConvert.DeserializeObject<List<Currency>>(apiResponse);
                    }
                    catch (JsonSerializationException)
                    {
                        // Add handling
                    }
                }
            }

            List<CurrencyDTO> result = new List<CurrencyDTO>();
            foreach (var currency in currencies)
            {
                if (currency != null)
                    result.Add(CurrencyDTO.ConvertToCurrencyDTO(currency));
            }

            return result;
        }
        #endregion

    }
}
