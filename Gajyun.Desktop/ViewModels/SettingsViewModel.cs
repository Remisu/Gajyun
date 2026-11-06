using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gajyun.Core.Services;
using Microsoft.Extensions.Logging;

namespace Gajyun.Desktop.ViewModels;

public partial class SettingsViewModel(IRoomSettingsService service, ILogger<SettingsViewModel> logger) : ObservableObject
{
    public ObservableCollection<RoomSettings> Rooms { get; } = [];
    public ObservableCollection<RoomPolicyRowViewModel> RoomPolicies { get; } = [];
    public ObservableCollection<TaxPolicyOption> Policies { get; } = [];
    public ObservableCollection<RoomAliasSettings> Aliases { get; } = [];
    public string PolicyNotice => "Choose a policy beside each room and click Save. This is an internal preference; legal rates depend on the stay date. Historical calculations stay unchanged, and breakfast-inclusive prices still require a verified taxable base.";

    [ObservableProperty] private RoomSettings? selectedRoom;
    [ObservableProperty] private RoomPolicyRowViewModel? selectedPolicyRoom;
    [ObservableProperty] private TaxPolicyOption? selectedPolicy;
    [ObservableProperty] private string status = "Initialize the database, then refresh room settings.";
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string capacitySummary = "Rooms not loaded";

    private bool CanLoad() => !IsBusy;
    private bool CanSave() => !IsBusy && SelectedRoom is not null && SelectedPolicy is not null && SelectedRoom.DefaultTaxPolicyID != SelectedPolicy.TaxPolicyID;
    private bool CanLoadAliases() => !IsBusy && SelectedRoom is not null;

    partial void OnIsBusyChanged(bool value)
    {
        LoadCommand.NotifyCanExecuteChanged();
        SavePolicyCommand.NotifyCanExecuteChanged();
        LoadAliasesCommand.NotifyCanExecuteChanged();
    }
    partial void OnSelectedRoomChanged(RoomSettings? value)
    {
        Aliases.Clear();
        SelectedPolicy = Policies.FirstOrDefault(x => x.TaxPolicyID == value?.DefaultTaxPolicyID);
        SavePolicyCommand.NotifyCanExecuteChanged();
        LoadAliasesCommand.NotifyCanExecuteChanged();
    }
    partial void OnSelectedPolicyChanged(TaxPolicyOption? value) => SavePolicyCommand.NotifyCanExecuteChanged();
    partial void OnSelectedPolicyRoomChanged(RoomPolicyRowViewModel? value) => SelectedRoom = value?.Room;

    private void ReportRoomPolicy(RoomSettings room, string message)
    {
        var index = Rooms.ToList().FindIndex(x => x.RoomID == room.RoomID);
        if (index >= 0) Rooms[index] = room;
        if (SelectedRoom?.RoomID == room.RoomID) SelectedRoom = room;
        Status = message;
    }

    [RelayCommand(CanExecute = nameof(CanLoad), IncludeCancelCommand = true)]
    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        var selectedID = SelectedRoom?.RoomID;
        Status = "Loading rooms and tax policy catalog...";
        try
        {
            var snapshot = await service.LoadAsync(cancellationToken);
            Rooms.Clear();
            Policies.Clear();
            Aliases.Clear();
            Policies.Add(new(null, "No internal policy selected"));
            foreach (var policy in snapshot.Policies) Policies.Add(policy);
            foreach (var room in snapshot.Rooms) Rooms.Add(room);
            RoomPolicies.Clear();
            foreach (var room in snapshot.Rooms) RoomPolicies.Add(new(room, Policies.ToArray(), service, ReportRoomPolicy));
            SelectedPolicyRoom = RoomPolicies.FirstOrDefault(x => x.Room.RoomID == selectedID) ?? RoomPolicies.FirstOrDefault();
            SelectedRoom = SelectedPolicyRoom?.Room;
            SelectedPolicy = Policies.FirstOrDefault(x => x.TaxPolicyID == SelectedRoom?.DefaultTaxPolicyID);
            CapacitySummary = $"{Rooms.Count} physical rooms; total capacity {Rooms.Sum(x => x.Capacity)} guests. No occupancy enforcement.";
            Status = "Choose each room's policy from its dropdown, then click Save beside that room.";
            if (SelectedRoom is not null) await ReadAliasesAsync(SelectedRoom.RoomID, cancellationToken);
        }
        catch (SettingsOperationException exception) { ShowError(exception); }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { Status = "Settings refresh cancelled."; }
        finally { IsBusy = false; }
    }

    [RelayCommand(CanExecute = nameof(CanLoadAliases), IncludeCancelCommand = true)]
    private async Task LoadAliasesAsync(CancellationToken cancellationToken)
    {
        if (SelectedRoom is null) return;
        var roomID = SelectedRoom.RoomID;
        IsBusy = true;
        try { await ReadAliasesAsync(roomID, cancellationToken); }
        catch (SettingsOperationException exception) { ShowError(exception); }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { Status = "Alias loading cancelled."; }
        finally { IsBusy = false; }
    }

    [RelayCommand(CanExecute = nameof(CanSave), IncludeCancelCommand = true)]
    private async Task SavePolicyAsync(CancellationToken cancellationToken)
    {
        if (SelectedRoom is null || SelectedPolicy is null) return;
        var room = SelectedRoom;
        var policy = SelectedPolicy;
        IsBusy = true;
        try
        {
            await service.SetTaxPolicyAsync(room.RoomID, policy.TaxPolicyID, room.DefaultTaxPolicyID, cancellationToken);
            var updated = room with { DefaultTaxPolicyID = policy.TaxPolicyID };
            var index = Rooms.IndexOf(room);
            Rooms[index] = updated;
            SelectedRoom = updated;
            Status = "Internal room tax policy saved. Historical tax calculations were not modified.";
        }
        catch (SettingsOperationException exception) { ShowError(exception); }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            Status = "Save cancelled. Refresh settings to confirm the persisted policy before retrying.";
        }
        finally { IsBusy = false; }
    }

    private async Task ReadAliasesAsync(int roomID, CancellationToken cancellationToken)
    {
        var aliases = await service.GetAliasesAsync(roomID, cancellationToken);
        if (SelectedRoom?.RoomID != roomID) return;
        Aliases.Clear();
        foreach (var alias in aliases) Aliases.Add(alias);
    }

    private void ShowError(SettingsOperationException exception)
    {
        logger.LogWarning("Room settings request failed: {Message}", exception.Message);
        Status = exception.Message;
    }
}
