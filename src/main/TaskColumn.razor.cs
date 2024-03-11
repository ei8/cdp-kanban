using Blazorise;
using ei8.Cortex.Diary.Port.Adapter.UI.ViewModels;
using ei8.Cortex.Library.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using ei8.Cortex.Diary.Port.Adapter.UI.Views.Blazor.Common;
using ei8.Cortex.Diary.Port.Adapter.UI.Views.Common;
using ei8.Cortex.Library.Client;
using ei8.Cortex.Diary.Application.Neurons;
using neurUL.Cortex.Common;

namespace ei8.Cortex.Diary.Plugins.Kanban 
{
    public partial class TaskColumn
    {

        private KnbnPluginSettingsService pluginSettingsService;

        [Parameter]
        public IEnumerable<Neuron> Tasks { get; set; }

        [Parameter]
        public string Title { get; set; }

        private Modal modalRef;

        private Task ShowModal() {
            return modalRef.Show();
        }

        private Task HideModal() {
            return modalRef.Hide();
        }

        private EditorNeuronViewModel editorNeuronViewModel = new EditorNeuronViewModel();
        private Neuron preEditCopy = null;
        private bool sending = false;
        private EditContext editContext;

        protected override void OnInitialized() {
            this.editContext = new EditContext(this.editorNeuronViewModel);
        }

        [Parameter]
        public string AvatarUrl { get; set; } = "http://192.168.100.9:65101/cortex/neurons";

        private EditorTerminalViewModel EditTerminal { get; set; }

        private ContextMenuOption selectedOption;
        [Parameter]
        public ContextMenuOption SelectedOption {
            get => this.selectedOption;
            set {
                if (this.selectedOption != value) {
                    this.selectedOption = value;

                    this.internalSelectedOptionChanged.InvokeAsync(this.selectedOption);
                    this.SelectedOptionChanged.InvokeAsync(this.selectedOption);
                }
            }
        }

        private EventCallback<ContextMenuOption> internalSelectedOptionChanged { get; set; }
        [Parameter]
        public EventCallback<ContextMenuOption> SelectedOptionChanged { get; set; }

        private Neuron selectedNeuron;
        [Parameter]
        public Neuron SelectedNeuron {
            get => this.selectedNeuron;
            set {
                if (this.selectedNeuron != value)
                    this.selectedNeuron = value;
            }
        }

        private EventCallback<NeuronResultItemViewModel> internalSelectedRegionNeuronChanged { get; set; }
        private NeuronResultItemViewModel selectedRegionNeuron;
        private NeuronResultItemViewModel SelectedRegionNeuron {
            get => this.selectedRegionNeuron;
            set {
                if (this.selectedRegionNeuron?.Neuron?.Id != value?.Neuron?.Id) {
                    this.selectedRegionNeuron = value;
                    this.internalSelectedRegionNeuronChanged.InvokeAsync(this.selectedRegionNeuron);
                }
            }
        }

        private Neuron initialRegionNeuron;
        [Parameter]
        public Neuron InitialRegionNeuron {
            get => this.initialRegionNeuron;
            set {
                if (this.initialRegionNeuron != value) {
                    this.initialRegionNeuron = value;

                    if (this.initialRegionNeuron != null) {
                        this.editorNeuronViewModel.InitialRegionId = this.initialRegionNeuron.Id;
                        this.editorNeuronViewModel.InitialRegionTag = this.initialRegionNeuron.Tag;
                    }
                    else
                        this.editorNeuronViewModel.ClearInitialRegion();
                }
            }
        }

        private IEnumerable<Neuron> initialPostsynapticNeurons;
        [Parameter]
        public IEnumerable<Neuron> InitialPostsynapticNeurons {
            get => this.initialPostsynapticNeurons;
            set {
                if (this.initialPostsynapticNeurons != value) {
                    this.initialPostsynapticNeurons = value;

                    if (this.initialPostsynapticNeurons != null)
                        this.editorNeuronViewModel.InitialPostsynaptics = this.initialPostsynapticNeurons;
                    else
                        this.editorNeuronViewModel.InitializePostsynaptic();
                }
            }
        }

        private bool IsSearchRegionNeuronVisible { get; set; } = false;

        private bool isTerminalEditorVisible = false;
        private bool IsTerminalEditorVisible {
            get => this.isTerminalEditorVisible;
            set {
                if (this.isTerminalEditorVisible != value) {
                    this.isTerminalEditorVisible = value;

                    if (!this.isTerminalEditorVisible)
                        this.EditTerminal = null;
                }
            }
        }

        private async Task KeyPress(KeyboardEventArgs e) {
            if ((e.Code == "Enter" || e.Code == "NumpadEnter") && e.CtrlKey) {
                await this.FormSubmitted(this.editContext);
            }
        }

        private async Task FormSubmitted(EditContext editContext) {
            bool formValid = editContext.Validate();
            if (formValid) {
                await this.ProcessSend(
                    async (rq, pi) => {
                        bool result = false;
                        switch (this.SelectedOption) {
                            case ContextMenuOption.New:
                                pi.Description = "Neuron creation";
                                this.IsConfirmCreateOwnerVisible =
                                    (await this.notificationApplicationService.GetNotificationLog(rq.AvatarUrl, string.Empty))
                                    .NotificationList.Count == 0;

                                if (!this.IsConfirmCreateOwnerVisible)
                                    result = await CreateNeuron(
                                        this.editorNeuronViewModel,
                                        this.neuronQueryService,
                                        this.neuronApplicationService,
                                        this.terminalApplicationService,
                                        rq,
                                        pi,
                                        () => this.IsConfirmCreateSimilarVisible = true
                                    );
                                else
                                    pi.Suspend = true;

                                await HideModal();

                                break;
                        }
                        return result;
                    },
                    () => Helper.ReinitializeOption(o => this.SelectedOption = o)
                );
            }
        }

        private async  Task<bool> CreateNeuron(EditorNeuronViewModel editorNeuronViewModel, INeuronQueryService neuronQueryService, INeuronApplicationService neuronApplicationService, ITerminalApplicationService terminalApplicationService, QueryUrl rq, ProcessInfo pi, Action hasSimilarHandler) {
            bool result = false;
            var items = (await neuronQueryService.GetNeurons(
                rq.AvatarUrl,
                neuronQuery: new NeuronQuery() {
                    TagContains = new string[] { editorNeuronViewModel.Tag },
                    TagContainsIgnoreWhitespace = true
                })
            ).Items;

            if (!items.Any()) {
                await CreateNeuronCore(neuronApplicationService, rq, editorNeuronViewModel, terminalApplicationService);
                result = true;
            }
            else {
                if (hasSimilarHandler != null)
                    hasSimilarHandler.Invoke();
                pi.Suspend = true;
            }

            return result;
        }

        private async  Task CreateNeuronCore(INeuronApplicationService neuronApplicationService, QueryUrl rq, EditorNeuronViewModel editorNeuronViewModel, ITerminalApplicationService terminalApplicationService) {
            var newNeuronId = Guid.NewGuid().ToString();

            await neuronApplicationService.CreateNeuron(
                rq.AvatarUrl,
                newNeuronId,
                editorNeuronViewModel.Tag,
                editorNeuronViewModel.RegionId,
                editorNeuronViewModel.NeuronExternalReferenceUrl
                );

            editorNeuronViewModel.Terminals.ToList().ForEach(async e =>
                await terminalApplicationService.CreateTerminalFromViewModel(e, newNeuronId, rq.AvatarUrl)
            );


            var HasStatusOfBacklogNeuron = await NeuronQueryService.GetNeurons(
                                    rq.AvatarUrl,
                                    new NeuronQuery() {
                                        ExternalReferenceUrl = new string[]
                                        {
                                            pluginSettingsService.PosterUrls.HasStatusOfBacklog
                                        }
                                    });

            //Creating Backlog Terminal
            await terminalApplicationService.CreateTerminal(
                rq.AvatarUrl,
                Guid.NewGuid().ToString(),
                newNeuronId,
                HasStatusOfBacklogNeuron.Items.FirstOrDefault().Id,
                NeurotransmitterEffect.Excite,
                1f,
                ""
            );

            var HasStatusOfInstantiatesTaskNeuron = await NeuronQueryService.GetNeurons(
                                    rq.AvatarUrl,
                                    new NeuronQuery() {
                                        ExternalReferenceUrl = new string[]
                                        {
                                            pluginSettingsService.PosterUrls.InstantiatesTask
                                        }
                                    });

            //Creating InstantiatesTask Terminal
            await terminalApplicationService.CreateTerminal(
                rq.AvatarUrl,
                Guid.NewGuid().ToString(),
                newNeuronId,
                HasStatusOfInstantiatesTaskNeuron.Items.FirstOrDefault().Id,
                NeurotransmitterEffect.Excite,
                1f,
                ""
            );
        }

        private async Task<bool> ProcessSend(Func<QueryUrl, ProcessInfo, Task<bool>> processCore, Action postProcess) {
            bool result = false;
            if (QueryUrl.TryParse(this.AvatarUrl, out QueryUrl resultQuery) && !this.sending) {
                var processInfo = new ProcessInfo();

                await this.toastService.UITryHandler(
                    async () => {
                        this.sending = true;
                        result = (await processCore.Invoke(resultQuery, processInfo));

                        return !processInfo.Suspend && result;
                    },
                    () => processInfo.Description,
                    postActionInvoker: () => this.sending = false
                );

                if (!processInfo.Suspend && postProcess != null)
                    postProcess.Invoke();
            }

            return result;
        }

        private bool IsConfirmCreateSimilarVisible { get; set; } = false;

        private bool IsConfirmCreateOwnerVisible { get; set; } = false;



        public string avatarUrl { get; set; } = "http://192.168.100.9:65101/cortex/neurons";
        public string Id  { get; set; } = Guid.NewGuid().ToString();
        public string presynapticNeuronId { get; set; }
        public string postsynapticNeuronId { get; set; }
        public NeurotransmitterEffect effect { get; set; } = NeurotransmitterEffect.Excite;
        public float strength { get; set; } = 1f;
        public string TerminalExternalReferenceUrl { get; set; } = "";



        [Parameter]
        public IPluginSettingsService PluginSettingsService { get => this.pluginSettingsService; set { this.pluginSettingsService = (KnbnPluginSettingsService)value; } }

        [Parameter]
        public INeuronQueryService NeuronQueryService { get; set; }

    }
}
