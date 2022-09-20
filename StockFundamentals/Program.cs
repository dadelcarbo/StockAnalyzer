using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StockFundamentals
{
    class Program
    {

        const string PARAMS = "{\"filter\":[{\"left\":\"price_book_ratio\",\"operation\":\"nempty\"},{\"left\":\"type\",\"operation\":\"equal\",\"right\":\"stock\"},{ \"left\":\"subtype\",\"operation\":\"equal\",\"right\":\"common\"},{ \"left\":\"SMA50\",\"operation\":\"less\",\"right\":\"close\"}],\"options\":{ \"active_symbols_only\":true,\"lang\":\"en\"},\"symbols\":{ \"query\":{ \"types\":[]},\"tickers\":[]},\"columns\":[\"logoid\",\"name\",\"close\",\"market_cap_basic\",\"price_earnings_ttm\",\"earnings_per_share_basic_ttm\",\"sector\",\"price_book_ratio\",\"dividend_yield_recent\",\"enterprise_value_fq\",\"enterprise_value_ebitda_ttm\",\"gross_margin\",\"description\",\"name\",\"type\",\"subtype\",\"update_mode\",\"pricescale\",\"minmov\",\"fractional\",\"minmove2\"],\"sort\":{ \"sortBy\":\"price_book_ratio\",\"sortOrder\":\"asc\"},\"range\":[0,150]}";

        //     logoid	name	close	market_cap_basic	price_earnings_ttm	earnings_per_share_basic_ttm	sector	price_book_ratio	dividend_yield_recent	enterprise_value_fq	enterprise_value_ebitda_ttm	gross_margin	description	name	type	subtype	update_mode	pricescale	minmov	fractional	minmove2

        [STAThread]
        static async Task Main(string[] args)
        {
            var httpClient = await TestCitiFirstDownload1Async();
            await TestCitiFirstDownload2Async(httpClient);

            return;

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://www.boursorama.com");

            var response = client.GetAsync("bourse/action/graph/ws/GetTicksEOD?symbol=1rPCA&length=5&period=-1&guid=").GetAwaiter().GetResult();
            var data = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            client = new HttpClient();
            client.BaseAddress = new Uri("https://scanner.tradingview.com");

            var content = new StringContent(PARAMS);

            var result = client.PostAsync("france/scan", content).GetAwaiter().GetResult();
            if (result.IsSuccessStatusCode)
            {
                var resultString = result.Content.ReadAsStringAsync().Result;
                Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(resultString);
                string text = string.Empty;
                foreach (var item in myDeserializedClass.data)
                {
                    var line = item.d.Select(d => d?.ToString()).Aggregate((i, j) => i + "\t" + j);
                    text += line + Environment.NewLine;
                }

                Clipboard.SetText(text);
            }
        }

        private static async Task<HttpClient> TestCitiFirstDownload1Async()
        {
            var handler = new HttpClientHandler();
            handler.UseCookies = false;

            // If you are using .NET Core 3.0+ you can replace `~DecompressionMethods.None` to `DecompressionMethods.All`
            handler.AutomaticDecompression = ~DecompressionMethods.None;

            // In production code, don't destroy the HttpClient through using, but better use IHttpClientFactory factory or at least reuse an existing HttpClient instance
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests
            // https://www.aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
            var httpClient = new HttpClient(handler);
            using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://fr.citifirst.com/FR/Produits/turbos_infinis/Ice_Brent_Crude_Future_Nov2022/DE000KF8UYP9/"))
            {
                request.Headers.TryAddWithoutValidation("Accept", "*/*");
                request.Headers.TryAddWithoutValidation("Accept-Language", "en-GB,en;q=0.9,fr;q=0.8");
                request.Headers.TryAddWithoutValidation("Cache-Control", "no-cache");
                request.Headers.TryAddWithoutValidation("Connection", "keep-alive");
                request.Headers.TryAddWithoutValidation("Origin", "https://fr.citifirst.com");
                request.Headers.TryAddWithoutValidation("Referer", "https://fr.citifirst.com/FR/Produits/turbos_infinis/Ice_Brent_Crude_Future_Nov2022/DE000KF8UYP9/");
                request.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
                request.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
                request.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");
                request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36");
                request.Headers.TryAddWithoutValidation("X-MicrosoftAjax", "Delta=true");
                request.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                request.Headers.TryAddWithoutValidation("sec-ch-ua", "^^");
                request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "^^");
                request.Headers.TryAddWithoutValidation("Cookie", "_ga=GA1.2.212861413.1632425622; DisclaimerAccepted=True; NewsletterPopupMinimizedByUser=true; noMoreCookieWarning=true; RememberMe=PZDpLTsFzA7l1aiUvKI1OP8lPoTX45DCszM6nQpXP0ipHSyvuswwg8vTbF03o6U+BaQtvEvq8MT+TWilnNmuO1+LSNuEWP37YSbsVl7Fuxgd/F+4+hqWdleiAn/Bi01m; AcceptedCookieTypes=TechnicalCookies, ComfortCookies, ThirdPartyMedia; AcceptedCookieTypesV2=TechnicalCookies, WebTrackingCookies, ComfortCookies, ThirdPartyMedia; _gid=GA1.2.166914993.1663702115; .ASPXAUTH=7D8A6582D62D65C83BA3D0ADF3C285F78E93736117842F72E880F0EB321010B6C8A26B9FD391C1ACC46DC8E7A6E315C0FD7419DB38A9B43C3B381BAE980C62E50AA713D40DCE428A65211E1E295D3EA62F22D5005D3DD705124298B79405A37121FA4DB16EE9C065BBAA74BE30F1481880B5DF75B5019955F942159417EA038C8E974C558125C5C3FCAEFD5547D2C79409990DB7; ASP.NET_SessionId=ylfhtosbjups21y5oczx33b3; __AntiXsrfToken=7b1edf03836c4bf293edb3843415d266; _gat=1; LS4_pushpricescitifirstcom=^|505^|; LS4_https^%^3A^%^2F^%^2Fpushprices.citifirst.com=^|505_pushpricescitifirstcom^|; LS4_505_pushpricescitifirstcom=1663702810965^|S^|citifirst_com_505_pushpricescitifirstcom");

                request.Content = new StringContent("ctl00^%^24ctl00^%^24ctl04=ctl00^%^24ctl00^%^24ctl40^%^24ctl21^%^7Cctl00^%^24ctl00^%^24ctl40^%^24ctl23^%^24ctl03&__EVENTTARGET=ctl00^%^24ctl00^%^24ctl40^%^24ctl23^%^24ctl03&__EVENTARGUMENT=1&__VIEWSTATE=nCZFmD7VGL8Y1SrBgJ6zJOV9sNyUetfI9rrRspQUnvDzYaj00IYddTYkbbLNJrOmc9j4qPZYHScXcApekzUAgFjRCDuJl1G7y5^%^2BuTfgfkzbAqAm0UpwD2BSphtbi79nfeeuw7V3fl6av^%^2Bz3E6WRFejeK74FTi^%^2FgiMqe8hwBnM0llasETHXkHp1SxNVVDuSSeQyScJ0ydNiwyKMDbElJrkO2vSGiu7Jva^%^2FDcxb4SYIOkVzYyk7GBzda^%^2BhRoMAt8Ie^%^2BoekiftceXvEq8sX7OV27se99qutddCaFyq0GISBX0yEnbl9JjAZ3LpYM^%^2F6^%^2B29IluTH3ZyMDpLQAWnJdfaBGs9Ja^%^2F^%^2F4pPM1LAv3qpHxcfs0JPeBhToJQrj45d8bbscMYQHuOWQ8Mven28NiBU0nQ^%^2Bvr19eTOxaQ^%^2FltRPMViHyh9J7KefowASA63XZdfoLKpwOSW7MHYIoZR9ieUyGyNPQZ2a71xSqc7^%^2F4XVWcqcpDezXikEbh6jfQu0iUP2xLLvwOXRu09afbfYA6PVdohlW6IGfOwM4nLl4EZxZb2qnFrq0ebQ^%^2BLhv2q^%^2BF29qs2bRjsD2V0j4l8tdFf1PbVrQbaKPkHYaYKxVjgQZqXcvzpmWZWEjSn5QV022pA^%^2BFKrxK97aSUdmo^%^2BB05ISOG6rGwzuAGHpvFfrL^%^2BFikXHlhHIuIXb0TyUsLX1t6Z3iX0ml4sYuB6Yi^%^2FSWze8^%^2FLpvHqiWGvS^%^2BVfgrbym1exOFseguZR^%^2BrLTmuFQkJ9RhXU0cMmapZhBmYtDABPEFHgfF73WYFAiGPQWLBlsgRIcpE^%^2BJ9qJjnteip^%^2BYU^%^2B^%^2FDYKW^%^2BzZ3^%^2F7w5TUrTIAtd1badaMjewxvPS3sTwMdUEfZErbiazLpN^%^2FP6WpKObkCtxb9CDS5CStIik0id5rUb7LnhOZgzY4kdFbc8pX2x6WmeMvnl01EJvRsypVXB8QMa7t5oL4MLGRAuB1iriwammYcWCnGMJwRTxz2YOuThoe1YR3qx8RTXDc6EBs73^%^2FQ^%^2FDp9B0MkjQ2uo8yEeh7^%^2B^%^2F47eAfcSKdD2HCiNX0xMhR4hci2cDszNdEoFfIDSOcsUljNyy^%^2BGGo9OMXaEMUevggqFqehEobfUMrmrLAfitkFSw8tHh^%^2BU9mOZE81auuDCNAWWgXy1^%^2Ft^%^2FQ0ZU9PLDde0S2jOgS2bKhdTEd0IdARavOnQh9aT8BSGTeDvqoqSGYnVU5Ip6IkUvVHzFgm^%^2BTvhwUpHZTMcb2fY9^%^2FA4oGpr4cbM^%^2FPzgfVRavnsrkCmxScZwkKVb9laZU322JYoiIGfM2pPwNIYo1dUigz8XS1W2tvuy9YyrmzZeumwUnPAzmy90^%^2Frel3EHLQr0Mkyd63ybCo4AjIeI3oL6oZFmBWKEpB69LNE5wrIojoEopSAvXG^%^2B^%^2FRzRbHLJOEZAnnusEJUv1VhZflG4DhQ9V7B5gAwFZ^%^2FiXwHxTgJxAEKn4jCOsBw5bMqUxFTKy6LY^%^2FCuzwJ5Dd0Ebf6ahtXA^%^2FCgv0iFoTy7SNyFrddGLAK5Pb1^%^2B0D26m7ik26CKni3nAniSj^%^2F52eYUiYhVj6nJPZ7P0O9fjR2Rw5^%^2BoYSyM4CNtVHqC8bICbGDd^%^2F85MaBWZXmKHL7h0fMsVhf2YnmR^%^2FTM45ZOpSSEgmKNshTVBxp9pBeA0EOm77IfmuGgtPO48yR4kzZjwmzdQrledBp8F15aMX6UK1^%^2BZ6f7SARArCeusVJHg461q8390RAj9A7rg^%^2FmXYfFT8vZlbzroPhYbAwiuRgnQZuc0cr3RU4J9^%^2BD0Z^%^2FBHazIy^%^2BgP9KkS79uk3QGLDVp3phvCp0MW14XWNN^%^2FeBiGppOORsxiOX4p2kOuBJJc8p8wcLAK^%^2FC3emvA8lPJoodPzS3FLnqRY^%^2F9K^%^2BqtN2V9YCnVE1^%^2B^%^2FfYJQygkP52avZINBwi05iZ8PJk56CpBQKJd07C6jqs0^%^2FyhAfm^%^2FKvxbeEiOlifTdFBP5fUBe2f6^%^2FpGV2wh7eO7v7OkM3WzFueLGY6AwnnIt1V2kFutL0Zo7GA9U5bxKyLy1jos^%^2FlqqphwiuWAGylJbGlDpw^%^2Bgx^%^2FvWYUe12RXKhis^%^2F4VPVHgPcU^%^2B7NbfTJf^%^2B6ezdA^%^2Bnwf^%^2FUnRfWhgeGxz5LzrmFW^%^2FhyDryVh1EGbDSL88mVXyty2EMl42X0mXbFl4Xt15c7k20oHXSHxcWqCtJ9k^%^2BJ097dqdNchdbD8LEKKOD7GhHM^%^2F4VmNCrg3ix7hizE0Bsm^%^2Forxrsb^%^2FLqxlnn5KAjfu9fSEFKPuoBQixXuyJtCuR7^%^2ByiSefRhNHlqjrEGZ1QXQ0DTO590gIwI1O85uvl58EGQcoBlChOljQYUqPivzAWqvXj2F9kPH55u1Xb0NGqguslg7bdDU^%^2Fe5KxGN5iqXCobrV^%^2FPWif35yZbrOMStUv65onnnpY3WphqXUQUDLBDUf3C09uA1c6eL0RGS6ESxUvbTxsOsUPbKuf4zi8RlSrlbUKW0HzcptitIrMzoM0CvjW8vnQZp^%^2Bb06oVdF2ZepRk3WWW6QSXafBSVcweEvc4XxGW2MX8xRDbRMQmhspn66qX9M5Bs91hBwL9lelS42x^%^2BPZ9UNehIOABys29yzre6iWBtH^%^2BFnAXYszs4Dwno9HfpZyiJ^%^2BIOCHw4wjGuWSZrt9JlvDex5uAqN7ZtB2L^%^2FJt^%^2BhlUHSaAyq8yNPJX^%^2FNoS9zA0BKLgHTOhSizXVOqpNVk6MJt^%^2FEPtLiOl^%^2F^%^2BDpZglzs4mjd3a^%^2BiNTxbCKyPI8^%^2Bvkb3tBqhKRZLoLcYozNsFWUo7DP8K7STT7vso2v8o6cL8CHruThJOvVXNu5mRgPN6LsFxqeDcEM4S3MWSQqCNpvOEa^%^2FxinWq5^%^2FPSTDRo4O7^%^2FDDN9Xv3uM^%^2FLbZCJVwKQFQ2GzKLEK2pCdZEehf^%^2FON^%^2BX64mbYWOMyq0hOZyJPdjKenERq8rs36LvBfKosAYvkI5eON4uI8YLmVsxOmr1rsXuqRrRP^%^2BUdY7aEREnel6mMxpxqopxmMQzsDpNlh2IjBXYzz1fQCYF^%^2B3ZwOdULQt6QH^%^2BTt8q5I4xz3XXkLZSWy9Z2sjhzXC9OyhGzopU4qJl7uYWs600Xbi0T^%^2B9qhI1bzLCKvdSFI^%^2Fb4VeGJPtXouIFYaGi1bpBbAKAr^%^2FoBJLhg8pXlcD5mkqi7Wn0Of2TNo^%^2Fkr7c2TzI96idryjYcQLP8LrkX2HPDeuuZRd52rdFT742TtwvglWtm3SSsbwWs^%^2FASAyPDgF78XQZs8UhRUl8s3TOYPcSD9^%^2BXrHfyBnmu4QrjHaKapqlV2ZlOY^%^2F21YZLVwHHld68IRZQXbsj0rAw1J8BRI2Jt2iuA9XgL^%^2Fp6dMhQrn281lUT8s2JjPobdEqiPGu6bHbZ2Y^%^2FS7aPyJUd1S1WSYguiXPon^%^2FO250cK^%^2FfsQIAQN^%^2B4GvWah4XTMvEscKZuH^%^2BnIjOHYcKN^%^2FLc17HYR1b6Z0Kwr6LZFmA6OhRJVxjkVeU5^%^2B^%^2Br77vib5g1B2cP8t4bMyQv^%^2FJySr2^%^2FoTLbxm8PsTcckFUXQ09^%^2FoppvBsN5cwEXAcpgrGdOCzV8MNVQjmNyTKAibeP26PLLs^%^2FiIDb1TAiIJ2J7zRGLcYXxb8GvfDRvHocVYHYp4r7^%^2B6LdEe4dLWlacZw37xXwNV2TmQEhbhmED^%^2FmFH5BgreKYHKyZKjxhBxQYryVxQu7zkjEFR^%^2FFvliqAVwKmTvIG5RCz4RN20icnYac6oWzcqG2qWggQyJ^%^2BBneAlpxkaA4X99yE5SbMRMmDzfSWgXI7TLy1CZkMv4Ja8o8WrkgJJAh0SMPRo^%^2FQ0mhdusVioMCTsiZNqtAV8tCQ88TN3oaG98^%^2FJprfW^%^2FjRNht1KRQIxkKjxi6mFseN8EDt16vFBAP3SQQfkb0OKIRX2BccWWVA5B86UILr25raDjivrhGAKiSSR3hkkjgN0Fw9yB9SZLeKNaGnSI6LLe^%^2FX0rd6eZFOMvStl1SNe60FRNrxAylwOvFBCCHYxvMe14SDeNTcfUjFwYaAcg62dvF1TdylcUSna46Sq2hIcmzJJidAJC9zqSXg4CJUtl8oJ^%^2FoXou^%^2Fx^%^2B7F2g6XIP^%^2BnUuGNhoO0FxxlczDiqiGpMlhG8v2Jps3HlxCNATfvxBL5k^%^2BYLtO2PJlHcJUPj6kQn19o3cIBkH0wrAbYsOeW^%^2BtXqAxUN^%^2F23^%^2Bs20TS8tF1sCls882ebmjHjBIM7kfxazTWBvTA^%^2Fuqlkg8Av5TU8r3QRpLvJvef0SSyF^%^2BdIHtOO^%^2BW7HbdZ7vW50eLVSE7Zv^%^2BphHPAVxMefCv6h^%^2FdYwyR4HRiB18c1sWK1D9orchPPX9mbLELVh2CBQrkwqOmLR1^%^2BOw5HNwEqQypNO3YNriHTYXEkuSr5ngv^%^2Fv9hJVSDPU8fonw8krG7dlpfxlbXosmzUWQjPunVGYMlf4y5aHgBJ^%^2FsfdDd^%^2B2KfK7hF85kfM^%^2B8QVh78REk3fcPYgYQH32WzFcQvm^%^2Bq7dEGVAK1x456FCMh07C9Ss4XwIOJblHw6Yt9KMX^%^2BCMUZ5CO64^%^2BE^%^2B6gh1tgdUSDPjohG33xArAgPcmbQ29XZyDY^%^2BBsvd9oyF1tJLKGNC35KS64fAU2ypXgvjD97XbbUwOWchUVHWwozXhRNTO0BzY^%^2Ba2qBzjZJ0af39xMiNEyZXZZZ2a1bnPTZEuQK6Hlu^%^2Fi^%^2FNpSeITvTV7OX2N9qYUI^%^2FoKUPj^%^2FjcaeukgzzY0hgNrYtjZP8b^%^2Bp405vY72lRO5sIEcMzpFRh2Op94FfhXhLWngxo7zC3VXp5m3WIQ6qDxAwFTeHNeWCZah3xesGrK6S0t9NKrkojx89c0UTSXDqSY9wUjiuRExylHO3fQRmAa93MpIrqKOPPGwaKk320cOVHoS^%^2BpBogQMMeHem9trpAemPpbn7Jj1aX549BGpQ65^%^2BYgxVB6Ny0eGqpuMs5leYA3eviAZyLNZOocuueG6LlHlyLFldYAQObXBn7wrb8OHGOuZkD^%^2FmdfXpMimXQKKBYvMnQgFL8XR^%^2F73E06lWFLykdUkzzTKxfd2jVKbPJlP3V0TXV16bFH0x56uHrCRfGXO0xI9wn2IcLHJqd5AUCl^%^2BQ8f1^%^2Fgfht3Thd4XvmL3VNZvjdfiSMqfK8nutP1Cb5^%^2BDiq^%^2FvLiSC0RKV3i6eU891KWS7JmftNa8TwHV55j6bcBVyEfki7S^%^2Favwf2Ol8zyU0fXvY9HZDOno^%^2FSrd8Ekc83fKx1zAdTbXsxt99fTrXqyw2UykqUN0IP6QEZl6IENVtArcJ5CvFDsf^%^2FeDjV83tSp^%^2B3E0ooaIksQVIAt7nllKWeevDVHhLh89srVcrtnMMUpzlDDQH1^%^2FYSJL82x7iSO9U^%^2B0wIcXEJY9NmPPAaqXF0y5rHr6mL5643wu0QVzHpRHn0nqvJO1IZeeqRDVNt4sRA0pDrGoaXK^%^2Fgt7sbeHR75eIbMLEzkKMFBxVvw^%^2FxiNPH1ormyWijeHgtcisf8Xsox7MbfjdRHqEUYUsVsUxmCct3pWFE^%^2FZzw46tqh^%^2FZY4P44MtxFsxuqrTANcjffwe05wbeWtY6EHrvL5OdtirUdUsunMI7gcZ^%^2BIlZEJRPP0bHiN3iWfRKM^%^2BQqNTS^%^2FbjTOTyxAXzErVsrCFghQ0HQ^%^2FEUHYwp1Kp4CHPJY086jnC7z1nDlDmnAy427YDYXCASJfTZFx4feWhBQOieYrwfWdh^%^2Bu5U3WCPYCKqWVrFULmeIWTmO^%^2BKRxioHS8Dswvw^%^2FjwRkTt^%^2FM3&__VIEWSTATEGENERATOR=D94E3D15&__VIEWSTATEENCRYPTED=&ctl00^%^24ctl00^%^24ctl09^%^24ctl00^%^24ctl02^%^24ctl00=FR&ctl00^%^24ctl00^%^24ctl09^%^24ctl00^%^24ctl03^%^24ctl00=e840af7f-69f4-4425-bb1d-fdded0a88f4c&ctl00^%^24ctl00^%^24ctl09^%^24ctl02^%^24ctl03^%^24ctl02^%^24ctl00=^%^7B^%^22Guid^%^22^%^3A^%^22ed81f7ad-9cc4-44e2-9b10-e193cf74e5d6^%^22^%^2C^%^22ProductTypes^%^22^%^3A^%^5B10^%^2C3^%^2C9^%^2C2^%^2C5^%^2C7^%^2C6^%^2C4^%^2C1^%^2C8^%^5D^%^7D&searchBox=&ctl00^%^24ctl00^%^24ctl40^%^24ctl23^%^24ctl06^%^24ctl00^%^24ctl00=20.09.2022&ctl00^%^24ctl00^%^24ctl40^%^24ctl23^%^24ctl06^%^24ctl02^%^24ctl00=20.09.2022&ctl00^%^24ctl00^%^24ctl16^%^24ctl00^%^24ctl13=&__ASYNCPOST=true&");
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded; charset=UTF-8");

                var response = await httpClient.SendAsync(request);

                var content = response.Content.ReadAsStringAsync().Result;
                //var cookie = response.Headers.GetValues("Set-Cookie").ToString();
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Success: " + content);
                }
                else
                {
                    Console.WriteLine($"Failure: {response.StatusCode} " + content);
                }
            }
            return httpClient;
        }

        private static async Task TestCitiFirstDownload2Async(HttpClient httpClient)
        {
            var handler = new HttpClientHandler();
            handler.UseCookies = false;

            // If you are using .NET Core 3.0+ you can replace `~DecompressionMethods.None` to `DecompressionMethods.All`
            handler.AutomaticDecompression = ~DecompressionMethods.None;

            // In production code, don't destroy the HttpClient through using, but better use IHttpClientFactory factory or at least reuse an existing HttpClient instance
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests
            // https://www.aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
            using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://fr.citifirst.com/Data/Json/Chart?quadrupel=102^%25^2cDE000KF8UYP9^%25^2c0^%25^2c0&period=OneWeek&chartStyle=JsonAbsolute&groups=^%25^7b^%25^22Groups^%25^22^%25^3a^%25^5b^%25^5d^%25^7d&postProcess=RemoveZerosPostProcess^%25^3bSetSecondSeriesToOpositeAxis^%25^3bMergeNullDataPointsIntoFirstSeries^%25^3bConvertDatesToUtc^%25^3bRecalculateWithLocalTimezone&chartHash=DFDD1192B9525251844F4A53422D29B908F594A6F00AF2AD8A23B154621BB8F3"))
            {
                request.Headers.TryAddWithoutValidation("Accept", "application/json, text/javascript, */*; q=0.01");
                request.Headers.TryAddWithoutValidation("Accept-Language", "en-GB,en;q=0.9,fr;q=0.8");
                request.Headers.TryAddWithoutValidation("Connection", "keep-alive");
                request.Headers.TryAddWithoutValidation("Referer", "https://fr.citifirst.com/FR/Produits/turbos_infinis/Ice_Brent_Crude_Future_Nov2022/DE000KF8UYP9/");
                request.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
                request.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
                request.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");
                request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36");
                request.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                request.Headers.TryAddWithoutValidation("sec-ch-ua", "^^");
                request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "^^");
                request.Headers.TryAddWithoutValidation("Cookie", "_ga=GA1.2.212861413.1632425622; DisclaimerAccepted=True; NewsletterPopupMinimizedByUser=true; noMoreCookieWarning=true; RememberMe=PZDpLTsFzA7l1aiUvKI1OP8lPoTX45DCszM6nQpXP0ipHSyvuswwg8vTbF03o6U+BaQtvEvq8MT+TWilnNmuO1+LSNuEWP37YSbsVl7Fuxgd/F+4+hqWdleiAn/Bi01m; AcceptedCookieTypes=TechnicalCookies, ComfortCookies, ThirdPartyMedia; AcceptedCookieTypesV2=TechnicalCookies, WebTrackingCookies, ComfortCookies, ThirdPartyMedia; _gid=GA1.2.166914993.1663702115; .ASPXAUTH=7D8A6582D62D65C83BA3D0ADF3C285F78E93736117842F72E880F0EB321010B6C8A26B9FD391C1ACC46DC8E7A6E315C0FD7419DB38A9B43C3B381BAE980C62E50AA713D40DCE428A65211E1E295D3EA62F22D5005D3DD705124298B79405A37121FA4DB16EE9C065BBAA74BE30F1481880B5DF75B5019955F942159417EA038C8E974C558125C5C3FCAEFD5547D2C79409990DB7; ASP.NET_SessionId=ylfhtosbjups21y5oczx33b3; __AntiXsrfToken=7b1edf03836c4bf293edb3843415d266; _gat=1; LS4_pushpricescitifirstcom=^|505^|; LS4_https^%^3A^%^2F^%^2Fpushprices.citifirst.com=^|505_pushpricescitifirstcom^|; LS4_505_pushpricescitifirstcom=1663702811968^|S^|citifirst_com_505_pushpricescitifirstcom");

                var response = await httpClient.SendAsync(request);

                var content = response.Content.ReadAsStringAsync().Result;
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Success: " + content);
                }
                else
                {
                    Console.WriteLine($"Failure: {response.StatusCode} " + content);
                }
            }
        }
        public class Datum
        {
            public string s { get; set; }
            public List<object> d { get; set; }
        }

        public class Root
        {
            public List<Datum> data { get; set; }
            public int totalCount { get; set; }
        }
    }
}
