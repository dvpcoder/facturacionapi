using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace bflex.facturacion.Models
{
    public class Comercio
    {
        public int IdComercio { get; set; }
        public Cuenta Cuenta { get; set; }
        public String Ruc { get; set; }
        public String RazonSocial { get; set; }
        public String NombreComercial { get; set; }
        public String Email { get; set; }
        public String Celular { get; set; }
        public String RepresentanteLegal { get; set; }
        public String IdentidadRepresentante { get; set; }
        public String DescripcionNegocio { get; set; }
        public String ElaboradoPor { get; set; }
        public String AccessToken { get; set; }
        public String CalleFiscal { get; set; }
        public String UbigeoFiscal { get; set; }
        public String UrbanizacionFiscal { get; set; }
        public String DepartamentoFiscal { get; set; }
        public String ProvinciaFiscal { get; set; }
        public String DistritoFiscal { get; set; }
        public String CarpetaServidor { get; set; }
        public String ArchivoCertificado { get; set; }
        public String PasswordCertificado { get; set; }
        public String UsuarioSunat { get; set; }
        public String PasswordSunat { get; set; }
        public String Normativa { get; set; }
        public bool Efact { get; set; }
        public bool Nubefact { get; set; }
        public String CuentaDetraccion { get; set; }
        public String UsuarioOSE { get; set; }
        public String PasswordOSE { get; set; }
        public String SitioWeb { get; set; }
        public Decimal PorcentajePropina { get; set; }
        public Decimal PorcentajeIgv { get; set; }
        public String RutaLogo { get; set; }
    }
}