using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Fimel.Models;
using Fimel.Models.Integraciones;
using iText.Html2pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace Fimel.Utils
{
    public class Utileria
    {
        private static IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        private static readonly APIClient APIBase = new APIClient(config["API_URL"]);
        public static readonly string SeparadorMiles = "#,##0";

        public Usuarios ObtenerSesion(string userJson)
        {
            try
            {
                Usuarios usuarioConectado = null;

                if (!string.IsNullOrEmpty(userJson))
                { usuarioConectado = System.Text.Json.JsonSerializer.Deserialize<Usuarios>(userJson); }

                return usuarioConectado;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al Obtener Usuario en Sesion: {ex}");
                return null;
            }
        }
        public T ObtenerData<T>(string json)
        {
            var obj = JsonConvert.DeserializeObject<T>(json);
            return obj;
        }
        public byte[] HtmlToPDF(string pathHtmlSource)
        {
            try
            {
                byte[] pdfBytes;

                string baseDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Documentos", "Recetas");
                Directory.CreateDirectory(baseDirectory);

                string pathPdfDest = Path.Combine(baseDirectory, $"{DateTime.Now.Ticks}.pdf");

                using (FileStream htmlSource = File.Open(pathHtmlSource, FileMode.Open))
                using (FileStream pdfDest = File.Open(pathPdfDest, FileMode.OpenOrCreate))
                {
                    ConverterProperties converterProperties = new ConverterProperties();
                    HtmlConverter.ConvertToPdf(htmlSource, pdfDest, converterProperties);
                    pdfBytes = System.IO.File.ReadAllBytes(pathPdfDest);
                    EliminarArchivo(pathPdfDest);
                }
                return pdfBytes;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error metodo HtmlToPDF (Utileria): {ex}");
                throw ex;
            }
        }
        public void EnviarCorreo(EnvioCorreo correo)
        {
            try
            {
                string destBitacora = string.Empty;
                string CCBitacora = string.Empty;
                string CCOBitacora = string.Empty;

                SmtpClient smtpClient = new SmtpClient(config["SMTP_Config:Domain"], Convert.ToInt32(config["SMTP_Config:Port"]));

                smtpClient.Credentials = new System.Net.NetworkCredential(config["SMTP_Config:Correo"], config["SMTP_Config:Contrasena"]);
                // smtpClient.UseDefaultCredentials = true; // uncomment if you don't want to use the network credentials
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.EnableSsl = false;
                MailMessage mail = new MailMessage();

                //Setting From , To and CC
                mail.From = new MailAddress(config["SMTP_Config:Correo"], config["SMTP_Config:Display_Name"]);

                if (correo.Destinatarios != null)
                {
                    foreach (var destinatario in correo.Destinatarios)
                    {
                        mail.To.Add(new MailAddress(destinatario));
                        destBitacora = string.Concat(destBitacora, ",", destinatario);
                    }
                }

                if (correo.CC != null)
                {
                    foreach (var CC in correo.CC)
                    {
                        mail.To.Add(new MailAddress(CC));
                        CCBitacora = string.Concat(CCBitacora, ",", CC);
                    }
                }

                if (correo.CCO != null)
                {
                    foreach (var CCO in correo.CCO)
                    {
                        mail.CC.Add(new MailAddress(CCO));
                        CCOBitacora = string.Concat(CCOBitacora, ",", CCO);
                    }
                }


                mail.Subject = correo.Asunto;
                mail.IsBodyHtml = true;
                mail.Body = correo.CuerpoCorreo;

                smtpClient.Send(mail);

                BitacoraMensajerias bitacora = new BitacoraMensajerias()
                {
                    Destinatarios = destBitacora,
                    CC = CCBitacora,
                    CCO = CCOBitacora,
                    Asunto = correo.Asunto,
                    CuerpoCorreo = correo.CuerpoCorreo,
                    Vigente = "S",
                    FechaCreacion = DateTime.Now
                };

                var postBitacora = APIBase.Post<BitacoraMensajerias>("BitacoraMensajerias", bitacora);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al EnviarCorreo: {ex}");
            }
        }
        public void EliminarArchivo(string rutaArchivo)
        {
            try
            {
                File.Delete(rutaArchivo);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al intentar eliminar archivo: {ex}");
            }
        }
        public string FormatearRut(int rut, string dv)
        {
            string rutFormateado = rut.ToString("D7");
            rutFormateado = string.Format("{0:#,0}", long.Parse(rutFormateado));

            return rutFormateado + "-" + dv;
        }
        public static string FormatearRutSinDv(int rut)
        {
            string text = DigitoVerificador(rut);
            return rut.ToString(SeparadorMiles).Replace(",", ".") + "-" + text;
        }
        public static string DigitoVerificador(int rut)
        {
            double num = rut;
            double num2 = 0.0;
            double num3 = 1.0;
            while (num != 0.0)
            {
                num3 = (num3 + num % 10.0 * (9.0 - num2++ % 6.0)) % 11.0;
                num = Math.Floor(num / 10.0);
            }

            return (num3 != 0.0) ? (num3 - 1.0).ToString() : "K";
        }
        public int CalcularEdad(DateTime fechaNacimiento)
        {
            var today = DateTime.Today;

            var age = today.Year - fechaNacimiento.Year;

            if (fechaNacimiento.Date > today.AddYears(-age)) age--;

            return age;
        }
        public static string Encrypt(string plainText)
        {
            try
            {
                string key = "jdsg432387#";
                byte[] EncryptKey = { };
                byte[] IV = { 55, 34, 87, 64, 87, 195, 54, 21 };
                EncryptKey = System.Text.Encoding.UTF8.GetBytes(key.Substring(0, 8));
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                byte[] inputByte = Encoding.UTF8.GetBytes(plainText);
                MemoryStream mStream = new MemoryStream();
                CryptoStream cStream = new CryptoStream(mStream, des.CreateEncryptor(EncryptKey, IV), CryptoStreamMode.Write);
                cStream.Write(inputByte, 0, inputByte.Length);
                cStream.FlushFinalBlock();
                return Convert.ToBase64String(mStream.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al encriptar: {ex}");
                return null;
            }
        }
        public static string Decrypt(string encryptedText)
        {
            try
            {
                string key = "jdsg432387#";
                byte[] DecryptKey = { };
                byte[] IV = { 55, 34, 87, 64, 87, 195, 54, 21 };
                byte[] inputByte = new byte[encryptedText.Length];

                DecryptKey = System.Text.Encoding.UTF8.GetBytes(key.Substring(0, 8));
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                inputByte = Convert.FromBase64String(encryptedText);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(DecryptKey, IV), CryptoStreamMode.Write);
                cs.Write(inputByte, 0, inputByte.Length);
                cs.FlushFinalBlock();
                System.Text.Encoding encoding = System.Text.Encoding.UTF8;
                return encoding.GetString(ms.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al desencriptar: {ex}");
                return null;
            }
        }
        public string DesencryptarBase64(string valor)
        {
            byte[] data = Convert.FromBase64String(valor);
            string originalValor = System.Text.Encoding.UTF8.GetString(data);

            return originalValor;
        }
        public int ConvertirDiaSemanaANumero(string dia)
        {
            var dias = new Dictionary<string, int>
    {
        { "Domingo", 0 },
        { "Lunes", 1 },
        { "Martes", 2 },
        { "Miércoles", 3 },
        { "Jueves", 4 },
        { "Viernes", 5 },
        { "Sábado", 6 }
    };

            return dias.ContainsKey(dia) ? dias[dia] : 0;
        }
    }
}
