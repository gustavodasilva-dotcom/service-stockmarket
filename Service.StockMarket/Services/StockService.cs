using System;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;
using Service.StockMarket.Repositories;
using Service.StockMarket.Entities.Entities;
using Service.StockMarket.Entities.JSON.Response;

namespace Service.StockMarket.Services
{
    public class StockService
    {
        private readonly LogService _logService;

        private readonly RESTRepository _restRepository;

        private readonly DatabaseRepository _databaseRepository;

        public StockService()
        {
            _logService = new LogService();

            _restRepository = new RESTRepository();

            _databaseRepository = new DatabaseRepository();
        }

        public void CheckStockSymbols()
        {
            try
            {
                var stocks = GetStocks();

                if (stocks.Any())
                {
                    foreach (var stock in stocks)
                    {
                        if (!CompanyProfileExists(stock.Symbol))
                        {
                            _logService.Log($"Accessing endpoint for the company profileof the stock {stock.Symbol}.");

                            _restRepository.Endpoint = $"{ConfigurationManager.AppSettings["baseURL"]}{ConfigurationManager.AppSettings["profile"]}{stock.Symbol}&{ConfigurationManager.AppSettings["token"]}";

                            var companyProfile = _restRepository.GetCompanyProfile();

                            if (!companyProfile.Equals(null))
                                GetCompanyProfileData(companyProfile, stock.Symbol);
                            else
                                _logService.Log($"It wasn't found a company profile for the symbol {stock.Symbol} at the API.");

                            // TODO: Implement timer to wait 2 minutes after each request.
                        }
                        else
                            _logService.Log($"There's already a company profile registered for the symbol {stock.Symbol}.");
                    }
                }
            }
            catch (Exception) { throw; }
        }

        private bool CompanyProfileExists(string symbol)
        {
            var query = string.Empty;

            try
            {
                #region SQL

                query =
                $@" SELECT		1
                    FROM		CompaniesProfile CP (NOLOCK)
                    INNER JOIN	Symbols			 SB (NOLOCK) ON CP.SymbolID = SB.ID
                    WHERE		SB.Symbol = '{symbol}';";

                #endregion

                var results = _databaseRepository.Execute(query);

                var count = 0;

                foreach (var result in results)
                    count += 1;

                if (count == 0)
                    return false;
                else
                    return true;
            }
            catch (Exception e)
            {
                _logService.Log($"O seguinte erro ocorreu: {e.Message}");
                _logService.Log($"Query: {query}");

                throw;
            }
        }

        private IEnumerable<SymbolEntity> GetStocks()
        {
            var query = string.Empty;

            var stocks = new List<SymbolEntity>();

            try
            {
                #region SQL

                query = @"SELECT * FROM Symbols (NOLOCK) WHERE Active = 1 AND Deleted = 0;";

                #endregion

                var results = _databaseRepository.Execute(query);

                foreach (var result in results)
                {
                    stocks.Add(new SymbolEntity
                    {
                        DisplaySymbol = result.DisplaySymbol.ToString(),
                        Symbol = result.Symbol.ToString()
                    });
                }

                return stocks;
            }
            catch (Exception e)
            {
                _logService.Log($"O seguinte erro ocorreu: {e.Message}");
                _logService.Log($"Query: {query}");

                throw;
            }
        }

        private void GetCompanyProfileData(ProfileResponse response, string symbol)
        {
            var query = string.Empty;

            try
            {
                int symbolID = 0, currencyID = 0;

                _logService.Log($"Validating data for symbol {symbol}.");

                #region SQL

                query = $@"SELECT * FROM Symbols (NOLOCK) WHERE Symbol = '{symbol}';";

                #endregion

                var dataFromDB = _databaseRepository.Execute(query);

                foreach (var data in dataFromDB)
                    symbolID = int.Parse(data.ID.ToString());

                #region SQL

                query = $@"SELECT * FROM Currencies (NOLOCK) WHERE Exchange = '{response.country}';";

                #endregion

                dataFromDB = _databaseRepository.Execute(query);

                foreach (var data in dataFromDB)
                    currencyID = int.Parse(data.ID.ToString());

                var companyProfile = new CompanyProfileEntity
                {
                    SymbolID = symbolID,
                    CurrencyID = currencyID
                };

                ValidateData(companyProfile, response);
            }
            catch (Exception e)
            {
                _logService.Log($"O seguinte erro ocorreu: {e.Message}");
                _logService.Log($"Query: {query}");
            }
        }

        private void ValidateData(CompanyProfileEntity companyProfile, ProfileResponse response)
        {
            var errorMessage = string.Empty;

            try
            {
                var isDate = DateTime.TryParse(response.ipo, out DateTime ipo);

                if (isDate)
                    companyProfile.Ipo = ipo;
                else
                    errorMessage = $"The Ipo date, from the symbol {response.ipo}, doesn't correspond to a date value.";

                if (string.IsNullOrEmpty(errorMessage))
                {
                    if (companyProfile.SymbolID.Equals(0))
                        errorMessage = $"There's no data registered, at the database, for the symbol {response.ticker}.";
                }

                if (string.IsNullOrEmpty(errorMessage))
                {
                    if (companyProfile.CurrencyID.Equals(0))
                        errorMessage = $"There's no data registered, at the database, for the currency {response.country}.";
                }

                if (string.IsNullOrEmpty(errorMessage))
                {
                    companyProfile.Logo = response.logo;
                    companyProfile.MarketCapitalization = response.marketCapitalization;
                    companyProfile.Name = response.name;
                    companyProfile.Phone = response.phone;
                    companyProfile.ShareOutstanding = response.shareOutstanding;
                    companyProfile.Ticker = response.ticker;
                    companyProfile.Weburl = response.weburl;

                    InsertCompanyProfile(companyProfile);
                }
                else
                {
                    _logService.Log(errorMessage);
                }
            }
            catch (Exception e)
            {
                _logService.Log($"O seguinte erro ocorreu: {e.Message}");
            }
        }

        private void InsertCompanyProfile(CompanyProfileEntity companyProfile)
        {
            var query = string.Empty;

            try
            {
                #region SQL

                query =
                $@" INSERT INTO CompaniesProfile
                    (
                    	 Ipo
                    	,Logo
                    	,MarketCapitalization
                    	,Name
                    	,Phone
                    	,ShareOutstanding
                    	,Ticker
                    	,Weburl
                    	,SymbolID
                    	,CurrencyID
                    )
                    VALUES
                    (
                    	 '{companyProfile.Ipo}'
                    	,'{companyProfile.Logo.Trim()}'
                    	,'{companyProfile.MarketCapitalization.ToString().Replace(",", ".")}'
                    	,'{companyProfile.Name.Trim().Replace("'", "")}'
                    	,'{companyProfile.Phone.Trim()}'
                    	,'{companyProfile.ShareOutstanding.ToString().Replace(",", ".")}'
                    	,'{companyProfile.Ticker.Trim()}'
                    	,'{companyProfile.Weburl.Trim()}'
                    	,{companyProfile.SymbolID}
                    	,{companyProfile.CurrencyID}
                    );";

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
