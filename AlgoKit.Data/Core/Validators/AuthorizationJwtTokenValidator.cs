using AlgoKit.Data.Core.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace AksharaClothing.Data.Core.Validators
{
    public class AuthorizationJwtTokenValidator
    {
        private readonly ClaimsIdentity _claimsIdentity;

        //private readonly IUserService _userService;
        //public AuthorizationJwtTokenValidator(IUserService userService)
        //{
        //    _userService = userService;
        //}
        //public bool IsTokenValid(ClaimsPrincipal claimsPrincipal)
        //{
        //    string Pkey = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "PKey").Value;
        //    string userType = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "RolePolicy").Value;
        //    string emailID = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == "EmailID").Value;
        //    string phoneNumber = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == "PhoneNumber").Value;

        //    string userExists = _userService.ValidateTokenParameters(Convert.ToInt32(Pkey), emailID, Convert.ToInt64(phoneNumber), userType);

        //    if (userExists == "EXISTS")
        //    {
        //        return true;

        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
    }
}
