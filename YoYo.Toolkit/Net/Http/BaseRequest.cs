using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using YoYo.Toolkit.Net.Enums;

namespace YoYo.Toolkit.Net.Http
{
    public abstract class BaseRequest
    {
        /// <summary>
        /// 请求根路径
        /// </summary>
        public abstract string RootUrl { get; }
        /// <summary>
        /// 业务路径
        /// </summary>
        public abstract string BusinessUrl { get; }
        /// <summary>
        /// 请求
        /// </summary>
        public virtual HttpMethodTypes Method { get; private set; } = HttpMethodTypes.GET;

        public virtual string ContentType { get; set; } = "application/json";
        /// <summary>
        /// 显示进度条
        /// </summary>
        public virtual bool IsShowProcess { get; } = true;
        /// <summary>
        /// 请求返回类
        /// </summary>
        public abstract Type ResponseType { get; }

        /// <summary>
        /// 序列化对象
        /// </summary>
        public abstract void SerializeObject();

        /// <summary>
        /// 请求结束
        /// </summary>
        public event Action<BaseResponse>? DoServiceAsyncCompleted;
        /// <summary>
        /// 错误
        /// </summary>
        public event Action<string, Exception>? Error;

        /// <summary>
        /// 返回
        /// </summary>
        BaseResponse? response = null;

        /// <summary>
        /// 请求数据
        /// </summary>
        public string Data { get; set; } = string.Empty;



        #region 请求任务

        #region 进度

        /// <summary>
        /// 显示进度
        /// </summary>
        public virtual void ShowProcess()
        {

        }

        /// <summary>
        /// 关闭进度
        /// </summary>
        public virtual void CloseProcess()
        {

        }

        #endregion

        /// <summary>
        /// 请求
        /// </summary>
        public void DoWorkerAsync()
        {
            BaseRequest request = this;
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (e, s) =>
            {
                try
                {
                    if (request == null) throw new Exception("转换出错!");
                    request.SerializeObject();
                    string? result = "";
                    switch (Method)
                    {
                        case HttpMethodTypes.POST:
                            result = Post(BusinessUrl, Data);
                            break;
                        case HttpMethodTypes.GET:
                            result = Get(BusinessUrl);
                            break;
                        case HttpMethodTypes.GetBody:
                            result = GetBody(BusinessUrl, Data);
                            break;
                        case HttpMethodTypes.DELETE:
                            result = Delete(BusinessUrl, Data);
                            break;
                        case HttpMethodTypes.PUT:
                            result = Put(BusinessUrl, Data);
                            break;
                        default:
                            break;
                    }

                    if (request.ResponseType != null)
                    {
                        response = Activator.CreateInstance(request.ResponseType) as BaseResponse;

                    }
                    if (response != null)
                    {
                        response.Data = result ?? "";
                        response.DeserializeObj();
                    }
                }
                catch (Exception ex)
                {
                    if (IsShowProcess)
                    {
                        CloseProcess();
                    }
                    Error?.Invoke($"请求发生错误{ex}{ex.Message}", ex);
                }
                finally
                {

                }
            };
            worker.RunWorkerCompleted += (s, e) =>
            {
                if (IsShowProcess)
                {
                    CloseProcess();
                }
                if (response != null)
                    DoServiceAsyncCompleted?.Invoke(response);

            };
            if (IsShowProcess)
                ShowProcess();
            worker.RunWorkerAsync();

        }

       


        /// <summary>
        /// 同步请求
        /// </summary>
        /// <returns></returns>
        public BaseResponse? DoWorkerSync()
        {
            BaseRequest request = this;

            request.SerializeObject();

            string? result = string.Empty;
            switch (Method)
            {
                case HttpMethodTypes.POST:
                    result = Post(BusinessUrl, Data);
                    break;
                case HttpMethodTypes.GET:
                    result = Get(BusinessUrl);
                    break;
                case HttpMethodTypes.GetBody:
                    result = GetBody(BusinessUrl, Data);
                    break;
                case HttpMethodTypes.DELETE:
                    result = Delete(BusinessUrl, Data);
                    break;
                case HttpMethodTypes.PUT:
                    result = Put(BusinessUrl, Data);
                    break;
                default:
                    break;
            }
            response = Activator.CreateInstance(request.ResponseType) as BaseResponse;
            if (!string.IsNullOrWhiteSpace(result) && response != null)
            {
                response.Data = result;
                try
                {
                    response.DeserializeObj();
                    response.IsOk = true;
                }
                catch (Exception ex)
                {
                    Error?.Invoke("解析发生错误！", ex);
                    response.IsOk = false;
                    response.Msg = "服务器未返回信息!";
                }
            }
            else
            {
                if (response != null)
                {
                    response.IsOk = false;
                    response.Msg = "服务器未返回信息!";
                }
            }
            return response;
        }
        #endregion

        #region 请求


        public virtual void SetHeader(HttpClient client)
        {

        }

        private string? GetBody(string apiPath, string data)
        {
            string url = $"{RootUrl}{apiPath}";
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {

                    SetHeader(httpClient);
                    HttpContent content = new StringContent(data, Encoding.UTF8, ContentType); // 请求体内容  

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Content = content;
                    HttpResponseMessage response = httpClient.SendAsync(request).Result;
                    response.EnsureSuccessStatusCode();
                    string result = response.Content.ReadAsStringAsync().Result;
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Error?.Invoke($"当前请求:{url}发生错误!", ex);
            }

            return null;
        }
        private string? Get(string apiPath)
        {
            string url = $"{RootUrl}{apiPath}";
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    SetHeader(httpClient);
                    var result = httpClient.GetAsync(url).Result.Content.ReadAsStringAsync().Result;
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Error?.Invoke($"当前请求:{url}发生错误!", ex);
            }

            return null;
        }
        /// <summary>
        /// Post请求
        /// </summary>
        /// <param name="apiPath"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private string? Post(string apiPath, string data)
        {
            string url = $"{RootUrl}{apiPath}";
            try
            {
                StringContent stringContent = new StringContent(data);
                using (HttpClient httpClient = new HttpClient())
                {
                    SetHeader(httpClient);
                    stringContent.Headers.ContentType = new MediaTypeHeaderValue(ContentType) { CharSet = "UTF-8" };
                    var result = httpClient.PostAsync(url, stringContent).Result.Content.ReadAsStringAsync().Result;
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Error?.Invoke($"当前请求:{url}发生错误!", ex);
            }

            return null;
        }
        private string? Delete(string apiPath, string data)
        {
            string url = $"{RootUrl}{apiPath}";
            try
            {
                StringContent stringContent = new StringContent(data);
                using (HttpClient httpClient = new HttpClient())
                {
                    SetHeader(httpClient);
                    stringContent.Headers.ContentType = new MediaTypeHeaderValue(ContentType) { CharSet= "UTF-8" };
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, url)
                    {
                        Content = stringContent
                    };

                   
                    var result = httpClient.SendAsync(request).Result.Content.ReadAsStringAsync().Result;
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Error?.Invoke($"当前请求:{url}发生错误!", ex);
            }

            return null;
        }

        private string Put(string apiPath, string data)
        {
            string url = $"{RootUrl}{apiPath}";
            try
            {
                StringContent stringContent = new StringContent(data);
                using (HttpClient httpClient = new HttpClient())
                {
                    SetHeader(httpClient);
                    stringContent.Headers.ContentType = new MediaTypeHeaderValue(ContentType) { CharSet = "UTF-8" };
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, url)
                    {
                        Content = stringContent
                    };


                    var result = httpClient.SendAsync(request).Result.Content.ReadAsStringAsync().Result;
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Error?.Invoke($"当前请求:{url}发生错误!", ex);
            }

            return null;
        }

        #endregion

    }
}
