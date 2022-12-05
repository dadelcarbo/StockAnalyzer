using Newtonsoft.Json;
using Saxo.OpenAPI.AuthenticationServices;
using Saxo.OpenAPI.TradingServices;
using System;
using System.Collections.Generic;
using System.IO;

namespace Saxo.OpenAPI.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // https://www.developer.saxo/openapi/referencedocs
                
                var clientId = "13040737";
                var appPath = Path.Combine(AppContext.BaseDirectory, "App_live.json");
                //clientId = "17236368";
                //appPath = Path.Combine(AppContext.BaseDirectory, "App_sim.json");

                var session = LoginService.Login(clientId, appPath);

                Console.WriteLine(session.Token.AccessToken);

                var accounts = new AccountService().GetAccounts();
                //Console.WriteLine(JsonConvert.SerializeObject(accounts, Formatting.Indented));
                //Console.WriteLine("================================ ");

                foreach (var account in accounts)
                {
                    var positions = new AccountService().GetPositions(account);
                    Console.WriteLine(JsonConvert.SerializeObject(positions, Formatting.Indented));
                    Console.WriteLine("================================ ");
                    File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), account.AccountId.Replace("/", "_") + "_Pos.json"), JsonConvert.SerializeObject(positions, Formatting.Indented));

                    var orders = new OrderService().GetOrders(account);
                    Console.WriteLine(JsonConvert.SerializeObject(orders, Formatting.Indented));
                    Console.WriteLine("================================ ");
                    File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), account.AccountId.Replace("/", "_") + "_Order.json"), JsonConvert.SerializeObject(orders, Formatting.Indented));

                    foreach (var position in positions)
                    {
                        var instrument = new InstrumentService().GetInstrumentById(position.PositionBase.Uic, account);
                        if (instrument == null)
                            continue;
                        Console.WriteLine($"{instrument.Symbol} {instrument.Description} {position.PositionBase.Amount} {position.PositionBase.OpenPrice}");
                    }
                }

                foreach (var isin in new Dictionary<string, string>() { { "FR0013214145", "SMCP" }, { "BE0974258874", "BEKAERT" }, { "NL0000852564", "AALBERTS" }, { "PTEDP0AM0009", "EDP" }, { "CH1150298557", "TURBO_GOLD" } })
                {
                    var instrument = new InstrumentService().GetInstrumentByIsin(isin.Key);
                    Console.WriteLine(JsonConvert.SerializeObject(instrument, Formatting.Indented));
                    Console.WriteLine("================================ ");
                    File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "_" + isin.Key + $"_{isin.Value}.json"), JsonConvert.SerializeObject(instrument, Formatting.Indented));
                }
                return;
                //var smcpInstrument = new InstrumentService().GetInstrumentById("FR0013214145"); // SMCP
                //Console.WriteLine(JsonConvert.SerializeObject(smcpInstrument, Formatting.Indented));
                //Console.WriteLine("================================ ");

                //var orderReq = new OrderRequest
                //{
                //    AccountKey = accounts[0].AccountKey,
                //    AssetType = smcpInstrument.AssetType,
                //    Amount = 1,
                //    BuySell = "Buy",
                //    Uic = smcpInstrument.Uic,
                //    ManualOrder = true,
                //    OrderDuration = new OrderDuration { DurationType = OrderDurationType.DayOrder.ToString() },
                //    OrderType = OrderType.Market.ToString()
                //};
                //var id = new OrderService().PostOrder(orderReq);
                //Console.WriteLine($"{id}");
                //Console.WriteLine("================================ ");

                //var order = new OrderService().GetOrder(id, accounts[0].ClientKey);
                //Console.WriteLine(JsonConvert.SerializeObject(order, Formatting.Indented));
                //Console.WriteLine("================================ ");

                //var orders = new OrderService().GetOrders(accounts[0].ClientKey, accounts[0].AccountKey);
                //Console.WriteLine(JsonConvert.SerializeObject(orders, Formatting.Indented));
                //Console.WriteLine("================================ ");

                //foreach (var account in accounts)
                //{
                //    var positions = new AccountService().GetPositions(account);
                //    Console.WriteLine(JsonConvert.SerializeObject(positions, Formatting.Indented));
                //    Console.WriteLine("================================ ");
                //}

                //instruments = new InstrumentService().GetInstruments("ACCOR");
                //Console.WriteLine(JsonConvert.SerializeObject(instruments, Formatting.Indented));
                //Console.WriteLine("================================ ");

                // Refhresh token and call api
                //Console.ForegroundColor = ConsoleColor.Green;
                //Console.WriteLine("Refreshing Token... ");
                //var newToken = LoginHelpers.RefreshToken(app, token.RefreshToken, listener);
                //client = new ClientService().GetClient(app.OpenApiBaseUrl, token.AccessToken, token.TokenType);
                //Console.WriteLine("New Token: ");
                //Console.WriteLine(JsonConvert.SerializeObject(new { Token = newToken, Client = client }, Formatting.Indented));
                Console.WriteLine("Demo Done.");
                Console.WriteLine("================================ ");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
