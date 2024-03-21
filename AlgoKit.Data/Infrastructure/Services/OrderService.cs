using Dapper;
using AlgoKit.Data.Core.Constants;
using AlgoKit.Data.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using AlgoKit.Data.Core.Interfaces.Services;
using AlgoKit.Data.Core.Interfaces;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace AlgoKit.Data.Infrastructure.Services
{
    public class OrderService : IOrderService
    {

        private readonly IAlgoKitConnectionFactory _AksharaClothingConnectionFactory;
        private static readonly HttpClient client = new HttpClient();

        public OrderService(IAlgoKitConnectionFactory AksharaClothingConnectionFactory)
        {
            _AksharaClothingConnectionFactory = AksharaClothingConnectionFactory;
        }


        public void UpdateKiteAPITokens(string userID, string AccessToken, string RefreshToken)
        {
            try
            {
                using (SqlConnection sqlConn = _AksharaClothingConnectionFactory.CreateConnection())
                {
                    var reader = sqlConn.Query(StoredProcedures.Update_KiteAPITokens, new
                    {
                        userID,
                        AccessToken,
                        RefreshToken
                    }, commandTimeout: 120, commandType: CommandType.StoredProcedure);

                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);

            }
        }


        public string GetRefreshToken(string userID)
        {
            try
            {
                using (SqlConnection sqlConn = _AksharaClothingConnectionFactory.CreateConnection())
                {
                    return sqlConn.Query<string>(StoredProcedures.GET_RefreshToken, new
                    {
                        userID,
                    }, commandTimeout: 120, commandType: CommandType.StoredProcedure).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
                return null;
            }
        }

        public string GetAccessToken(string userID)
        {
            try
            {
                using (SqlConnection sqlConn = _AksharaClothingConnectionFactory.CreateConnection())
                {
                    return sqlConn.Query<string>(StoredProcedures.GET_AccessToken, new
                    {
                        userID,
                    }, commandTimeout: 120, commandType: CommandType.StoredProcedure).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
                return null;
            }
        }


        public void LogException(string exceptionMessage)
        {
            try
            {
                using (SqlConnection sqlConn = _AksharaClothingConnectionFactory.CreateConnection())
                {
                    sqlConn.Query(StoredProcedures.INSERT_EXCEPTION, new
                    {
                        exceptionMessage,
                    }, commandTimeout: 120, commandType: CommandType.StoredProcedure);

                }
               // client.GetAsync("https://2factor.in/API/V1/a2cbd769-9ef3-11e8-a895-0200cd936042/SMS/" + 9640059446 + "/" +1234 + "/AksharaClothing_OTP");

            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
        }



        public string InsertSuperTrendSignal(string Signal, string Period)
        {
            try
            {
                using (SqlConnection sqlConn = _AksharaClothingConnectionFactory.CreateConnection())
                {
                    return sqlConn.Query<string>(StoredProcedures.INSERT_SignalData, new
                    {
                       Signal = Signal.ToUpper(),
                       Period = Period.ToUpper()

                    }, commandTimeout: 120, commandType: CommandType.StoredProcedure).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
                return null;
            }
        }

        public string GetTradeSignal()
        {
            try
            {
                using (SqlConnection sqlConn = _AksharaClothingConnectionFactory.CreateConnection())
                {
                    return sqlConn.Query<string>(StoredProcedures.GET_SignalSummaryToTrade, new
                    {
                    }, commandTimeout: 120, commandType: CommandType.StoredProcedure).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
                return null;
            }
        }


    }

    }