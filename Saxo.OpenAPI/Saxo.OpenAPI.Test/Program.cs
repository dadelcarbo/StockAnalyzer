using Newtonsoft.Json;
using Saxo.OpenAPI.Models;
using Saxo.OpenAPI.AuthenticationServices;
using System;
using System.IO;
using Saxo.OpenAPI.TradingServices;

namespace Saxo.OpenAPI.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string appPath;
                // https://www.developer.saxo/openapi/referencedocs
                appPath = Path.Combine(AppContext.BaseDirectory, "App_live.json");
                appPath = Path.Combine(AppContext.BaseDirectory, "App_sim.json");

                var token = LoginService.Login(appPath);

                var accounts = new AccountService().GetAccounts();
                Console.WriteLine(JsonConvert.SerializeObject(accounts, Formatting.Indented));
                Console.WriteLine("================================ ");

                foreach (var account in accounts)
                {
                    var positions = new AccountService().GetPositions(account);
                    Console.WriteLine(JsonConvert.SerializeObject(positions, Formatting.Indented));
                    Console.WriteLine("================================ ");
                }

                var instrument = new InstrumentService().GetInstrumentById(8090026); // SMCP
                Console.WriteLine(JsonConvert.SerializeObject(instrument, Formatting.Indented));
                Console.WriteLine("================================ ");

                //instrument = new InstrumentService().GetInstrumentById("NL0012969182"); // ADYEN
                //Console.WriteLine(JsonConvert.SerializeObject(instrument, Formatting.Indented));
                //Console.WriteLine("================================ ");

                var orderReq = new OrderRequest
                {
                    AccountKey = accounts[0].AccountKey,
                    AssetType = instrument.AssetType,
                    Amount = 100,
                    BuySell = "Buy",
                    Uic = instrument.Identifier,
                    ManualOrder = true,
                    OrderDuration = new OrderDuration { DurationType = OrderDurationType.DayOrder.ToString() },
                    OrderType = OrderType.Market.ToString()
                };
                var id = new OrderService().PostOrder(orderReq);
                Console.WriteLine($"{id}");
                Console.WriteLine("================================ ");

                var order = new OrderService().GetOrder(id, accounts[0].ClientKey);
                Console.WriteLine(JsonConvert.SerializeObject(order, Formatting.Indented));
                Console.WriteLine("================================ ");

                var orders = new OrderService().GetOrders(accounts[0].ClientKey, accounts[0].AccountKey);
                Console.WriteLine(JsonConvert.SerializeObject(orders, Formatting.Indented));
                Console.WriteLine("================================ ");

                foreach (var account in accounts)
                {
                    var positions = new AccountService().GetPositions(account);
                    Console.WriteLine(JsonConvert.SerializeObject(positions, Formatting.Indented));
                    Console.WriteLine("================================ ");
                }

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
            finally
            {
                Console.ReadKey();
            }
        }
    }
}
