﻿using StockAnalyzer;
using StockAnalyzer.StockBinckPortfolio;
using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;

namespace StockAnalyzerApp.CustomControl.BinckPortfolioDlg.TradeDlgs
{
    public class OpenTradeViewModel : NotifyPropertyChangedBase
    {
        private int entryQty;
        private float entryValue;
        private float stopValue;
        private DateTime entryDate;

        public string StockName { get; set; }

        private void OnEntryChanged()
        {
            this.OnPropertyChanged("EntryQty");
            this.OnPropertyChanged("EntryValue");
            this.OnPropertyChanged("EntryCost");
            this.OnPropertyChanged("Fee");

            this.OnPropertyChanged("StopValue");
            this.OnPropertyChanged("TradeRisk");
            this.OnPropertyChanged("PortfolioRisk");
            this.OnPropertyChanged("PortfolioPercent");
        }

        public int EntryQty
        {
            get => entryQty;
            set
            {
                if (entryQty != value)
                {
                    entryQty = value;
                    OnEntryChanged();
                }
            }
        }
        public float EntryValue
        {
            get => entryValue;
            set
            {
                if (entryValue != value)
                {
                    entryValue = value;
                    OnEntryChanged();
                }
            }
        }
        public float EntryCost => EntryQty * EntryValue + Fee;
        public float Fee => (EntryQty * EntryValue) < 1000f ? 2.5f : 5.0f;
        public float StopValue
        {
            get => stopValue;
            set
            {
                if (stopValue != value)
                {
                    stopValue = value;
                    OnEntryChanged();
                }
            }
        }
        public float TradeRisk => (EntryValue - StopValue) / EntryValue;
        public float PortfolioPercent => 1f - (this.Portfolio.TotalValue - this.EntryCost) / this.Portfolio.TotalValue;
        public float PortfolioRisk => (EntryValue - StopValue) * EntryQty / this.Portfolio.TotalValue;
        public float PortfolioReturn => (this.Portfolio.TotalValue - this.Portfolio.InitialBalance) / this.Portfolio.TotalValue;
        public DateTime EntryDate
        {
            get => entryDate;
            set
            {
                if (entryDate != value)
                {
                    entryDate = value;
                    this.OnPropertyChanged("EntryDate");
                }
            }
        }
        public TimeSpan EntryTime
        {
            get => entryDate.TimeOfDay;
            set
            {
                if (entryDate.TimeOfDay != value)
                {
                    entryDate = entryDate.Date.Add(value);
                    this.OnPropertyChanged("EntryDate");
                }
            }
        }
        public StockBarDuration BarDuration { get; set; }
        public string IndicatorName { get; set; }
        public string EntryComment { get; set; }
        public static Array BarDurations => Enum.GetValues(typeof(BarDuration));
        public static IList<int> LineBreaks => new List<int> { 0, 1, 2, 3, 4, 5 };
        public IList<string> IndicatorNames { get; set; }
        public StockPortfolio Portfolio { get; set; }
    }
}
