using System.Security.Claims;
using System.ServiceModel;
using System.Text;

namespace WcfService
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        string Ping();
    }

    class Service : IService
    {
        public string Ping()
        {
            var sb = new StringBuilder();

            foreach (var claim in ClaimsPrincipal.Current.Claims)
            {
                sb.AppendFormat("{0} :: {1}\n", claim.Type, claim.Value);
            }

            return sb.ToString();
        }
    }
}