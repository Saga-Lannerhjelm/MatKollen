using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MatKollen.Services
{
    public static class UserHelper
    {
        public static int GetUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

            if (int.TryParse(userIdClaim, out int result))
            {
                return result;
            }
            else
            {
                return 0;
            }
        }
    }
}