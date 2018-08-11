using System;
using System.Collections.Generic;
using System.Text;
using Rabbit.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using System.Threading.Tasks;

namespace Jacob.Common
{

    [ServiceTagAttribute("IUserService")]
    public interface IUserService
    {
        Task<string> GetUserName(int id);

        Task<bool> Exists(int id);

        Task<int> GetUserId(string userName);

        Task<DateTime> GetUserLastSignInTime(int id);

        Task<UserModel> GetUser(int id);

        Task<bool> Update(int id, UserModel model);

        Task<IDictionary<string, string>> GetDictionary();
   
        Task Try();

        Task TryThrowException();

    }
}
