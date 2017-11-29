using CoinBot.DAL.DTO;
using CoinBot.DAL.Entities;
using CoinBot.DAL.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CoinBot.DAL.Services
{
    [Serializable]
    public class CoinMarketCapService : ICurrencyService
    {
        // This variable (_avaliableCurrencies) saves network requests (for example in Add currency command)
        // We do not need to send network request just to check if currency name is right
        private List<CurrencyDTO> _avaliableCurrencies;
        private static Timer _timer;
        private string _apiUrl = "https://api.coinmarketcap.com/v1/ticker/";
        private int _timerInterval = 10000;
        private double _portfolioGrowthPercentage;

        public CoinMarketCapService()
        {
            //_provider = new CoinMarketCapProvider();
            Portfolio = new List<CurrencyDTO>();
            RefreshAvaliableCurrenciesData();
            _timer = new Timer(_timerInterval);
            _timer.Elapsed += new ElapsedEventHandler(CheckPortfolioGrowth);
            //_timer.Enabled = true;
            //_timer.Stop();
        }

        public List<CurrencyDTO> Portfolio { get; }

        public delegate void PortfolioEvent();
        public event PortfolioEvent OnPortfolioGrowth;

        // Returns fresh updated currency
        public CurrencyDTO GetCurrencyByNameOrSymbol(string currencyNameOrSymbol)
        {
            var currency = _avaliableCurrencies.Where(c => c.Name == currencyNameOrSymbol || c.Symbol == currencyNameOrSymbol)
                    .FirstOrDefault();
            if (currency != null)
                return this.GetCurrencyById(currency.Id);
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
            var currency = _avaliableCurrencies.Where(c => c.Name == currencyNameOrSymbol || c.Symbol == currencyNameOrSymbol)
                    .FirstOrDefault();
            return (currency != null) ? true : false;
        }

        public bool IsCurrencyInPortfolio(CurrencyDTO currency)
        {
            var contains = Portfolio.Where(c => c.Equals(currency)).FirstOrDefault();
            if (contains != null)
                return true;
            return false;
        }

        // This method should be synchronous because we can`t work with half-updated potfolio
        public void RefreshPortfolio()
        {
            for (int i = 0; i < Portfolio.Count; i++)
            {
                var tempCurrency = this.GetCurrencyById(Portfolio[i].Id);
                if (tempCurrency != null)
                    Portfolio[i] = tempCurrency;
            }
        }

        public void StartTrackingPortfolio(double percentage)
        {
            this._portfolioGrowthPercentage = percentage;
            _timer.Start();
        }

        public void StopTrackingPortfolio()
        {
            _timer.Stop();
        }

        // This method should be synchronous because we can`t work with half-updated list of avaliable currencies
        private void RefreshAvaliableCurrenciesData()
        {
            _avaliableCurrencies = this.GetAllCurrencies();
        }

        private void CheckPortfolioGrowth(object sender, ElapsedEventArgs e)
        {
            for (int i = 0; i < Portfolio.Count; i++)
            {
                var tempCurrency = this.GetCurrencyById(Portfolio[i].Id);
                if (tempCurrency != null)
                {
                    double oldValue = Portfolio[i].Price*Portfolio[i].Multiplier;
                    double newValue = tempCurrency.Price*tempCurrency.Multiplier;
                    double percentageGrowth = ((newValue - oldValue) / Math.Truncate((newValue + oldValue) / 2.0)) * 100.0;
                    if (percentageGrowth >= _portfolioGrowthPercentage)
                    {
                        OnPortfolioGrowth?.Invoke();
                    }
                }

            }
        }

        #region Getting data form WebApi

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
                    }
                }
            }

            return CurrencyDTO.Convert(currency);
        }


        public async Task<CurrencyDTO> GetCurrencyByIdAsync(string currencyId)
        {
            Currency currency = null;
            string urlPath = _apiUrl + currencyId;

            using (var client = new HttpClient())
            //using (var response = await client.GetAsync(urlPath))
            using (var response = client.GetAsync(urlPath).Result)
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
                    }
                }
            }

            return CurrencyDTO.Convert(currency);
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
                    }

                }
            }

            List<CurrencyDTO> result = new List<CurrencyDTO>();
            foreach (var currency in currencies)
            {
                if (currency != null)
                    result.Add(CurrencyDTO.Convert(currency));
            }

            return result;
        }

        public async Task<List<CurrencyDTO>> GetAllCurrenciesAsync()
        {
            List<Currency> currencies = null;
            string urlPath = _apiUrl;

            using (var client = new HttpClient())
            //using (var response = await client.GetAsync(urlPath))
            using (var response = client.GetAsync(urlPath).Result)
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
                    }
                }
            }

            List<CurrencyDTO> result = new List<CurrencyDTO>();
            foreach (var currency in currencies)
            {
                if (currency != null)
                    result.Add(CurrencyDTO.Convert(currency));
            }

            return result;
        }
        #endregion

    }
}
