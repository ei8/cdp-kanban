using ei8.Cortex.Diary.Application.Neurons;
using ei8.Cortex.Diary.Port.Adapter.UI.ViewModels;
using ei8.Cortex.Library.Client;
using ei8.Cortex.Library.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ei8.Cortex.Diary.Plugins.Kanban
{
    public class TaskNeuronViewModel
    {
        private string avatarUrl;
        private INeuronQueryService neuronQueryService;

        public TaskNeuronViewModel(Neuron neuron, string avatarUrl, INeuronQueryService neuronQueryService)
        {
            this.Neuron = neuron;
            this.avatarUrl = avatarUrl;
            this.neuronQueryService = neuronQueryService;
            this.Children = new List<TaskNeuronViewModel>();
        }

        public IList<TaskNeuronViewModel> Children { get; set; }

        public Neuron Neuron { get; private set; }

        public ExpansionState ExpansionState { get; private set; }

        public async Task Toggle()
        {
            this.ExpansionState = this.ExpansionState == ExpansionState.Collapsed ? ExpansionState.Expanding : ExpansionState.Collapsed;

            if (this.ExpansionState == ExpansionState.Expanding)
            {
                var children = new List<TaskNeuronViewModel>();
                if (Library.Client.QueryUrl.TryParse(this.avatarUrl, out QueryUrl result))
                {
                    (await this.neuronQueryService.GetNeurons(result.AvatarUrl, this.Neuron.Id, new NeuronQuery() { PageSize = Constants.TreeNodeChildrenQueryPageSize }))
                        .Items
                        .ToList().ForEach(n =>
                        children.Add(new TaskNeuronViewModel(new Neuron(n), this.avatarUrl, this.neuronQueryService))
                    );
                    this.Children = children.ToArray();
                }
                this.ExpansionState = ExpansionState.Expanded;
            }
        }
    }
}
