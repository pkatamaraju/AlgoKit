using System;
using System.Collections.Generic;
using System.Text;

namespace AlgoKit.Data.Core.Constants
{
    internal static class StoredProcedures
    {
        //APIKeys
        public const string Update_KiteAPITokens = "Update_KiteAPITokens";
        public const string GET_RefreshToken = "GET_RefreshToken";
        public const string GET_AccessToken= "GET_AccessToken";

        public const string INSERT_EXCEPTION = "INSERT_EXCEPTION";

        public const string INSERT_SignalData = "INSERT_SignalData";
        public const string GET_SignalSummaryToTrade = "GET_SignalSummaryToTrade";

    }
}
