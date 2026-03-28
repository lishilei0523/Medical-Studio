using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Caliburn.Micro;
using MedicalSharp.Client.ViewModels.HomeContext;
using MedicalSharp.Dicoms;
using Microsoft.Extensions.DependencyInjection;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.IOC.Core.Mediators;
using SD.IOC.Extension.NetCore;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace MedicalSharp.Client
{
    /// <summary>
    /// Caliburn启动器
    /// </summary>
    public class Startup : BootstrapperBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 默认构造器
        /// </summary>
        public Startup()
        {
            GlobalExceptionAspect.UnhandledTaskException += this.OnUnhandledException;
            Dispatcher.UIThread.UnhandledException += this.OnUnhandledException;
        }

        #endregion

        #region # 事件

        #region 应用程序启动事件 —— override void OnStartup(object sender...
        /// <summary>
        /// 应用程序启动事件
        /// </summary>
        protected override async void OnStartup(object sender, ControlledApplicationLifetimeStartupEventArgs eventArgs)
        {
            await base.DisplayRootViewFor<WireframeViewModel>();
        }
        #endregion

        #region 应用程序异常事件 —— Task OnUnhandledException(MethodBase sender...
        /// <summary>
        /// 应用程序异常事件
        /// </summary>
        protected Task OnUnhandledException(MethodBase sender, Exception exception)
        {
            //TODO 异常处理
            return Task.CompletedTask;
        }
        #endregion

        #region 应用程序异常事件 —— void OnUnhandledException(object sender..
        /// <summary>
        /// 应用程序异常事件
        /// </summary>
        private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs eventArgs)
        {
            eventArgs.Handled = true;
        }
        #endregion

        #region 应用程序退出事件 —— override void OnExit(object sender...
        /// <summary>
        /// 应用程序退出事件
        /// </summary>
        protected override void OnExit(object sender, ControlledApplicationLifetimeExitEventArgs eventArgs)
        {
            ResolveMediator.Dispose();
        }
        #endregion

        #endregion

        #region # 方法

        #region 配置应用程序 —— override void Configure()
        /// <summary>
        /// 配置应用程序
        /// </summary>
        protected override void Configure()
        {
            //初始化依赖注入容器
            if (!ResolveMediator.ContainerBuilt)
            {
                IServiceCollection serviceCollection = ResolveMediator.GetServiceCollection();
                serviceCollection.RegisterConfigs();

                ResolveMediator.Build();
            }

            //初始化SimpleITK
            DicomManager.Initialize();
        }
        #endregion

        #region 解析服务实例 —— override object GetInstance(Type service...
        /// <summary>
        /// 解析服务实例
        /// </summary>
        /// <param name="service">服务类型</param>
        /// <param name="key">键</param>
        /// <returns>服务实例</returns>
        protected override object GetInstance(Type service, string key)
        {
            object instance = ResolveMediator.Resolve(service);

            return instance;
        }
        #endregion

        #region 解析服务实例列表 —— override IEnumerable<object> GetAllInstances(Type service)
        /// <summary>
        /// 解析服务实例列表
        /// </summary>
        /// <param name="service">服务类型</param>
        /// <returns>服务实例列表</returns>
        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            IEnumerable<object> instances = ResolveMediator.ResolveAll(service);

            return instances;
        }
        #endregion

        #endregion
    }
}
