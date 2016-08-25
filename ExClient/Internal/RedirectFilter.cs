﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient.Internal
{
    internal class RedirectFilter : IHttpFilter
    {
        public RedirectFilter(IHttpFilter innerFilter)
        {
            inner = innerFilter;
        }

        IHttpFilter inner;

        public IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> SendRequestAsync(HttpRequestMessage request)
        {
            return Run<HttpResponseMessage, HttpProgress>(async (token, progress) =>
            {
                HttpResponseMessage response = null;
                do
                {
                    if(response != null)
                    {
                        var newRequest = new HttpRequestMessage();
                        newRequest.RequestUri = response.Headers.Location;
                        if((response.StatusCode == HttpStatusCode.Found || response.StatusCode == HttpStatusCode.SeeOther) && request.Method == HttpMethod.Post)
                            newRequest.Method = HttpMethod.Get;
                        else
                            newRequest.Method = request.Method;
                        foreach(var item in request.Headers.ToList())
                        {
                            newRequest.Headers.Add(item);
                        }
                        foreach(var item in request.Properties.ToList())
                        {
                            newRequest.Properties.Add(item);
                        }
                        request = newRequest;
                        response = null;
                    }
                    var task = inner.SendRequestAsync(request);
                    task.Progress = (sender, p) => progress.Report(p);
                    response = await task;
                } while(response.StatusCode == HttpStatusCode.MultipleChoices ||
                response.StatusCode == HttpStatusCode.MovedPermanently ||
                response.StatusCode == HttpStatusCode.Found ||
                response.StatusCode == HttpStatusCode.SeeOther ||
                response.StatusCode == HttpStatusCode.TemporaryRedirect ||
                response.StatusCode == HttpStatusCode.PermanentRedirect);
                return response;
            });
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if(!disposedValue)
            {
                if(disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    inner.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~RedirectFilter() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}