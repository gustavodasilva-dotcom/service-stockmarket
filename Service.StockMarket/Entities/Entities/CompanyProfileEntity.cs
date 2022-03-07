﻿using System;

namespace Service.StockMarket.Entities.Entities
{
    public class CompanyProfileEntity
    {
        public DateTime Ipo { get; set; }

        public string Logo { get; set; }

        public double MarketCapitalization { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public double ShareOutstanding { get; set; }

        public string Ticker { get; set; }

        public string Weburl { get; set; }

        public int SymbolID { get; set; }

        public int CurrencyID { get; set; }
    }
}
