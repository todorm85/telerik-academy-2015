﻿namespace HikingPlanAndRescue.Web
{
    using System.Data.Entity;
    using System.Reflection;
    using System.Web.Http;
    using System.Web.Mvc;

    using Autofac;
    using Autofac.Integration.Mvc;
    using Autofac.Integration.WebApi;
    using Controllers;

    using Data;
    using Data.Common;

    using Services.Data;
    using Services.Data.Contracts;
    using Services.Logic;
    using Services.Logic.Contracts;
    using Services.Web;

    public static class AutofacConfig
    {
        public static void RegisterAutofac()
        {
            var builder = new ContainerBuilder();
            var config = GlobalConfiguration.Configuration;

            // Register API controllers
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            // Register your MVC controllers.
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            // OPTIONAL: Register model binders that require DI.
            builder.RegisterModelBinders(Assembly.GetExecutingAssembly());
            builder.RegisterModelBinderProvider();

            // OPTIONAL: Register web abstractions like HttpContextBase.
            builder.RegisterModule<AutofacWebTypesModule>();

            // OPTIONAL: Enable property injection in view pages.
            builder.RegisterSource(new ViewRegistrationSource());

            // OPTIONAL: Enable property injection into action filters.
            builder.RegisterFilterProvider();

            // Register services
            RegisterServices(builder);

            // Set the dependency resolver to be Autofac.
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }

        private static void RegisterServices(ContainerBuilder builder)
        {
            builder.Register(x => new ApplicationDbContext())
                .As<DbContext>()
                .InstancePerRequest();
            builder.Register(x => new HttpCacheService())
                .As<ICacheService>()
                .InstancePerRequest();

            //builder.Register(x => new IdentifierProvider())
            //    .As<IIdentifierProvider>()
            //    .InstancePerRequest();

            var dataServicesAssembly = Assembly.GetAssembly(typeof(ITrainingsService));
            builder.RegisterAssemblyTypes(dataServicesAssembly).AsImplementedInterfaces();

            var predictionsServiceAssembly = Assembly.GetAssembly(typeof(ITrainingPrediction));
            builder.RegisterAssemblyTypes(predictionsServiceAssembly).AsImplementedInterfaces();

            builder.RegisterGeneric(typeof(DbRepository<>))
                .As(typeof(IDbRepository<>))
                .InstancePerRequest();

            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AssignableTo<BaseController>().PropertiesAutowired();
        }
    }
}