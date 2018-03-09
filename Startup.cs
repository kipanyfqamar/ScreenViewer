using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ScreenViewer.Startup))]
namespace ScreenViewer
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //ConfigureAuth(app);
        }
    }
}