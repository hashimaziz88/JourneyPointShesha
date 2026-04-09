using System.Reflection;
using System.Threading.Tasks;
using Abp.AspNetCore;
using Abp.AspNetCore.Configuration;
using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Intent.RoslynWeaver.Attributes;
using Hashim.JourneyPoint.Domain;
using Shesha;
using Shesha.Modules;
using Shesha.Startup;
using Shesha.Web.FormsDesigner;

[assembly: IntentTemplate("Boxfusion.Modules.Application.Services.AppService", Version = "1.0")]
[assembly: DefaultIntentManaged(Mode.Fully)]

namespace Hashim.JourneyPoint.Application
{
    [IntentManaged(Mode.Ignore)]
    /// <summary>
    /// JourneyPoint Module
    /// </summary>
    [DependsOn(
        typeof(JourneyPointModule),
        typeof(SheshaCoreModule),
        typeof(AbpAspNetCoreModule)
    )]
    public class JourneyPointApplicationModule : SheshaSubModule<JourneyPointModule>
    {
        public override async Task<bool> InitializeConfigurationAsync()
        {
            // Import any configuration embeded as resources in this assembly on application start-up.
            return await ImportConfigurationAsync();
        }

        /// inheritedDoc
        public override void Initialize()
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddMaps(thisAssembly)
            );
        }

        /// inheritedDoc
        public override void PreInitialize()
        {
            base.PreInitialize();

            Configuration.Modules.AbpAspNetCore()
                .CreateControllersForAppServices(
                    typeof(SheshaCoreModule).GetAssembly()
                );

            Configuration.Modules.AbpAspNetCore()
                 .CreateControllersForAppServices(
                     typeof(SheshaApplicationModule).GetAssembly()
                 );

            Configuration.Modules.AbpAspNetCore()
                 .CreateControllersForAppServices(
                     typeof(SheshaFormsDesignerModule).GetAssembly()
                 );

            Configuration.Modules.AbpAspNetCore()
                 .CreateControllersForAppServices(
                     typeof(SheshaFrameworkModule).GetAssembly()
                 );

            Configuration.Modules.AbpAspNetCore().CreateControllersForAppServices(
               typeof(JourneyPointApplicationModule).Assembly,
               moduleName: "JourneyPoint",
                useConventionalHttpVerbs: true);

            Configuration.Modules.AbpAspNetCore()
                 .CreateControllersForAppServices(
                     typeof(JourneyPointModule).GetAssembly()
                 );

            Configuration.Modules.AbpAspNetCore()
                 .CreateControllersForAppServices(
                     typeof(JourneyPointApplicationModule).GetAssembly()
                 );
        }
    }
}
