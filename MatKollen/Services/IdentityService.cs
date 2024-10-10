// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using MatKollen.Options;
// using Microsoft.Extensions.Options;

// namespace MatKollen.Services
// {
//     public class IdentityService
//     {
//         private readonly JwtSettings? _settings;
//         private readonly byte[] _key;

//         public IdentityService(IOptions<JwtSettings> jwtOptions)
//         {
//             _settings = jwtOptions.Value;
//             ArgumentNullException.ThrowIfNull(_settings)
//         }
//     }
// }