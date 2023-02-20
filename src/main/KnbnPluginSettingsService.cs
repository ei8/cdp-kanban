using Microsoft.Extensions.Configuration;
using ei8.Cortex.Diary.Port.Adapter.UI.Views.Blazor.Common;

namespace ei8.Cortex.Diary.Plugins.Kanban
{
    public class KnbnPluginSettingsService : IPluginSettingsService
    {
        private const int DefaultUpdateCheckInterval = 2000;

        public int UpdateCheckInterval => int.TryParse(this.Configuration["UpdateCheckInterval"], out int uci) ? uci : KnbnPluginSettingsService.DefaultUpdateCheckInterval;

        private IConfiguration configuration;
        public IConfiguration Configuration
        {
            get => this.configuration;
            set
            {
                this.configuration = value;

                if (this.configuration != null)
                {
                    this.PosterUrls = new PosterUrls(this.configuration);
                }
            }
        }
        public PosterUrls PosterUrls { get; set; }
    }
}
