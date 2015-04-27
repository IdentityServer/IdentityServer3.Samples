using System;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel.Security;
using System.Xml;

namespace SampleWCFApiHost.CustomToken
{
    /// <summary>
    /// CustomSecurityTokenSerializer for use with the AccessToken
    /// </summary>
    /// 
    public class CustomSecurityTokenSerializer : WSSecurityTokenSerializer
    {
        public CustomSecurityTokenSerializer(SecurityTokenVersion version) : base() { }

        protected override bool CanReadTokenCore(XmlReader reader)
        {
            XmlDictionaryReader localReader = XmlDictionaryReader.CreateDictionaryReader(reader);

            if (reader == null) throw new ArgumentNullException("reader");

            if (reader.IsStartElement(Constants.CustomTokenName, Constants.CustomTokenNamespace))
                return true;

            return base.CanReadTokenCore(reader);
        }

        protected override SecurityToken ReadTokenCore(XmlReader reader, SecurityTokenResolver tokenResolver)
        {
            if (reader == null) throw new ArgumentNullException("reader");

            if (reader.IsStartElement(Constants.CustomTokenName, Constants.CustomTokenNamespace))
            {
                string id = reader.GetAttribute(Constants.Id, Constants.WsUtilityNamespace);

                reader.ReadStartElement();

                // read the Access Token
                string AccessToken = reader.ReadElementString(Constants.CustomTokenElementName, Constants.CustomTokenNamespace);

                reader.ReadEndElement();

                return new CustomToken(AccessToken, id);
            }
            else
            {
                return WSSecurityTokenSerializer.DefaultInstance.ReadToken(reader, tokenResolver);
            }
        }

        protected override bool CanWriteTokenCore(SecurityToken token)
        {
            if (token is CustomToken)
                return true;
            else
                return base.CanWriteTokenCore(token);
        }

        protected override void WriteTokenCore(XmlWriter writer, SecurityToken token)
        {
            if (writer == null) { throw new ArgumentNullException("writer"); }
            if (token == null) { throw new ArgumentNullException("token"); }

            CustomToken c = token as CustomToken;
            if (c != null)
            {
                writer.WriteStartElement(Constants.CustomTokenPrefix, Constants.CustomTokenName, Constants.CustomTokenNamespace);
                writer.WriteAttributeString(Constants.WsUtilityPrefix, Constants.Id, Constants.WsUtilityNamespace, token.Id);
                writer.WriteElementString(Constants.CustomTokenElementName, Constants.CustomTokenNamespace, c.AccessToken);

                writer.WriteEndElement();
                writer.Flush();
            }
            else
            {
                base.WriteTokenCore(writer, token);
            }
        }
    }
}
