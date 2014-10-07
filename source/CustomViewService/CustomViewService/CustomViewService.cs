using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Views;

namespace SampleApp
{
    public class CustomViewService : IViewService
    {
        IClientStore clientStore;
        public CustomViewService(IClientStore clientStore)
        {
            this.clientStore = clientStore;
        }

        public virtual async Task<System.IO.Stream> Login(IDictionary<string, object> env, LoginViewModel model, SignInMessage message)
        {
            var client = await clientStore.FindClientByIdAsync(message.ClientId);
            return await Render(model, "login", client.ClientName);
        }

        public virtual Task<System.IO.Stream> Logout(IDictionary<string, object> env, LogoutViewModel model)
        {
            return Render(model, "logout");
        }

        public virtual Task<System.IO.Stream> LoggedOut(IDictionary<string, object> env, LoggedOutViewModel model)
        {
            return Render(model, "loggedOut");
        }

        public virtual Task<System.IO.Stream> Consent(IDictionary<string, object> env, ConsentViewModel model)
        {
            return Render(model, "consent");
        }

        public virtual Task<System.IO.Stream> Error(IDictionary<string, object> env, ErrorViewModel model)
        {
            return Render(model, "error");
        }

        protected virtual Task<System.IO.Stream> Render(CommonViewModel model, string page, string clientName = null)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.None, new Newtonsoft.Json.JsonSerializerSettings() { ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() });

            string html = LoadHtml(page);
            html = Replace(html, new {
                siteName = model.SiteName,
                model = json,
                clientName = clientName
            });
            
            return Task.FromResult(StringToStream(html));
        }

        private string LoadHtml(string name)
        {
            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"content\app");
            file = Path.Combine(file, name + ".html");
            return File.ReadAllText(file);
        }

        string Replace(string value, IDictionary<string, object> values)
        {
            foreach (var key in values.Keys)
            {
                value = value.Replace("{" + key + "}", values[key].ToString());
            }
            return value;
        }

        string Replace(string value, object values)
        {
            return Replace(value, Map(values));
        }

        IDictionary<string, object> Map(object values)
        {
            var dictionary = values as IDictionary<string, object>;

            if (dictionary == null)
            {
                dictionary = new Dictionary<string, object>();
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(values))
                {
                    dictionary.Add(descriptor.Name, descriptor.GetValue(values));
                }
            }

            return dictionary;
        }
        
        Stream StringToStream(string s)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            sw.Write(s);
            sw.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
    }
}