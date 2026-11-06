using CommunityToolkit.Mvvm.ComponentModel;
using Gajyun.Core.Csv;

namespace Gajyun.Desktop.ViewModels;

public partial class ImportReviewRowViewModel : ObservableObject
{
    private readonly Action changed;
    public ImportReviewRow Row { get; }
    public IReadOnlyList<CustomerMatchOption> CustomerMatches => Row.CustomerMatches;
    public IReadOnlyList<ImportRoomOption> RoomChoices => Row.RoomChoices;
    public bool HasCustomerQuestion => CustomerMatches.Count > 1;
    public bool HasRoomQuestion => RoomChoices.Count > 0;
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
    public bool IsUnresolved => HasCustomerQuestion && SelectedCustomer is null &&
        (Row.Disposition is ImportRowDisposition.Ready or ImportRowDisposition.Cancelled || SelectedRoom is not null) ||
        HasRoomQuestion && SelectedRoom is null && Row.Disposition is ImportRowDisposition.Changed or ImportRowDisposition.CancellationUpdate;
    public ImportRowDecision Decision => new(Row.RowNumber, SelectedCustomer?.CustomerID, SelectedCustomer?.SourceRowNumber, SelectedRoom?.RoomID, SelectedCustomer is not null);
    [ObservableProperty] private CustomerMatchOption? selectedCustomer;
    [ObservableProperty] private ImportRoomOption? selectedRoom;

    public ImportReviewRowViewModel(ImportReviewRow row, Action changed)
    {
        Row = row;
        this.changed = changed;
        if (CustomerMatches.Count == 1) selectedCustomer = CustomerMatches[0];
    }
    partial void OnSelectedCustomerChanged(CustomerMatchOption? value) => changed();
    partial void OnSelectedRoomChanged(ImportRoomOption? value) => changed();
}
