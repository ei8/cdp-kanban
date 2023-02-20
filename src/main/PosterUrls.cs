using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ei8.Cortex.Diary.Plugins.Kanban
{
    public class PosterUrls
    {
        private IConfiguration configuration;

        public PosterUrls(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string InstantiatesTask => this.configuration["PosterUrls:InstantiatesTask"];
        public string HasStatusOfBacklog => this.configuration["PosterUrls:HasStatusOfBacklog"];
        public string HasStatusOfPrioritized => this.configuration["PosterUrls:HasStatusOfPrioritized"];
        public string HasStatusOfInProgress => this.configuration["PosterUrls:HasStatusOfInProgress"];
        public string HasStatusOfDone => this.configuration["PosterUrls:HasStatusOfDone"];

        public bool HasValue
        {
            get => !string.IsNullOrWhiteSpace(this.InstantiatesTask) || !string.IsNullOrWhiteSpace(this.HasStatusOfBacklog)
                || !string.IsNullOrWhiteSpace(this.HasStatusOfPrioritized) || !string.IsNullOrWhiteSpace(this.HasStatusOfInProgress)
                || !string.IsNullOrWhiteSpace(HasStatusOfDone);
        }
    }
}