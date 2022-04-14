using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Prism.Commands;
using PropertyChanged.SourceGenerator;
using WDE.Common.Parameters;
using WDE.Common.Services.FindAnywhere;
using WDE.Common.Utils;
using WDE.Module.Attributes;
using WDE.MVVM;

namespace WoWDatabaseEditorCore.Services.FindAnywhere;

[AutoRegister]
public partial class FindAnywhereDialogViewModel : ObservableBase, IFindAnywhereDialogViewModel
{
    public int DesiredWidth => 400;
    public int DesiredHeight => 220;
    public string Title => "Find anywhere";
    public bool Resizeable => true;
    public ICommand Accept { get; set; }
    public ICommand Cancel { get; set; }
    public event Action? CloseCancel;
    public event Action? CloseOk;

    public ICommand FindCommand { get; }
    public ICommand PickValue { get; }
    [Notify] private string searchText = "";
    [Notify] private FindSourceDialog selectedSource;
    public ObservableCollection<FindSourceDialog> Sources { get; } = new();

    public FindAnywhereDialogViewModel(IParameterPickerService pickerService,
        IParameterFactory parameterFactory,
        IFindAnywhereService findAnywhereService)
    {
        Sources.Add(new FindSourceDialog("Creature entry", "CreatureParameter", "CreatureGameobjectParameter", "ConditionObjectEntryParameter"));
        Sources.Add(new FindSourceDialog("Game object entry", "GameobjectParameter", "CreatureGameobjectParameter", "ConditionObjectEntryParameter"));
        Sources.Add(new FindSourceDialog("Spell", "SpellParameter", "SpellAreaSpellParameter", "SpellOrRankedSpellParameter", "MultiSpellParameter"));
        Sources.Add(new FindSourceDialog("Quest", "QuestParameter"));
        Sources.Add(new FindSourceDialog("Faction", "FactionParameter"));
        Sources.Add(new FindSourceDialog("Map", "MapParameter"));
        Sources.Add(new FindSourceDialog("Zone/area", "ZoneAreaParameter"));
        Sources.Add(new FindSourceDialog("Emote", "EmoteParameter"));
        Sources.Add(new FindSourceDialog("Item", "ItemParameter"));
        Sources.Add(new FindSourceDialog("Sound", "SoundParameter"));
        Sources.Add(new FindSourceDialog("Achievement", "AchievementParameter"));
        Sources.Add(new FindSourceDialog("Skill", "SkillParameter"));
        Sources.Add(new FindSourceDialog("Event", "EventScriptParameter"));
        Sources.Add(new FindSourceDialog("Timed action list", "TimedActionListParameter"));
        
        selectedSource = Sources[0];
        
        Cancel = new DelegateCommand(() => CloseCancel?.Invoke());
        PickValue = new AsyncAutoCommand(async () =>
        {
            var parameter = parameterFactory.Factory(selectedSource.Parameters[0]);
            long searchValue = 0;
            long.TryParse(searchText, out searchValue);
            var result = await pickerService.PickParameter(parameter, searchValue);
            if (result.ok)
                SearchText = result.value.ToString();
        });
        FindCommand = new DelegateCommand(() =>
        {
            long searchValue = 0;
            long.TryParse(searchText, out searchValue);
            findAnywhereService.OpenFind(selectedSource.Parameters, searchValue);
            CloseOk?.Invoke();
        }, () => long.TryParse(searchText, out var val) && val != 0).ObservesProperty(() => SearchText);
        Accept = new DelegateCommand(() =>
        {
            if (FindCommand.CanExecute(null))
                FindCommand?.Execute(null); 
        });
    }
}