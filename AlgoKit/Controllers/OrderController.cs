using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AlgoKit.Data.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using AlgoKit.Data.Core.Models;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using KiteConnect;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;
using System.Drawing;
using System.Globalization;

namespace AlgoKit.Controllers
{
    [Route("api/Core/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IConfiguration _configuration;
        static Ticker ticker;
        static Kite kite;
        static int RegularStoplossPercentage;
        static int ScalpingStoplossPercentage;

        User user;
        // Initialize key and secret of your app
        static string MyAPIKey;
        static string MySecret;
        static string MyUserID;

        // persist these data in settings or db or file
        static string MyPublicToken;
        static string MyAccessToken = "";
        static string RefreshToken = "";

        int retry = 0;
        static List<Order> orders_static;
        static List<Position> positions_static;

        static string RegularOrScalping = "REGULAR";

        static List<InstrumentPosition> InstrumentPositions;
        public OrderController(IOrderService orderService, IConfiguration configuration)
        {
            _orderService = orderService;
            _configuration = configuration;


            MyAPIKey = configuration.GetSection("APIKey").Value;
            MySecret = configuration.GetSection("Secret").Value;
            MyUserID = configuration.GetSection("UserID").Value;

            RegularStoplossPercentage = Convert.ToInt32(configuration.GetSection("RegularStoplossPercentage").Value);
            ScalpingStoplossPercentage = Convert.ToInt32(configuration.GetSection("ScalpingStoplossPercentage").Value);

            MyAccessToken = _orderService.GetAccessToken(MyUserID);

            kite = new Kite(MyAPIKey, Debug: true);

            // For handling 403 errors

            kite.SetSessionExpiryHook(OnTokenExpire);

            // Initializes the login flow

            try
            {
                if (string.IsNullOrEmpty(MyAccessToken))
                {
                    initSession();
                }

            }
            catch (Exception e)
            {
                // Cannot continue without proper authentication
                Console.WriteLine(e.Message);
                Console.ReadKey();
                Environment.Exit(0);
                _orderService.LogException(e.Message);
            }

            kite.SetAccessToken(MyAccessToken);

        }

        [HttpGet("initiateSession/{request_token}")]
        public IActionResult initiateSession(string request_token)
        {

            User user = kite.GenerateSession(request_token, MySecret);

            MyAccessToken = user.AccessToken;
            MyPublicToken = user.PublicToken;
            RefreshToken = user.RefreshToken;
            _orderService.UpdateKiteAPITokens(MyUserID, user.AccessToken, user.PublicToken);
            return Ok();
        }

        private void initSession()
        {
            try
            {
                string requestToken = _configuration.GetSection("requestToken").Value;
                User user = kite.GenerateSession(requestToken, MySecret);

                MyAccessToken = user.AccessToken;
                MyPublicToken = user.PublicToken;
                RefreshToken = user.RefreshToken;
                _orderService.UpdateKiteAPITokens(MyUserID, user.AccessToken, user.PublicToken);
            }
            catch (Exception ex)
            {
                _orderService.LogException(ex.Message);
            }
        }

        /// <summary>
        ///Add Order
        /// </summary>
        /// <param name=""></param>
        [HttpPost("PlaceOrder")]
        [RequestSizeLimit(100_000_000)]
        public IActionResult PlaceOrder(Signal signal)
        {
            kite.SetAccessToken(MyAccessToken);


            _orderService.InsertSuperTrendSignal(signal.BuyOrSell, signal.Period);

            string BuyOrSell = _orderService.GetTradeSignal();

            if (BuyOrSell.ToUpper() == "BUY")
            {
                RegularOrScalping = signal.RegularOrScalping;
                BuyOrder_CE(signal.ScriptName);
            }

            else if (BuyOrSell.ToUpper() == "EXIT_CE")
            {

                ExitBankNiftyPositions(BuyOrSell, signal.ScriptName);
            }
            else if (BuyOrSell.ToUpper() == "SELL")
            {
                RegularOrScalping = signal.RegularOrScalping;
                BuyOrder_PE(signal.ScriptName);
            }
            else if (BuyOrSell.ToUpper() == "EXIT_PE")
            {

                ExitBankNiftyPositions(BuyOrSell, signal.ScriptName);
            }

            return Ok();
        }


        [HttpGet("BuyCE")]
        public void BuyOrder_CE(string ScriptName)
        {
            try
            {
                UserMargin equityMargins = kite.GetMargins(Constants.MARGIN_EQUITY);
                int availableCashToTrade = (int)equityMargins.Net;

                string TradingSymbolComputed = "";
                int lots, quantity;

                string year, month, day, TodaysDay;

                Dictionary<string, LTP> LPTData;
                decimal lastTradedPrice = 0;

                PositionResponse positionResponse;
                List<Position> Netpositions;


                if (ScriptName.ToUpper() == "BANKNIFTY")
                {
                    Dictionary<string, LTP> ltps = kite.GetLTP(InstrumentId: new string[] { "NSE:NIFTY BANK" });
                    double BankNiftyValue = Math.Round(Convert.ToInt32(ltps["NSE:NIFTY BANK"].LastPrice) / 100.0) * 100;

                    DateTime nextWednessDay = FindNextWednesday(DateTime.UtcNow);


                    year = nextWednessDay.Year.ToString().Substring(2);
                    month = nextWednessDay.Month.ToString();
                    day = nextWednessDay.Day.ToString();
                    TodaysDay = DateTime.Now.Day.ToString();

                    if (TodaysDay == day)//check if it expiry day
                    {
                        nextWednessDay = FindNextWednesday(DateTime.UtcNow.AddDays(1));

                        year = nextWednessDay.Year.ToString().Substring(2);
                        month = nextWednessDay.Month.ToString();
                        day = nextWednessDay.Day.ToString();

                    }

                    if (IsLastWednesdayOfMonth(new DateTime(nextWednessDay.Year, nextWednessDay.Month, nextWednessDay.Day)))//check if it is last week of the month
                    {
                        TradingSymbolComputed = "BANKNIFTY" + year + DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(nextWednessDay.Month).ToUpper() + BankNiftyValue + "CE";
                    }
                    else
                    {
                        TradingSymbolComputed = "BANKNIFTY" + year + month + day + BankNiftyValue + "CE";
                    }


                    positionResponse = kite.GetPositions();

                    Netpositions = positionResponse.Net.FindAll(position => position.Quantity > 0);

                    positions_static = Netpositions;


                    if (Netpositions.FindAll(x => x.TradingSymbol == TradingSymbolComputed && x.Quantity > 0).Count == 0)
                    {

                        LPTData = kite.GetLTP(InstrumentId: new string[] { "NFO:" + TradingSymbolComputed });
                        lastTradedPrice = LPTData["NFO:" + TradingSymbolComputed].LastPrice;

                        lots = (int)((availableCashToTrade) / (15 * lastTradedPrice));

                        quantity = lots * 15;

                        if (quantity > 900)
                        {
                            quantity = 900;
                        }

                        if (quantity >= 15)
                        {
                            kite.PlaceOrder(
                                       Exchange: Constants.EXCHANGE_NFO,
                                       TradingSymbol: TradingSymbolComputed,
                                       TransactionType: Constants.TRANSACTION_TYPE_BUY,
                                       Quantity: 15,
                                       OrderType: Constants.ORDER_TYPE_MARKET,
                                       Variety: Constants.VARIETY_REGULAR,
                                       Product: Constants.PRODUCT_MIS
                                   );
                        }


                        PlaceStoplossOrder(TradingSymbolComputed);

                        List<Order> orders = kite.GetOrders();
                        orders_static = orders;
                        positionResponse = kite.GetPositions();

                        Netpositions = positionResponse.Net.FindAll(position => position.Quantity > 0);

                        positions_static = Netpositions;

                        initTicker();
                    }
                }
            }
            catch (Exception ex)
            {

                _orderService.LogException(ex.Message);
            }
        }


        [HttpGet("BuyPE")]
        public void BuyOrder_PE(string ScriptName)
        {
            try
            {
                UserMargin equityMargins = kite.GetMargins(Constants.MARGIN_EQUITY);
                int availableCashToTrade = (int)equityMargins.Net;

                string TradingSymbolComputed = "";
                int lots, quantity;

                string year, month, day, TodaysDay;

                Dictionary<string, LTP> LPTData;
                decimal lastTradedPrice = 0;

                PositionResponse positionResponse;
                List<Position> Netpositions;


                if (ScriptName.ToUpper() == "BANKNIFTY")
                {
                    Dictionary<string, LTP> ltps = kite.GetLTP(InstrumentId: new string[] { "NSE:NIFTY BANK" });
                    double BankNiftyValue = Math.Round(Convert.ToInt32(ltps["NSE:NIFTY BANK"].LastPrice) / 100.0) * 100;

                    DateTime nextWednessDay = FindNextWednesday(DateTime.UtcNow);


                    year = nextWednessDay.Year.ToString().Substring(2);
                    month = nextWednessDay.Month.ToString();
                    day = nextWednessDay.Day.ToString();
                    TodaysDay = DateTime.Now.Day.ToString();

                    if (TodaysDay == day)//check if it expiry day
                    {
                        nextWednessDay = FindNextWednesday(DateTime.UtcNow.AddDays(1));

                        year = nextWednessDay.Year.ToString().Substring(2);
                        month = nextWednessDay.Month.ToString();
                        day = nextWednessDay.Day.ToString();

                    }

                    if (IsLastWednesdayOfMonth(new DateTime(nextWednessDay.Year, nextWednessDay.Month, nextWednessDay.Day)))//check if it is last week of the month
                    {
                        TradingSymbolComputed = "BANKNIFTY" + year + DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(nextWednessDay.Month).ToUpper() + BankNiftyValue + "PE";
                    }
                    else
                    {
                        TradingSymbolComputed = "BANKNIFTY" + year + month + day + BankNiftyValue + "PE";
                    }


                    positionResponse = kite.GetPositions();

                    Netpositions = positionResponse.Net.FindAll(position => position.Quantity > 0);

                    positions_static = Netpositions;


                    if (Netpositions.FindAll(x => x.TradingSymbol == TradingSymbolComputed && x.Quantity > 0).Count == 0)
                    {

                        LPTData = kite.GetLTP(InstrumentId: new string[] { "NFO:" + TradingSymbolComputed });
                        lastTradedPrice = LPTData["NFO:" + TradingSymbolComputed].LastPrice;

                        lots = (int)((availableCashToTrade) / (15 * lastTradedPrice));

                        quantity = lots * 15;

                        if (quantity > 900)
                        {
                            quantity = 900;
                        }

                        if (quantity >= 15)
                        {
                            kite.PlaceOrder(
                                       Exchange: Constants.EXCHANGE_NFO,
                                       TradingSymbol: TradingSymbolComputed,
                                       TransactionType: Constants.TRANSACTION_TYPE_BUY,
                                       Quantity: 15,
                                       OrderType: Constants.ORDER_TYPE_MARKET,
                                       Variety: Constants.VARIETY_REGULAR,
                                       Product: Constants.PRODUCT_MIS
                                   );
                        }


                        PlaceStoplossOrder(TradingSymbolComputed);

                        List<Order> orders = kite.GetOrders();
                        orders_static = orders;
                        positionResponse = kite.GetPositions();

                        Netpositions = positionResponse.Net.FindAll(position => position.Quantity > 0);

                        positions_static = Netpositions;

                        initTicker();
                    }
                }
            }
            catch (Exception ex)
            {

                _orderService.LogException(ex.Message);
            }

        }


        [HttpGet("initiateTicker")]
        public IActionResult initiateTicker()
        {
            initTicker();
            return Ok();
        }

        private void initTicker()
        {
            try
            {
                ticker = new Ticker(MyAPIKey, MyAccessToken);

                ticker.OnTick += OnTick;
                ticker.OnReconnect += OnReconnect;
                ticker.OnNoReconnect += OnNoReconnect;
                ticker.OnError += OnError;
                ticker.OnClose += OnClose;
                ticker.OnConnect += OnConnect;
                ticker.OnOrderUpdate += OnOrderUpdate;
                ticker.EnableReconnect(Interval: 5, Retries: 50);
                ticker.Connect();

                // Subscribing to websocket with instrument tokens and setting mode to LTP

                RegularStoplossPercentage = Convert.ToInt32(_configuration.GetSection("RegularStoplossPercentage").Value);
                ScalpingStoplossPercentage = Convert.ToInt32(_configuration.GetSection("ScalpingStoplossPercentage").Value);

                List<Order> orders = kite.GetOrders();
                orders_static = orders;
                PositionResponse positionResponse = kite.GetPositions();

                List<Position> Netpositions = positionResponse.Net.FindAll(position => position.Quantity > 0);

                positions_static = Netpositions;

                UInt32[] tokens = new UInt32[Netpositions.Count];

                int index = 0;
                foreach (Position position in Netpositions)
                {
                    tokens[index] = position.InstrumentToken;
                    index++;
                }

                ticker.Subscribe(tokens);
                ticker.SetMode(tokens, Mode: Constants.MODE_LTP);
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception occured");
                _orderService.LogException(ex.Message);
            }

        }

        private void OnTokenExpire()
        {
            Console.WriteLine("Need to login again");

        }

        private static void OnConnect()
        {
            Console.WriteLine("Connected ticker");
        }
        private static void OnClose()
        {
            Console.WriteLine("Closed ticker");
        }
        private static void OnError(string Message)
        {
            Console.WriteLine("Error: " + Message);
        }
        private static void OnNoReconnect()
        {
            Console.WriteLine("Not reconnecting");
        }
        private static void OnReconnect()
        {
            Console.WriteLine("Reconnecting");
        }

        private void OnTick(Tick TickData)
        {
            TrailingStoplossTick(TickData);
            //Console.WriteLine("Tick " + Utils.JsonSerialize(TickData));
        }

        private static void OnOrderUpdate(Order OrderData)
        {
            if (OrderData.Status.ToUpper() == Constants.ORDER_STATUS_COMPLETE && OrderData.TransactionType.ToUpper()=="SELL" )
            {

                PositionResponse positionResponse = kite.GetPositions();

                List<Position> Netpositions = positionResponse.Net.FindAll(position => position.Quantity > 0 && position.InstrumentToken==OrderData.InstrumentToken);

                positions_static = Netpositions;

                if (Netpositions.Count == 0)
                {
                    if (ticker != null)
                    {
                        ticker.UnSubscribe(Tokens: new UInt32[] { OrderData.InstrumentToken });
                    }
                }
            }
        }


        [HttpPost("PostBack")]
        public IActionResult PostBack(PostbackPayload Payload)
        {

            if (Payload.status.ToUpper() == Constants.ORDER_STATUS_COMPLETE && Payload.transaction_type.ToUpper() == "SELL")
            {

                PositionResponse positionResponse = kite.GetPositions();

                List<Position> Netpositions = positionResponse.Net.FindAll(position => position.Quantity > 0 && position.InstrumentToken == Payload.instrument_token);

                positions_static = Netpositions;

                if (Netpositions.Count == 0)
                {
                    if (ticker != null)
                    {
                        ticker.UnSubscribe(Tokens: new UInt32[] { Payload.instrument_token });
                    }
                }
            }

            _orderService.LogException("DATA RECEIVED ON POSTBACK and Order status - " + Payload.status + " " + Payload.instrument_token.ToString() + " " + Payload.tradingsymbol);

            return Ok();

        }


        private void TrailingStoplossTick(Tick TickData)
        {

            uint InstrumentToken = TickData.InstrumentToken;
            
            try
            {
                Order order = orders_static.Find(x => x.InstrumentToken == TickData.InstrumentToken && x.Status.ToUpper() == "TRIGGER PENDING");


                Position position = positions_static.Find(x => x.InstrumentToken == TickData.InstrumentToken && x.Quantity > 0);

                int TriggerPrice, price;

                List<Order> OrdersForInstrument = orders_static.FindAll(order => order.InstrumentToken == position.InstrumentToken && order.Product == position.Product && order.Status.ToUpper() == "TRIGGER PENDING");


                if (OrdersForInstrument.Count > 0)
                {
                    decimal change = ((TickData.LastPrice - order.TriggerPrice) / order.TriggerPrice) * 100;
                    change = (int)change;

                    if (order.InstrumentToken != 0 &&  (change - RegularStoplossPercentage) >= 2)
                    {

                        TriggerPrice = GetTriggerPrice(TickData.LastPrice, RegularOrScalping.ToUpper());
                        price = TriggerPrice - 1;

                        kite.ModifyOrder(
                                        Exchange: position.Exchange,
                                        OrderId: order.OrderId,
                                        TradingSymbol: position.TradingSymbol,
                                        TransactionType: Constants.TRANSACTION_TYPE_SELL,
                                        Quantity: position.Quantity.ToString(),
                                        Price: price,
                                        TriggerPrice: TriggerPrice,
                                        OrderType: Constants.ORDER_TYPE_SL,
                                        Variety: Constants.VARIETY_REGULAR,
                                        Product: order.Product
                                    );

                        order.TriggerPrice = TriggerPrice;

                        int index = orders_static.FindIndex(x => x.InstrumentToken == TickData.InstrumentToken && x.Status.ToUpper() == "TRIGGER PENDING");
                        orders_static[index] = order;

                        _orderService.LogException("Stoploss Modified at " + TickData.LastPrice.ToString()+ " for TradingSymbol - " + position.TradingSymbol);

                    }
                }
            }
            catch (Exception ex)
            {
                if (ticker != null) {
                    ticker.UnSubscribe(Tokens: new UInt32[] { InstrumentToken });
                }
                _orderService.LogException(ex.Message);
            }
        }

        private void PlaceStoplossOrder(string TradingSymbolComputed)
        {


            decimal LastTradedPrice, InstrumentToken;

            Dictionary<string, Quote> InstrumentDetails = kite.GetQuote(InstrumentId: new string[] { "NFO:" + TradingSymbolComputed });
            InstrumentToken = InstrumentDetails["NFO:" + TradingSymbolComputed].InstrumentToken;
            LastTradedPrice = InstrumentDetails["NFO:" + TradingSymbolComputed].LastPrice;



            int TriggerPrice = GetTriggerPrice(LastTradedPrice, RegularOrScalping);
            int price = TriggerPrice - 1;


            List<Order> orders = kite.GetOrders();

            PositionResponse positionResponse = kite.GetPositions();
            List<Position> Netpositions = positionResponse.Net.FindAll(position => position.Quantity > 0 && position.InstrumentToken == InstrumentToken);


            foreach (Position position in Netpositions)
            {
                List<Order> OrdersForInstrument = orders.FindAll(order => order.InstrumentToken == position.InstrumentToken && order.Product == position.Product && order.Status.ToUpper() == "TRIGGER PENDING");


                if (OrdersForInstrument.Count == 1)
                {

                    kite.ModifyOrder(
                                   OrderId: OrdersForInstrument[0].OrderId,
                                   Exchange: position.Exchange,
                                   TradingSymbol: TradingSymbolComputed,
                                   TransactionType: Constants.TRANSACTION_TYPE_SELL,
                                   Quantity: position.Quantity.ToString(),
                                   Price: price,
                                   TriggerPrice: TriggerPrice,
                                   OrderType: Constants.ORDER_TYPE_SL,
                                   Variety: Constants.VARIETY_REGULAR,
                                   Product: position.Product
                               );

                }
                else if (OrdersForInstrument.Count == 0)
                {
                    kite.PlaceOrder(
                                          Exchange: position.Exchange,
                                          TradingSymbol: TradingSymbolComputed,
                                          TransactionType: Constants.TRANSACTION_TYPE_SELL,
                                          Quantity: position.Quantity,
                                          Price: price,
                                          TriggerPrice: TriggerPrice,
                                          OrderType: Constants.ORDER_TYPE_SL,
                                          Variety: Constants.VARIETY_REGULAR,
                                          Product: Constants.PRODUCT_MIS
                                      );

                }
                else if (OrdersForInstrument.Count > 1)
                {
                    foreach (Order order in OrdersForInstrument)
                    {
                        kite.CancelOrder(order.OrderId);
                    }

                    kite.PlaceOrder(
                                          Exchange: position.Exchange,
                                          TradingSymbol: TradingSymbolComputed,
                                          TransactionType: Constants.TRANSACTION_TYPE_SELL,
                                          Quantity: position.Quantity,
                                          Price: price,
                                          TriggerPrice: TriggerPrice,
                                          OrderType: Constants.ORDER_TYPE_SL,
                                          Variety: Constants.VARIETY_REGULAR,
                                          Product: Constants.PRODUCT_MIS
                                      );
                }
            }


        }

        [HttpGet("ExitBankNiftyPositions/{ExitCode}/{ScriptName}")]
        public void ExitBankNiftyPositions(string ExitCode, string ScriptName)
        {
            if (ExitCode.ToUpper() == "EXIT_CE" && ScriptName.ToUpper() == "BANKNIFTY")
            {
                List<Order> orders = kite.GetOrders();
                orders_static = orders;
                PositionResponse positionResponse = kite.GetPositions();

                List<Position> Netpositions = positionResponse.Net.FindAll(position => position.Quantity > 0 && position.TradingSymbol.Contains("CE") && position.TradingSymbol.StartsWith("BANKNIFTY"));

                positions_static = Netpositions;

                foreach (Position position in Netpositions)
                {
                    List<Order> OrdersForInstrument = orders.FindAll(order => order.InstrumentToken == position.InstrumentToken && order.Product == position.Product && order.Status.ToUpper() == "TRIGGER PENDING");

                    if (OrdersForInstrument.Count == 1)
                    {

                        kite.ModifyOrder(
                                       OrderId: OrdersForInstrument[0].OrderId,
                                       Exchange:position.Exchange,
                                       TradingSymbol: position.TradingSymbol,
                                       TransactionType: Constants.TRANSACTION_TYPE_SELL,
                                       Quantity: position.Quantity.ToString(),
                                       OrderType: Constants.ORDER_TYPE_MARKET,
                                       Variety: Constants.VARIETY_REGULAR,
                                       Product: position.Product
                                   );
                        UnsubscribeTickerForCompletedPosition(position.InstrumentToken);
                    }

                    else if (OrdersForInstrument.Count == 0)
                    {
                        kite.PlaceOrder(
                               Exchange: position.Exchange,
                               TradingSymbol: position.TradingSymbol,
                               TransactionType: Constants.TRANSACTION_TYPE_SELL,
                               Quantity: position.Quantity,
                               OrderType: Constants.ORDER_TYPE_MARKET,
                               Variety: Constants.VARIETY_REGULAR,
                               Product: position.Product
                           );

                        UnsubscribeTickerForCompletedPosition(position.InstrumentToken);
                    }
                    else if (OrdersForInstrument.Count > 1)
                    {
                        foreach (Order order in OrdersForInstrument)
                        {
                            kite.CancelOrder(order.OrderId);
                        }

                        kite.PlaceOrder(
                               Exchange: position.Exchange,
                               TradingSymbol: position.TradingSymbol,
                               TransactionType: Constants.TRANSACTION_TYPE_SELL,
                               Quantity: position.Quantity,
                               OrderType: Constants.ORDER_TYPE_MARKET,
                               Variety: Constants.VARIETY_REGULAR,
                               Product: position.Product
                           );
                        UnsubscribeTickerForCompletedPosition(position.InstrumentToken);
                    }
                }
            }
            else if (ExitCode.ToUpper() == "EXIT_PE" && ScriptName.ToUpper() == "BANKNIFTY")
            {
                List<Order> orders = kite.GetOrders();
                orders_static = orders;
                PositionResponse positionResponse = kite.GetPositions();

                List<Position> Netpositions = positionResponse.Net.FindAll(position => position.Quantity > 0 && position.TradingSymbol.Contains("PE") && position.TradingSymbol.StartsWith("BANKNIFTY"));

                positions_static = Netpositions;

                foreach (Position position in Netpositions)
                {
                    List<Order> OrdersForInstrument = orders.FindAll(order => order.InstrumentToken == position.InstrumentToken && order.Product == position.Product && order.Status.ToUpper() == "TRIGGER PENDING");

                    if (OrdersForInstrument.Count == 1)
                    {

                        kite.ModifyOrder(
                                       OrderId: OrdersForInstrument[0].OrderId,
                                       Exchange: position.Exchange,
                                       TradingSymbol: position.TradingSymbol,
                                       TransactionType: Constants.TRANSACTION_TYPE_SELL,
                                       Quantity: position.Quantity.ToString(),
                                       OrderType: Constants.ORDER_TYPE_MARKET,
                                       Variety: Constants.VARIETY_REGULAR,
                                       Product: position.Product
                                   );
                        UnsubscribeTickerForCompletedPosition(position.InstrumentToken);
                    }
                    else if (OrdersForInstrument.Count == 0)
                    {
                        kite.PlaceOrder(
                               Exchange: position.Exchange,
                               TradingSymbol: position.TradingSymbol,
                               TransactionType: Constants.TRANSACTION_TYPE_SELL,
                               Quantity: position.Quantity,
                               OrderType: Constants.ORDER_TYPE_MARKET,
                               Variety: Constants.VARIETY_REGULAR,
                               Product: position.Product
                           );
                        UnsubscribeTickerForCompletedPosition(position.InstrumentToken);
                    }
                    else if (OrdersForInstrument.Count > 1)
                    {
                        foreach (Order order in OrdersForInstrument)
                        {
                            kite.CancelOrder(order.OrderId);
                        }

                        kite.PlaceOrder(
                               Exchange: position.Exchange,
                               TradingSymbol: position.TradingSymbol,
                               TransactionType: Constants.TRANSACTION_TYPE_SELL,
                               Quantity: position.Quantity,
                               OrderType: Constants.ORDER_TYPE_MARKET,
                               Variety: Constants.VARIETY_REGULAR,
                               Product: position.Product
                           );
                        UnsubscribeTickerForCompletedPosition(position.InstrumentToken);
                    }
                }
            }

        }


        [HttpGet("ExitAllPositionsManually")]
        public IActionResult ExitAllPositionsManually()
        {

                List<Order> orders = kite.GetOrders();
                PositionResponse positionResponse = kite.GetPositions();

                List<Position> Netpositions = positionResponse.Net.FindAll(position => position.Quantity > 0);

                foreach (Position position in Netpositions)
                {
                    List<Order> OrdersForInstrument = orders.FindAll(order => order.InstrumentToken == position.InstrumentToken && order.Product == position.Product && order.Status.ToUpper() == "TRIGGER PENDING");


                    if (OrdersForInstrument.Count > 0)
                    {

                        kite.ModifyOrder(
                                       Exchange:position.Exchange,
                                       OrderId: OrdersForInstrument[0].OrderId,
                                       TradingSymbol: position.TradingSymbol,
                                       TransactionType: Constants.TRANSACTION_TYPE_SELL,
                                       Quantity: position.Quantity.ToString(),
                                       OrderType: Constants.ORDER_TYPE_MARKET,
                                       Variety: Constants.VARIETY_REGULAR,
                                       Product: position.Product
                                   );

                    UnsubscribeTickerForCompletedPosition(position.InstrumentToken);
                }
                else if (OrdersForInstrument.Count == 0)
                    {
                        kite.PlaceOrder(
                               Exchange: position.Exchange,
                               TradingSymbol: position.TradingSymbol,
                               TransactionType: Constants.TRANSACTION_TYPE_SELL,
                               Quantity: position.Quantity,
                               OrderType: Constants.ORDER_TYPE_MARKET,
                               Variety: Constants.VARIETY_REGULAR,
                               Product: position.Product
                           );
                    UnsubscribeTickerForCompletedPosition(position.InstrumentToken);
                }
            }

            return Ok();
        }

        private static int GetTriggerPrice(decimal lastTradedPrice, string RegularOrScalping)
        {
            int TriggerPrice = 0;
            if (RegularOrScalping.ToUpper() == "REGULAR")
            {
                TriggerPrice = Convert.ToInt32(Math.Ceiling(lastTradedPrice * ((100.0m - RegularStoplossPercentage)/100.0m)));
            }
            else if (RegularOrScalping.ToUpper() == "SCALPING")
            {
                TriggerPrice = Convert.ToInt32(Math.Ceiling(lastTradedPrice * ((100.0m - ScalpingStoplossPercentage) / 100.0m)));
            }

            return TriggerPrice;

        }



        private void UnsubscribeTickerForCompletedPosition(uint InstrumentToken)
        {
            if (ticker != null)
            {
                ticker.UnSubscribe(Tokens: new UInt32[] { InstrumentToken });
            }
        }
        //private static int GetTriggerPriceLast3minLowPrice(decimal lastTradedPrice,int instrumentToken)
        //{
        //    int TriggerPrice = 0;

        //    InstrumentPosition instrumentPosition = InstrumentPositions.Find(x => x.InstrumentToken == instrumentToken);

        //    DateTime PresentCandleDatetime = DateTime.Now;
        //    DateTime PreviousCandleDatetime = RemoveSeconds(PresentCandleDatetime.AddMinutes(-3));

        //    List<Historical> historical = kite.GetHistoricalData(
        //       InstrumentToken: instrumentToken.ToString(),
        //       FromDate: PreviousCandleDatetime,
        //       ToDate: PresentCandleDatetime,
        //       //FromDate: PresentCandleDatetime,   // 2016-01-01 12:50:00 AM   FromDate: new DateTime(2016, 1, 1, 12, 50, 0)
        //       //ToDate: PreviousCandleDatetime,    // 2016-01-01 01:10:00 PM
        //       Interval: Constants.INTERVAL_3MINUTE,
        //       Continuous: false
        //    );

        //    decimal lowestPrice = historical[0].Low;

            
        //}


        private bool BuyDecision(string ScriptName, string BuyOrSell)
        {
            bool buyOrNot = false;
            UInt32 instrumentToken = 0;

            DateTime PresentCandleDatetime = DateTime.Now;
            DateTime PreviousCandleDatetime = RemoveSeconds(PresentCandleDatetime.AddMinutes(-2));
            decimal PresentCandleAvgPrice;

            if (ScriptName.ToUpper() == "BANKNIFTY")
            {
                Dictionary<string, Quote> InstrumentDetails = kite.GetQuote(InstrumentId: new string[] { "NSE:NIFTY BANK" });
                instrumentToken = InstrumentDetails["NSE:NIFTY BANK"].InstrumentToken;
            }
            else if (ScriptName.ToUpper() == "NIFTY")
            {
                Dictionary<string, Quote> InstrumentDetails = kite.GetQuote(InstrumentId: new string[] { "NSE:NIFTY 50" });
                instrumentToken = InstrumentDetails["NSE:NIFTY BANK"].InstrumentToken;
            }

            List<Historical> historical = kite.GetHistoricalData(
               InstrumentToken: instrumentToken.ToString(),
               FromDate: PreviousCandleDatetime,
               ToDate: PresentCandleDatetime,
               //FromDate: PresentCandleDatetime,   // 2016-01-01 12:50:00 AM   FromDate: new DateTime(2016, 1, 1, 12, 50, 0)
               //ToDate: PreviousCandleDatetime,    // 2016-01-01 01:10:00 PM
               Interval: Constants.INTERVAL_MINUTE,
               Continuous: false
           );


            if (historical.Count >= 3)
            {

                decimal candle0;
                decimal candle1;


                if (BuyOrSell.ToUpper() == "BUY")
                {
                    PresentCandleAvgPrice = (historical[2].Open + historical[2].High) / 2.0m;

                    candle0 = historical[0].Close - historical[0].Open;
                    candle1 = historical[1].Close - historical[1].Open;

                    if (candle1 > candle0 && (historical[1].Close < PresentCandleAvgPrice))
                    {
                        buyOrNot = true;
                    }
                }
                else if (BuyOrSell.ToUpper() == "SELL")
                {
                    PresentCandleAvgPrice = (historical[2].Open + historical[2].Low) / 2.0m;
                    candle0 = historical[0].Open - historical[0].Close;
                    candle1 = historical[1].Open - historical[1].Close;

                    if (candle1 > candle0 && (historical[1].Close > PresentCandleAvgPrice))
                    {
                        buyOrNot = true;
                    }
                }
            }

            PositionResponse positionResponse = kite.GetPositions();

            List<Position> Netpositions = positionResponse.Net.FindAll(position => position.Quantity > 0 && position.InstrumentToken==instrumentToken);

            if (Netpositions.Count > 0)
            {
                buyOrNot = false;
            }



            return buyOrNot;
        }


        public static DateTime RemoveSeconds(DateTime dateTime)
        {
            // Create a new DateTime object with seconds component set to zero
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0);
        }

        static DateTime FindNextWednesday(DateTime date)
        {
            // Find the day of the week of the given date
            DayOfWeek dayOfWeek = date.DayOfWeek;

            // Calculate the number of days to add to reach the next Wednesday
            int daysToAdd = ((int)DayOfWeek.Wednesday - (int)dayOfWeek + 7) % 7;

            // Add the calculated number of days to the given date to get the next Wednesday
            DateTime nextWednesday = date.AddDays(daysToAdd);

            return nextWednesday;
        }

        static bool IsLastWednesdayOfMonth(DateTime date)
        {


            // Check if the given date is a Wednesday
            if (date.DayOfWeek != DayOfWeek.Wednesday)
                return false;

            // Check if it's the last Wednesday of the month
            DateTime nextDay = date.AddDays(7); // Get the next Wednesday
            return nextDay.Month != date.Month; // If the next Wednesday is in the next month, then it's the last Wednesday of the current month
        }

        static bool IsLastThursdayOfMonth(DateTime date)
        {


            // Check if the given date is a Wednesday
            if (date.DayOfWeek != DayOfWeek.Thursday)
                return false;

            // Check if it's the last Wednesday of the month
            DateTime nextDay = date.AddDays(7); // Get the next Wednesday
            return nextDay.Month != date.Month; // If the next Wednesday is in the next month, then it's the last Wednesday of the current month
        }

        static DateTime FindNextThursday(DateTime date)
        {
            // Find the day of the week of the given date
            DayOfWeek dayOfWeek = date.DayOfWeek;

            // Calculate the number of days to add to reach the next Wednesday
            int daysToAdd = ((int)DayOfWeek.Thursday - (int)dayOfWeek + 7) % 7;

            // Add the calculated number of days to the given date to get the next Wednesday
            DateTime nextWednesday = date.AddDays(daysToAdd);

            return nextWednesday;
        }


        //[HttpGet("TestOpenInterest")]
        //public IActionResult TestOpenInterest()
        //{
        //    string TradingSymbolComputed = "";

        //    string year, month, day;

        //    Dictionary<string, LTP> LPTData;
        //    uint instrumentToken = 0;

        //    DateTime PresentCandleDatetime =  new DateTime(2024, 3, 19, 11, 20, 0);//DateTime.Now;
        //    DateTime PreviousCandleDatetime = RemoveSeconds(PresentCandleDatetime.AddMinutes(-60));

        //    Dictionary<string, LTP> ltps = kite.GetLTP(InstrumentId: new string[] { "NSE:NIFTY BANK" });
        //    double BankNiftyValue = Math.Round(Convert.ToInt32(ltps["NSE:NIFTY BANK"].LastPrice) / 100.0) * 100;

        //    DateTime nextWednessDay = FindNextWednesday(DateTime.UtcNow);

        //    year = nextWednessDay.Year.ToString().Substring(2);
        //    month = nextWednessDay.Month.ToString();
        //    day = nextWednessDay.Day.ToString();

        //    TradingSymbolComputed = "BANKNIFTY" + year + month + day + BankNiftyValue + "CE";
        //    LPTData = kite.GetLTP(InstrumentId: new string[] { "NFO:" + TradingSymbolComputed });
        //    instrumentToken = LPTData["NFO:" + TradingSymbolComputed].InstrumentToken;

        //    List<Historical> historical = kite.GetHistoricalData(
        //       InstrumentToken: instrumentToken.ToString(),
        //       FromDate: PreviousCandleDatetime,
        //       ToDate: PresentCandleDatetime,
        //       //FromDate: PresentCandleDatetime,   // 2016-01-01 12:50:00 AM   FromDate: new DateTime(2016, 1, 1, 12, 50, 0)
        //       //ToDate: PreviousCandleDatetime,    // 2016-01-01 01:10:00 PM
        //       Interval: Constants.INTERVAL_3MINUTE,
        //       OI: true,
        //       Continuous: false
        //   );


        //    TradingSymbolComputed = "BANKNIFTY" + year + month + day + BankNiftyValue + "PE";

        //    LPTData = kite.GetLTP(InstrumentId: new string[] { "NFO:" + TradingSymbolComputed });
        //    instrumentToken = LPTData["NFO:" + TradingSymbolComputed].InstrumentToken;

        //    List<Historical> historical2 = kite.GetHistoricalData(
        //      InstrumentToken: instrumentToken.ToString(),
        //      FromDate: PreviousCandleDatetime,
        //      ToDate: PresentCandleDatetime,
        //      //FromDate: PresentCandleDatetime,   // 2016-01-01 12:50:00 AM   FromDate: new DateTime(2016, 1, 1, 12, 50, 0)
        //      //ToDate: PreviousCandleDatetime,    // 2016-01-01 01:10:00 PM
        //      Interval: Constants.INTERVAL_3MINUTE,
        //      OI: true,
        //      Continuous: false
        //  );

        //    return Ok();

        //}



        }



    }
