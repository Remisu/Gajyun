using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gajyun.Core.Services;

namespace Gajyun.Desktop.ViewModels;

public partial class RoomPolicyRowViewModel : ObservableObject
{
    private readonly IRoomSettingsService service;
    private readonly Action<RoomSettings, string> report;
    public RoomSettings Room { get; private set; }
    public IReadOnlyList<TaxPolicyOption> Policies { get; }
    [ObservableProperty] private TaxPolicyOption? selectedPolicy;
    [ObservableProperty] private bool isSaving;
    [ObservableProperty] private string status = "";

    public RoomPolicyRowViewModel(RoomSettings room, IReadOnlyList<TaxPolicyOption> policies, IRoomSettingsService service, Action<RoomSettings, string> report)
    {
        Room = room; Policies = policies; this.service = service; this.report = report;
        selectedPolicy = policies.FirstOrDefault(x => x.TaxPolicyID == room.DefaultTaxPolicyID);
    }
    private bool CanSave() => !IsSaving && SelectedPolicy is not null && SelectedPolicy.TaxPolicyID != Room.DefaultTaxPolicyID;
    partial void OnSelectedPolicyChanged(TaxPolicyOption? value) { Status = CanSave() ? "Not saved" : ""; SaveCommand.NotifyCanExecuteChanged(); }
    partial void OnIsSavingChanged(bool value) => SaveCommand.NotifyCanExecuteChanged();

    [RelayCommand(CanExecute = nameof(CanSave), IncludeCancelCommand = true)]
    private async Task SaveAsync(CancellationToken ct)
    {
        if (SelectedPolicy is null) return;
        var policy = SelectedPolicy; IsSaving = true;
        try
        {
            await service.SetTaxPolicyAsync(Room.RoomID, policy.TaxPolicyID, Room.DefaultTaxPolicyID, ct);
            Room = Room with { DefaultTaxPolicyID = policy.TaxPolicyID };
            OnPropertyChanged(nameof(Room)); Status = "Saved";
            report(Room, $"{Room.RoomName}: {policy.PolicyName} saved. Historical tax calculations unchanged.");
        }
        catch (SettingsOperationException exception) { Status = exception.Message; report(Room, exception.Message); }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { Status = "Cancelled; refresh to confirm."; report(Room, Status); }
        finally { IsSaving = false; }
    }
}
