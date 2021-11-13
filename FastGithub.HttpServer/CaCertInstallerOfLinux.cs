﻿using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FastGithub.HttpServer
{
    abstract class CaCertInstallerOfLinux : ICaCertInstaller
    {
        private readonly ILogger logger;

        /// <summary>
        /// 更新工具文件名
        /// </summary>
        protected abstract string CertToolName { get; }

        /// <summary>
        /// 证书根目录
        /// </summary>
        protected abstract string CertStorePath { get; }


        public CaCertInstallerOfLinux(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// 是否支持
        /// </summary>
        /// <returns></returns>
        public bool IsSupported()
        {
            return OperatingSystem.IsLinux() && File.Exists(this.CertToolName);
        }

        /// <summary>
        /// 安装ca证书
        /// </summary>
        /// <param name="caCertFilePath">证书文件路径</param>
        public void Install(string caCertFilePath)
        {
            var destCertFilePath = Path.Combine(this.CertStorePath, "fastgithub.crt");
            if (File.Exists(destCertFilePath) && File.ReadAllBytes(caCertFilePath).SequenceEqual(File.ReadAllBytes(destCertFilePath)))
            {
                return;
            }

            if (Environment.UserName != "root")
            {
                this.logger.LogWarning($"无法自动安装CA证书{caCertFilePath}，因为没有root权限");
                return;
            }

            try
            {
                Directory.CreateDirectory(this.CertStorePath);
                File.Copy(caCertFilePath, destCertFilePath, overwrite: true);
                Process.Start(this.CertToolName).WaitForExit();
            }
            catch (Exception ex)
            {
                File.Delete(destCertFilePath);
                this.logger.LogWarning(ex.Message, "自动安装证书异常");
            }
        }
    }
}