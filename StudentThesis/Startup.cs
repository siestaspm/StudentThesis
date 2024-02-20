using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(StudentThesis.Startup))]
namespace StudentThesis
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
