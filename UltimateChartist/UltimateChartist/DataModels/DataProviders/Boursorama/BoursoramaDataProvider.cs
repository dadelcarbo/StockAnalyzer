using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Windows;

namespace UltimateChartist.DataModels.DataProviders.Boursorama;

public class BoursoramaDataProvider : StockDataProviderBase
{
    public override BarDuration[] BarDurations { get; } = { BarDuration.M_1, BarDuration.M_2,
        BarDuration.M_5, BarDuration.M_15, BarDuration.M_30, BarDuration.H_1, BarDuration.H_2, BarDuration.H_4, BarDuration.Daily, BarDuration.Weekly, BarDuration.Monthly };
    public override BarDuration DefaultBarDuration => BarDuration.Daily;

    public override string Name => "Boursorama";
    public override string DisplayName => "Boursorama";

    public override void InitDictionary()
    {
        InitCacheFolders();
    }

    public override List<StockBar> LoadData(Instrument instrument, BarDuration duration)
    {
        return DownloadData(instrument, duration);
    }
    public override List<StockBar> DownloadData(Instrument instrument, BarDuration duration)
    {
        List<StockBar> result = null;
        try
        {
            var symbol = instrument.Symbol;

            var period = duration switch
            {
                BarDuration.M_1 => -1,
                BarDuration.M_2 => -2,
                BarDuration.M_5 => -5,
                BarDuration.M_15 => -15,
                BarDuration.M_30 => -30,
                BarDuration.H_1 => -60,
                BarDuration.H_2 => -120,
                BarDuration.H_4 => -240,
                BarDuration.Daily => 0,
                BarDuration.Weekly => 7,
                BarDuration.Monthly => 30,
                _ => throw new ArgumentException($"BarDuration {duration} not supported")
            };
            var length = duration switch
            {
                BarDuration.M_1 => 5,
                BarDuration.M_2 => 5,
                BarDuration.M_5 => 5,
                BarDuration.M_15 => 5,
                BarDuration.M_30 => 5,
                BarDuration.H_1 => 5,
                BarDuration.H_2 => 5,
                BarDuration.H_4 => 5,
                BarDuration.Daily => 180,
                BarDuration.Weekly => 365,
                BarDuration.Monthly => 1825,
                _ => throw new ArgumentException($"BarDuration {duration} not supported")
            };

            var client = new HttpClient();
            var url = $"https://www.boursorama.com/bourse/action/graph/ws/GetTicksEOD?symbol={symbol}&length={length}&period={period}&guid=";
            var response = client.GetStringAsync(url).Result;
            if (response == "[]")
                return null;
            if (period >= 0)
            {
                var refDate = new DateTime(1970, 01, 01);
                var boursoData = JsonSerializer.Deserialize<BoursoData>(response);
                result = boursoData?.d?.QuoteTab?.Select(d => new StockBar(refDate.AddDays(d.d), d.o, d.h, d.l, d.c, d.v)).ToList();
            }
            else
            {
                var boursoData = JsonSerializer.Deserialize<BoursoData>(response);
                result = boursoData?.d?.QuoteTab?.Select(d => new StockBar(BoursoIntradayDateToDateTime(d.d), d.o, d.h, d.l, d.c, d.v)).ToList();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        return result;
    }
    DateTime BoursoIntradayDateToDateTime(long d)
    {
        var dateString = d.ToString();

        var year = 2000 + int.Parse(dateString.Substring(0, 2));
        var month = int.Parse(dateString.Substring(2, 2));
        var day = int.Parse(dateString.Substring(4, 2));

        var minutes = int.Parse(dateString.Substring(6, 4));

        return new DateTime(year, month, day).AddMinutes(minutes);
    }
}

public class BoursoData
{
    public Datum d { get; set; }
}
public class Datum
{
    public string Name { get; set; }
    public string SymbolId { get; set; }
    public Quotetab[] QuoteTab { get; set; }
}
public class Quotetab
{
    public long d { get; set; }
    public decimal o { get; set; }
    public decimal h { get; set; }
    public decimal l { get; set; }
    public decimal c { get; set; }
    public long v { get; set; }
}

