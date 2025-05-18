using bflex.facturacion.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace bflex.facturacion.SunatCore
{
    public class ConexionExterna
    {
        public static string GestionarTokenPHP(string token, string parametro, string valor)
        {
            string output = "";
            
            RestClient servidorPhp = new RestClient("https://mysupportproject.pe/");
            RestRequest peticion = new RestRequest("/utilities/token.php");

            if (!String.IsNullOrWhiteSpace(token))
            {
                peticion.Method = Method.GET;
                peticion.AddParameter("token", token);
            }
            else
            {
                peticion.Method = Method.POST;
            }

            if (!String.IsNullOrWhiteSpace(valor))
            {
                peticion.AddParameter(parametro, valor);
            }

            output = servidorPhp.Execute(peticion).Content;
            return output;
        }

        public static async Task<string> GestionarTokenPHP(string[] keys, string[] values)
        {
            RestClient servidorPhp = new RestClient("https://mysupportproject.pe/");
            RestRequest peticion = new RestRequest("/utilities/token.php", Method.POST);

            if (keys.Length == values.Length)
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    peticion.AddParameter(keys[i], values[i]);
                }
            }

            IRestResponse respuesta = await servidorPhp.ExecuteAsync(peticion);
            return respuesta.Content;
        }
    }
}