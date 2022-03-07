using System;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;
using Service.StockMarket.Repositories;
using Service.StockMarket.Entities.Entities;
using Service.StockMarket.Entities.JSON.Response;

namespace Service.StockMarket.Services
{
    public class SymbolService
    {
        private readonly LogService _logService;

        private readonly RESTRepository _restRepository;

        private readonly DatabaseRepository _databaseRepository;

        public SymbolService()
        {
            _logService = new LogService();

            _restRepository = new RESTRepository();

            _databaseRepository = new DatabaseRepository();
        }

        public IEnumerable<SymbolResponse> GetSymbolsInfo()
        {
            try
            {
                var currencies = GetCurrencies();

                if (currencies.Any())
                {
                    foreach (var currency in currencies)
                    {
                        _restRepository.Endpoint = $"{ConfigurationManager.AppSettings["baseURL"]}{ConfigurationManager.AppSettings["symbol"]}{currency}&{ConfigurationManager.AppSettings["token"]}";

                        return _restRepository.GetSymbols();
                    }
                }
                else
                {
                    _logService.Log("There's no currencies registered at the database or an error ocurred while connecting.");
                }

                return null;
            }
            catch (Exception) { throw; }
        }

        public void CheckSymbols(IEnumerable<SymbolResponse> symbols)
        {
            try
            {
                foreach (var symbol in symbols)
                {
                    if (string.IsNullOrEmpty(VerifySymbolValues(symbol)))
                    {
                        if (!SymbolExists(symbol.symbol))
                        {
                            var typeID = 0;

                            if (!string.IsNullOrEmpty(symbol.type))
                                typeID = GetStockTypeID(symbol.type);

                            InsertSymbol(symbol, typeID);

                            if (SymbolExists(symbol.symbol))
                                _logService.Log($"Symbol {symbol.symbol} registered successfully.");
                            else
                                _logService.Log($"Symbol {symbol.symbol} was suposed to be registered, but it wasn't.");
                        }
                        else
                        {
                            _logService.Log($"Symbol {symbol.symbol} is already registered at the database.");
                        }
                    }
                }
            }
            catch (Exception) { throw; }
        }

        public string VerifySymbolValues(SymbolResponse symbol)
        {
            try
            {
                var validation = string.Empty;

                if (string.IsNullOrEmpty(symbol.displaySymbol)) validation = $"The display symbol cannot be null or empty";
                if (string.IsNullOrEmpty(symbol.symbol)) validation = $"The symbol cannot be null or empty.";

                if (!string.IsNullOrEmpty(symbol.type))
                    if (!StockTypeExists(symbol.type)) validation = $"It was not possible to insert the symbol {symbol.description}.";

                if (!string.IsNullOrEmpty(validation)) _logService.Log($"Error while inserting {symbol.symbol}: {validation}");

                return validation;
            }
            catch (Exception) { throw; }
        }

        private List<string> GetCurrencies()
        {
            var currencies = new List<string>();

            #region SQL

            var query =
            @"  SELECT		C.*
                FROM		Currencies		C  (NOLOCK)
                INNER JOIN	BaseCurrencies	BC (NOLOCK)
                	ON BC.ID = C.BaseCurrencyID
                WHERE		C.Deleted	= 0
                  AND		C.Active	= 1
                  AND		BC.Deleted	= 0
                  AND		BC.Active	= 1;";

            #endregion

            try
            {
                var getCurrencies = _databaseRepository.Execute(query);

                if (getCurrencies != null)
                    foreach (var currency in getCurrencies) currencies.Add(currency.Exchange.ToString());

                return currencies;
            }
            catch (Exception e)
            {
                _logService.Log($"O seguinte erro ocorreu: {e.Message}");
                _logService.Log($"Query: {query}");

                throw;
            }
        }

        private bool StockTypeExists(string type)
        {
            var query = string.Empty;

            try
            {
                #region SQL

                query = $@"SELECT 1 FROM StockTypes WHERE Description = '{type.Trim()}' AND Active = 1 AND Deleted = 0;";

                #endregion

                var results = _databaseRepository.Execute(query);

                var count = 0;

                foreach (var result in results)
                    count += 1;

                if (count == 0)
                {
                    InsertType(type);

                    return false;
                }
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

        private bool SymbolExists(string symbol)
        {
            var query = string.Empty;

            try
            {
                #region SQL

                query = $@"SELECT 1 FROM Symbols WHERE Symbol = '{symbol.Trim()}' AND Active = 1 AND Deleted = 0;";

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

        private int GetStockTypeID(string stockType)
        {
            var query = string.Empty;

            try
            {
                #region SQL

                query = $@"SELECT * FROM StockTypes WHERE Description = '{stockType.Trim()}' AND Active = 1 AND Deleted = 0;";

                #endregion

                var type = _databaseRepository.Execute(query);

                foreach (var line in type)
                    return int.Parse(line.ID.ToString());

                return 0;
            }
            catch (Exception e)
            {
                _logService.Log($"O seguinte erro ocorreu: {e.Message}");
                _logService.Log($"Query: {query}");

                throw;
            }
        }

        private void InsertSymbol(SymbolResponse symbol, int stockTypeID)
        {
            var query = string.Empty;

            try
            {
                var newSymbol = new SymbolEntity()
                {
                    Description = symbol.description.Trim().Replace("'", ""),
                    DisplaySymbol = symbol.displaySymbol.Trim(),
                    Figi = string.IsNullOrEmpty(symbol.figi) ? null : symbol.figi.Trim(),
                    Isin = string.IsNullOrEmpty(symbol.isin) ? null : symbol.isin.Trim(),
                    Mic = string.IsNullOrEmpty(symbol.mic) ? null : symbol.mic.Trim(),
                    ShareClassFIGI = string.IsNullOrEmpty(symbol.shareClassFIGI) ? null : symbol.shareClassFIGI.Trim(),
                    Symbol = string.IsNullOrEmpty(symbol.symbol) ? null : symbol.symbol.Trim(),
                    Symbol2 = string.IsNullOrEmpty(symbol.symbol2) ? null : symbol.symbol2.Trim(),
                    Type = stockTypeID.Equals(0) ? null : stockTypeID.ToString()
                };

                #region SQL

                if (string.IsNullOrEmpty(newSymbol.Type))
                {
                    query =
                    $@"  INSERT INTO Symbols
                        (
                        	 Description
                        	,DisplaySymbol
                        	,Figi
                        	,Isin
                        	,Mic
                        	,ShareClassFIGI
                        	,Symbol
                        	,Symbol2
                        	,StokeTypeID
                        )
                        VALUES
                        (
                        	 '{newSymbol.Description}'
                        	,'{newSymbol.DisplaySymbol}'
                        	,'{newSymbol.Figi}'
                        	,'{newSymbol.Isin}'
                        	,'{newSymbol.Mic}'
                        	,'{newSymbol.ShareClassFIGI}'
                        	,'{newSymbol.Symbol}'
                        	,'{newSymbol.Symbol2}'
                        	,null
                        );";
                }
                else
                {
                    query =
                    $@"  INSERT INTO Symbols
                        (
                        	 Description
                        	,DisplaySymbol
                        	,Figi
                        	,Isin
                        	,Mic
                        	,ShareClassFIGI
                        	,Symbol
                        	,Symbol2
                        	,StokeTypeID
                        )
                        VALUES
                        (
                        	 '{newSymbol.Description}'
                        	,'{newSymbol.DisplaySymbol}'
                        	,'{newSymbol.Figi}'
                        	,'{newSymbol.Isin}'
                        	,'{newSymbol.Mic}'
                        	,'{newSymbol.ShareClassFIGI}'
                        	,'{newSymbol.Symbol}'
                        	,'{newSymbol.Symbol2}'
                        	,'{newSymbol.Type}'
                        );";
                }

                #endregion

                _databaseRepository.Execute(query);
            }
            catch (Exception e)
            {
                _logService.Log($"O seguinte erro ocorreu: {e.Message}");
                _logService.Log($"Query: {query}");
            }
        }

        private void InsertType(string type)
        {
            var query = string.Empty;

            try
            {
                #region SQL

                query = $@"INSERT INTO StockTypes (Description) VALUES ('{type}');";

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
