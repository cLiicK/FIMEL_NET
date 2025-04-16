using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Fimel.Utils
{
    public class APIClient
    {
        private string BASE_URL { get; set; }
        private string API_KEY { get; set; }

        private readonly HttpClient Client = new HttpClient();

        public APIClient(string baseURI)
        {
            this.BASE_URL = baseURI;

            this.Client.BaseAddress = new Uri(BASE_URL);
            this.Client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            this.Client.Timeout = TimeSpan.FromMinutes(30);
        }
        public APIClient(string baseURI, string apiKey)
        {
            this.BASE_URL = baseURI;
            this.API_KEY = apiKey;

            this.Client.BaseAddress = new Uri(this.BASE_URL);
            this.Client.DefaultRequestHeaders.Add("API_KEY", this.API_KEY);
            this.Client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            this.Client.Timeout = TimeSpan.FromMinutes(30);
            //this.Client.Timeout = TimeSpan.FromSeconds(5);
        }

        public string ObtenerBASE_URL()
        {
            return BASE_URL;
        }

        public void AddHeaders(string nombre, string valor)
        {
            try
            {
                if (this.Client.DefaultRequestHeaders.Contains(nombre))
                    this.Client.DefaultRequestHeaders.Remove(nombre);
                this.Client.DefaultRequestHeaders.Add(nombre, valor);
            }
            catch (Exception ex)
            {
                Logger.Log("Error AddHeaders", ex.ToString());
                throw;
            }
        }

        public T Get<T>(string URI)
        {
            try
            {
                HttpResponseMessage response = this.Client.GetAsync(URI).Result;

                if (!this.ResponseOk(response.StatusCode))
                    return default(T);

                Task<string> result = response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<T>(result.Result.ToString());
                return obj;

            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                Logger.Log(URI);
                throw ex;
            }
        }
        public T Get<T>(string URI, string apOrigen, string metodoOrigen)
        {
            try
            {
                AddHeaders("AppOrigen", apOrigen);
                AddHeaders("MetOrigen", metodoOrigen);
                AddHeaders("ReqId", DateTimeOffset.UtcNow.Ticks.ToString());
                HttpResponseMessage response = this.Client.GetAsync(URI).Result;

                if (!this.ResponseOk(response.StatusCode))
                    return default(T);

                Task<string> result = response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<T>(result.Result.ToString());
                return obj;

            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                Logger.Log(URI);
                throw ex;
            }
        }
        public async Task<T> GetAsync<T>(string URI)
        {
            try
            {
                HttpResponseMessage response = await this.Client.GetAsync(URI).ConfigureAwait(false);

                if (!this.ResponseOk(response.StatusCode))
                    return default(T);

                var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var obj = JsonConvert.DeserializeObject<T>(result);
                return obj;

            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                Logger.Log(URI);
                throw ex;
            }
        }
        public T Get<T>(string URI, object patemetersObject)
        {
            try
            {
                var keyValueContent = patemetersObject.ToKeyValue();
                var formUrlEncodedContent = new FormUrlEncodedContent(keyValueContent);
                var urlEncodedString = formUrlEncodedContent.ReadAsStringAsync();

                HttpResponseMessage response = this.Client.GetAsync(URI + "?" + urlEncodedString.Result).Result;

                if (response.StatusCode != HttpStatusCode.OK)
                    return default(T);

                Task<string> result = response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<T>(result.Result.ToString());
                return obj;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                Logger.Log(URI);
                throw ex;
            }
        }

        public T Post<T>(string URI, T obj)
        {
            try
            {
                string jsonParams = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects });
                var content = new StringContent(jsonParams, Encoding.UTF8, "application/json");
                HttpResponseMessage response = this.Client.PostAsync(URI, content).GetAwaiter().GetResult();

                if (!this.ResponseOk(response.StatusCode))
                    return default(T);

                var result = response.Content.ReadAsStringAsync();//.GetAwaiter().GetResult();
                var obj2 = JsonConvert.DeserializeObject<T>(result.Result);
                return obj2;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                Logger.Log(URI);
                Logger.Log(JsonConvert.SerializeObject(obj, Formatting.Indented));
                throw ex;
            }
        }

        public async Task<T> PostAsync<T>(string URI, T obj)
        {
            try
            {
                string jsonParams = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects });
                var content = new StringContent(jsonParams, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await this.Client.PostAsync(URI, content).ConfigureAwait(false);

                if (!this.ResponseOk(response.StatusCode))
                    return default(T);

                var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);//.GetAwaiter().GetResult();
                var obj2 = JsonConvert.DeserializeObject<T>(result);
                return obj2;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                Logger.Log(URI);
                Logger.Log(JsonConvert.SerializeObject(obj, Formatting.Indented));
                throw ex;
            }
        }

        public T PostReferenceHandlingNone<T>(string URI, T obj)
        {
            try
            {
                //string jsonParams = JsonConvert.SerializeObject(obj);
                string jsonParams = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.None });
                var content = new StringContent(jsonParams, Encoding.UTF8, "application/json");
                HttpResponseMessage response = this.Client.PostAsync(URI, content).GetAwaiter().GetResult();

                if (!this.ResponseOk(response.StatusCode))
                    return default(T);

                var result = response.Content.ReadAsStringAsync();//.GetAwaiter().GetResult();
                var obj2 = JsonConvert.DeserializeObject<T>(result.Result);
                return obj2;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                Logger.Log(URI);
                Logger.Log(JsonConvert.SerializeObject(obj, Formatting.Indented));
                throw ex;
            }
        }

        public T PostASRL<T>(string URI, T obj)
        {
            try
            {
                //string jsonParams = JsonConvert.SerializeObject(obj);
                string jsonParams = JsonConvert.SerializeObject(obj, Formatting.Indented);
                var content = new StringContent(jsonParams, Encoding.UTF8, "application/json");
                HttpResponseMessage response = this.Client.PostAsync(URI, content).GetAwaiter().GetResult();

                if (!this.ResponseOk(response.StatusCode))
                    return default(T);

                var result = response.Content.ReadAsStringAsync();//.GetAwaiter().GetResult();
                var obj2 = JsonConvert.DeserializeObject<T>(result.Result);
                return obj2;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                Logger.Log(URI);
                Logger.Log(JsonConvert.SerializeObject(obj, Formatting.Indented));
                throw ex;
            }
        }

        public HttpResponseMessage PostRequest<T>(string URI, T obj)
        {
            try
            {
                string jsonParams = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects });
                var content = new StringContent(jsonParams, Encoding.UTF8, "application/json");
                HttpResponseMessage response = this.Client.PostAsync(URI, content).GetAwaiter().GetResult();
                return response;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                Logger.Log(URI);
                Logger.Log(JsonConvert.SerializeObject(obj, Formatting.Indented));
                throw ex;
            }
        }

        public T PostReferencesHandlingNone<T, A>(string URI, A obj)
        {
            try
            {
                string jsonParams = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.None });
                var content = new StringContent(jsonParams, Encoding.UTF8, "application/json");
                //Logger.Log(jsonParams);
                HttpResponseMessage response = this.Client.PostAsync(URI, content).GetAwaiter().GetResult();

                if (!this.ResponseOk(response.StatusCode))
                {
                    Logger.Log(response.ToString());
                    if (response.Content != null)
                        Logger.Log(response.Content.ReadAsStringAsync().Result);
                    Logger.Log(jsonParams);
                    return default;
                }

                var result = response.Content.ReadAsStringAsync();//.GetAwaiter().GetResult();
                var obj2 = JsonConvert.DeserializeObject<T>(result.Result);
                return obj2;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                Logger.Log(URI);
                Logger.Log(JsonConvert.SerializeObject(obj, Formatting.Indented));
                throw ex;
            }
        }

        public T Post<T, A>(string URI, A obj)
        {
            try
            {
                string jsonParams = JsonConvert.SerializeObject(obj, Formatting.Indented);
                var content = new StringContent(jsonParams, Encoding.UTF8, "application/json");
                //Logger.Log(jsonParams);
                HttpResponseMessage response = this.Client.PostAsync(URI, content).GetAwaiter().GetResult();

                if (!this.ResponseOk(response.StatusCode))
                {
                    Logger.Log(response.ToString());
                    if (response.Content != null)
                    {
                        string msj = response.Content.ReadAsStringAsync().Result;
                        Logger.Log(msj);
                    }
                    Logger.Log(jsonParams);
                }

                var result = response.Content.ReadAsStringAsync();//.GetAwaiter().GetResult();
                var obj2 = JsonConvert.DeserializeObject<T>(result.Result);
                return obj2;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                Logger.Log(URI);
                Logger.Log(JsonConvert.SerializeObject(obj, Formatting.Indented));
                throw ex;
            }
        }

        public T PostDynamic<T, A>(string URI, A obj)
        {
            try
            {
                string jsonParams = JsonConvert.SerializeObject(obj, Formatting.Indented);
                var content = new StringContent(jsonParams, Encoding.UTF8, "application/json");
                //Logger.Log(jsonParams);
                HttpResponseMessage response = this.Client.PostAsync(URI, content).GetAwaiter().GetResult();

                if (!this.ResponseOk(response.StatusCode))
                {
                    Logger.Log(response.ToString());
                    if (response.Content != null)
                        Logger.Log(response.Content.ReadAsStringAsync().Result);
                    Logger.Log(jsonParams);
                    return default;
                }

                var result = response.Content.ReadAsStringAsync();//.GetAwaiter().GetResult();
                var obj2 = JsonConvert.DeserializeObject<T>(result.Result);
                return obj2;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                Logger.Log(URI);
                Logger.Log(JsonConvert.SerializeObject(obj, Formatting.Indented));
                throw ex;
            }
        }

        public T PostReferencesHandlingObject<T, A>(string URI, A obj)
        {
            try
            {
                string jsonParams = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects });
                var content = new StringContent(jsonParams, Encoding.UTF8, "application/json");
                HttpResponseMessage response = this.Client.PostAsync(URI, content).GetAwaiter().GetResult();

                if (!this.ResponseOk(response.StatusCode))
                {
                    Logger.Log(response.ToString());
                    if (response.Content != null)
                        Logger.Log(response.Content.ReadAsStringAsync().Result);
                    Logger.Log(jsonParams);
                    return default;
                }

                var result = response.Content.ReadAsStringAsync();//.GetAwaiter().GetResult();
                var obj2 = JsonConvert.DeserializeObject<T>(result.Result);
                return obj2;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                Logger.Log(URI);
                Logger.Log(JsonConvert.SerializeObject(obj, Formatting.Indented));
                throw ex;
            }
        }

        public HttpResponseMessage Post(string URI)
        {
            try
            {
                var content = new StringContent("", Encoding.UTF8, "application/json");
                HttpResponseMessage response = this.Client.PostAsync(URI, content).GetAwaiter().GetResult();

                var result = response.Content.ReadAsStringAsync();

                if (!this.ResponseOk(response.StatusCode))
                    return null;

                return response;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                Logger.Log(URI);
                throw ex;
            }
        }

        public T Put<T>(string URI, T obj)
        {
            try
            {
                string jsonParams = JsonConvert.SerializeObject(obj, Formatting.None,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                HttpResponseMessage response = this.Client.PutAsync(URI, new StringContent(jsonParams, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();

                if (!this.ResponseOk(response.StatusCode))
                    return default(T);

                var content = response.Content.ReadAsStringAsync();
                var obj2 = JsonConvert.DeserializeObject<T>(content.Result);
                return obj2;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                Logger.Log(URI);
                Logger.Log(JsonConvert.SerializeObject(obj, Formatting.Indented));
                throw ex;
            }
        }

        public async Task<T> PutAsync<T>(string URI, T obj)
        {
            try
            {
                string jsonParams = JsonConvert.SerializeObject(obj, Formatting.None,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                HttpResponseMessage response = await this.Client.PutAsync(URI, new StringContent(jsonParams, Encoding.UTF8, "application/json")).ConfigureAwait(false);

                if (!this.ResponseOk(response.StatusCode))
                    return default(T);

                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var obj2 = JsonConvert.DeserializeObject<T>(content);
                return obj2;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                Logger.Log(URI);
                Logger.Log(JsonConvert.SerializeObject(obj, Formatting.Indented));
                throw ex;
            }
        }
        public HttpResponseMessage PutRequest<T>(string URI, T obj)
        {
            try
            {
                string jsonParams = JsonConvert.SerializeObject(obj, Formatting.None,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                HttpResponseMessage response = this.Client.PutAsync(URI, new StringContent(jsonParams, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();
                return response;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                Logger.Log(URI);
                Logger.Log(JsonConvert.SerializeObject(obj, Formatting.Indented));
                throw ex;
            }
        }

        public string PutStr(string URI, object obj)
        {
            try
            {
                string jsonParams = JsonConvert.SerializeObject(obj, Formatting.None,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                HttpResponseMessage response = this.Client.PutAsync(URI, new StringContent(jsonParams, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();

                if (!this.ResponseOk(response.StatusCode))
                    return null;

                var content = response.Content.ReadAsStringAsync();
                return content.Result;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                Logger.Log(URI);
                Logger.Log(JsonConvert.SerializeObject(obj, Formatting.Indented));
                throw ex;
            }
        }

        public T Delete<T>(string URI)
        {
            try
            {
                HttpResponseMessage response = this.Client.DeleteAsync(URI).GetAwaiter().GetResult();

                if (!this.ResponseOk(response.StatusCode))
                    return default(T);

                var content = response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<T>(content.Result);
                return obj;
            }
            catch (Exception ex)
            {
                Logger.Log(URI);
                Logger.Log(ex.ToString());
                throw ex;
            }
        }

        public bool ResponseOk(HttpStatusCode httpStatusCode)
        {
            switch (httpStatusCode)
            {
                case HttpStatusCode.OK:
                case HttpStatusCode.Created:
                case HttpStatusCode.Accepted:
                case HttpStatusCode.NonAuthoritativeInformation:
                case HttpStatusCode.NoContent:
                case HttpStatusCode.ResetContent:
                case HttpStatusCode.PartialContent:
                    return true;
                default:
                    return false;
            }
        }
    }
}
