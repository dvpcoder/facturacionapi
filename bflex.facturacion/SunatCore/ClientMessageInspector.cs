using Microsoft.Web.Services3.Security.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Web;
using System.Xml;

namespace bflex.facturacion.SunatCore
{
    public class ClientMessageInspector : IClientMessageInspector
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public ClientMessageInspector(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            return;
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            UsernameToken token = new UsernameToken(Username, Password, PasswordOption.SendPlainText);
            XmlElement securityToken = token.GetXml(new XmlDocument());

            XmlNode nodo = securityToken.GetElementsByTagName("wsse:Nonce").Item(0);
            if (nodo != null) nodo.RemoveAll();

            MessageHeader securityHeader = MessageHeader.CreateHeader(
                "Security", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd",
                securityToken, false
            );
            request.Headers.Add(securityHeader);

            return Convert.DBNull;
        }
    }
}