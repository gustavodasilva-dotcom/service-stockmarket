using System;
using System.Configuration;
using System.Collections.Generic;
using Service.StockMarket.Repositories;
using Service.StockMarket.Entities.JSON.Response;

namespace Service.StockMarket.Services
{
    public class CurrencyService
    {
        private readonly LogService _logService;

        private readonly RESTRepository _restRepository;

        private readonly DatabaseRepository _databaseRepository;

        public CurrencyService()
        {
            _logService = new LogService();

            _restRepository = new RESTRepository();

            _databaseRepository = new DatabaseRepository();
        }

        public void CheckCurrencyCountries()
        {
            try
            {
                var countries = GetCountries();

                foreach (var country in countries)
                {
                    CheckIfCurrencyExists(country.code2, country.currencyCode);
                }
            }
            catch (Exception) { throw; }
        }

        private IEnumerable<CountryResponse> GetCountries()
        {
            try
            {
                _restRepository.Endpoint = $"{ConfigurationManager.AppSettings["baseURL"]}{ConfigurationManager.AppSettings["country"]}{ConfigurationManager.AppSettings["token"]}";

                return _restRepository.GetCountries();
            }
            catch (Exception) { throw; }
        }

        private void CheckIfCurrencyExists(string code2, string currencyCode)
        {
            var query = string.Empty;
            
            try
            {
                var countBaseCurrencies = 0;
                
                var countCurrencies = 0;

                #region SQL

                query = $@"SELECT * FROM BaseCurrencies (NOLOCK) WHERE Name = '{currencyCode}';";

                #endregion

                var resultsBaseCurrencies = _databaseRepository.Execute(query);

                #region SQL

                query = $@"SELECT * FROM Currencies (NOLOCK) WHERE Exchange = '{code2}';";

                #endregion

                var resultsCurrencies = _databaseRepository.Execute(query);

                foreach (var result in resultsBaseCurrencies)
                    countBaseCurrencies += 1;

                foreach (var result in resultsCurrencies)
                    countCurrencies += 1;

                if (countBaseCurrencies != 0 && countCurrencies != 0)
                    _logService.Log($"There's already, at the database, a base currency {currencyCode} and a currency {code2} registered.");

                if (countBaseCurrencies == 0 && countCurrencies == 0)
                    InsertBaseCurrency(currencyCode, code2);

                if (countBaseCurrencies != 0 && countCurrencies == 0)
                {
                    _logService.Log($"The base currency {currencyCode} is registered, but the currency {code2} is not.");
                    _logService.Log($"Inserting currency {code2}.");

                    foreach (var result in resultsBaseCurrencies)
                        InsertCurrency(int.Parse(result.ID.ToString()), code2);
                }
            }
            catch (Exception e)
            {
                _logService.Log($"O seguinte erro ocorreu: {e.Message}");
                _logService.Log($"Query: {query}");

                throw;
            }
        }

        private void InsertBaseCurrency(string currencyCode, string code2)
        {
            var query = string.Empty;

            try
            {
                #region SQL

                query = $@"INSERT INTO BaseCurrencies (Name) VALUES('{currencyCode}');";

                #endregion

                _databaseRepository.Execute(query);

                #region SQL

                query = $@"SELECT * FROM BaseCurrencies WHERE Name = '{currencyCode}';";

                #endregion

                var baseCurrency = _databaseRepository.Execute(query);

                foreach (var data in baseCurrency)
                {
                    InsertCurrency(int.Parse(data.ID.ToString()), code2);
                }
            }
            catch (Exception e)
            {
                _logService.Log($"O seguinte erro ocorreu: {e.Message}");
                _logService.Log($"Query: {query}");
            }
        }

        private void InsertCurrency(int baseCurrencyID, string code2)
        {
            var query = string.Empty;

            try
            {
                #region SQL

                query = $@"INSERT INTO Currencies (Exchange, BaseCurrencyID) VALUES('{code2}', {baseCurrencyID});";

                #endregion

                _databaseRepository.Execute(query);
            }
            catch (Exception e)
            {
                _logService.Log($"O seguinte erro ocorreu: {e.Message}");
                _logService.Log($"Query: {query}");
            }
        }
    }
}
