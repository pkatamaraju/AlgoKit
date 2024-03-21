using System;
using System.Collections.Generic;
using System.Text;
using AlgoKit.Data.Core.Models;
namespace AlgoKit.Data.Core.Interfaces.Services
{
    public interface IOrderService
    {
        void UpdateKiteAPITokens(string userID, string AccessToken, string RefreshToken);
        string GetRefreshToken(string userID);
        string GetAccessToken(string userID);
        void LogException(string exceptionMessage);

        string InsertSuperTrendSignal(string Signal, string Period);

        string GetTradeSignal();

    }
}