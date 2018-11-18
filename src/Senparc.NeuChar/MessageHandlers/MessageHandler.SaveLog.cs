﻿using Senparc.CO2NET.APM;
using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.Trace;
using Senparc.CO2NET.Utilities;
using Senparc.NeuChar.Entities;
using Senparc.NeuChar.NeuralSystems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
#if NET35 || NET40 || NET45
using System.Web;
#endif
namespace Senparc.NeuChar.MessageHandlers
{
    public abstract partial class MessageHandler<TC, TRequest, TResponse>
    {
        readonly Func<string> _getRandomFileName = () => DateTime.Now.ToString("yyyyMMdd-HHmmss") + Guid.NewGuid().ToString("n").Substring(0, 6);

        #region 记录 日志 

        /// <summary>
        /// 获取日志保存地址
        /// </summary>
        /// <returns></returns>
        private string GetLogPath()
        {
#if NET35 || NET40 || NET45
            var appDomainAppPath = HttpRuntime.AppDomainAppPath;
#else
            var appDomainAppPath = AppContext.BaseDirectory; //dll所在目录：;
#endif

            var logPath = Path.Combine(appDomainAppPath, $"\\App_Data\\{this.MessageEntityEnlightener.PlatformType.ToString()}\\{ SystemTime.Now.ToString("yyyy-MM-dd")}\\");
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }

            return logPath;
        }

        /// <summary>
        /// 保存请求信息
        /// </summary>
        /// <param name="logPath">保存日志目录，默认为 ~/App_Data/&lt;模块类型&gt;/<yyyy-MM-dd>/</code></param>
        public void SaveRequestMessageLog(string logPath = null)
        {
            logPath = logPath ?? GetLogPath();
            //测试时可开启此记录，帮助跟踪数据，使用前请确保App_Data文件夹存在，且有读写权限。
            this.RequestDocument.Save(Path.Combine(logPath, string.Format("{0}_Request_{1}_{2}.txt", _getRandomFileName(),
                this.RequestMessage.FromUserName,
                this.RequestMessage.MsgType)));
            if (this.UsingEcryptMessage && this.EcryptRequestDocument != null)
            {
                this.EcryptRequestDocument.Save(Path.Combine(logPath, string.Format("{0}_Request_Ecrypt_{1}_{2}.txt", _getRandomFileName(),
                    this.RequestMessage.FromUserName,
                    this.RequestMessage.MsgType)));
            }
        }

        /// <summary>
        /// 保存响应信息
        /// </summary>
        /// <param name="logPath">保存日志目录，默认为 ~/App_Data/&lt;模块类型&gt;/<yyyy-MM-dd>/</code></param>
        public void SaveResponseMessageLog(string logPath = null)
        {
            logPath = logPath ?? GetLogPath();
            if (this.ResponseDocument != null && this.ResponseDocument.Root != null)
            {
                this.ResponseDocument.Save(Path.Combine(logPath, string.Format("{0}_Response_{1}_{2}.txt", _getRandomFileName(),
                    this.ResponseMessage.ToUserName,
                    this.ResponseMessage.MsgType)));
            }

            if (this.UsingEcryptMessage &&
                this.FinalResponseDocument != null && this.FinalResponseDocument.Root != null)
            {
                //记录加密后的响应信息
                this.FinalResponseDocument.Save(Path.Combine(logPath, string.Format("{0}_Response_Final_{1}_{2}.txt", _getRandomFileName(),
                    this.ResponseMessage.ToUserName,
                    this.ResponseMessage.MsgType)));
            }
        }

        #endregion

    }
}