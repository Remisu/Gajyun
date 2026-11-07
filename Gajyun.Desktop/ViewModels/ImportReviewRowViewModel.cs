using CommunityToolkit.Mvvm.ComponentModel;
using Gajyun.Core.Csv;

namespace Gajyun.Desktop.ViewModels;

public partial class ImportReviewRowViewModel : ObservableObject
{
    private readonly Action changed;
    public ImportReviewRow Row { get; }
    public IReadOnlyList<ImportRoomOption> RoomChoices => Row.RoomChoices;
    public bool HasRoomQuestion => RoomChoices.Count > 0;
    public string ExistingHeading => Row.HasExistingReservation ? "Existing reservation (database)" : "No existing reservation";
    public string Outcome => Row.Disposition switch
    {
        ImportRowDisposition.Ready => "New booking",
        ImportRowDisposition.Cancelled => "New cancelled booking",
        ImportRowDisposition.Changed => "Changed",
        ImportRowDisposition.CancellationUpdate => "Was active → cancelled",
        ImportRowDisposition.Duplicate => "Unchanged",
        ImportRowDisposition.Held => "Needs review",
        _ => "Invalid"
    };
    public bool IsUnresolved => HasRoomQuestion && SelectedRoom is null &&
        Row.Disposition is ImportRowDisposition.Changed or ImportRowDisposition.CancellationUpdate;
    public ImportRowDecision Decision => new(Row.RowNumber, SelectedRoom?.RoomID);
    [ObservableProperty] private ImportRoomOption? selectedRoom;

    public ImportReviewRowViewModel(ImportReviewRow row, Action changed)
    {
        Row = row;
        this.changed = changed;
    }
    partial void OnSelectedRoomChanged(ImportRoomOption? value) => changed();
}
