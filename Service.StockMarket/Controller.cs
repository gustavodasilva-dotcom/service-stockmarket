using System;
using Service.StockMarket.Services;

namespace Service.StockMarket
{
    public class Controller
    {
        private readonly LogService _logService;

        private readonly StockService _stockService;

        private readonly SymbolService _symbolService;

        private readonly CurrencyService _currencyService;

        public Controller()
        {
            _logService = new LogService();

            _stockService = new StockService();

            _symbolService = new SymbolService();

            _currencyService = new CurrencyService();
        }

        public void Run()
        {
            try
            {
                _currencyService.CheckCurrencyCountries();

                var symbols = _symbolService.GetSymbolsInfo();

                if (!symbols.Equals(null))
                {
                    _symbolService.CheckSymbols(symbols);

                    _stockService.CheckStockSymbols();
                }
            }
            catch (Exception e)
            {
                _logService.Log(e.Message);

                Run();
            }
        }
    }
}
