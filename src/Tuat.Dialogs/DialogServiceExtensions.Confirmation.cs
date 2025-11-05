using Tuat.Dialogs;
using Radzen;

public static partial class DialogServiceExtensions
{
    public async static Task<ConfirmationDialog.DialogButton> ShowYesCancelConfirmationDialogAsync(
        this DialogService service,
        string dialogTitle,
        string content)
        => await ShowConfirmDialogAsync(service, dialogTitle, content, ConfirmationDialog.DialogButton.YesCancel); 

    public async static Task<ConfirmationDialog.DialogButton> ShowNoYesCancelConfirmationDialogAsync(
        this DialogService service,
        string dialogTitle,
        string content)
        => await ShowConfirmDialogAsync(service, dialogTitle, content, ConfirmationDialog.DialogButton.NoYesCancel);

    public async static Task<ConfirmationDialog.DialogButton> ShowNoYesConfirmationDialogAsync(
        this DialogService service,
        string dialogTitle,
        string content)
        => await ShowConfirmDialogAsync(service, dialogTitle, content, ConfirmationDialog.DialogButton.NoYes);
    
    public async static Task<ConfirmationDialog.DialogButton> ShowYesConfirmationDialogAsync(
        this DialogService service,
        string dialogTitle,
        string content)
        => await ShowConfirmDialogAsync(service, dialogTitle, content, ConfirmationDialog.DialogButton.Yes);
    
    public async static Task<ConfirmationDialog.DialogButton> ShowOkCancelConfirmationDialogAsync(
        this DialogService service,
        string dialogTitle,
        string content)
        => await ShowConfirmDialogAsync(service, dialogTitle, content, ConfirmationDialog.DialogButton.OkCancel);
    
    private async static Task<ConfirmationDialog.DialogButton> ShowConfirmDialogAsync(
        DialogService service,
        string dialogTitle,
        string content,
        ConfirmationDialog.DialogButton dialogButton)
    {
        void SetParameters(ConfirmationDialog dialog)
        {
#pragma warning disable BL0005 // Component parameter should not be set outside of its component.
            dialog.Title = dialogTitle;
            dialog.Content = content;
            dialog.Buttons = dialogButton;
#pragma warning restore BL0005 // Component parameter should not be set outside of its component.
        }

        return await service.ShowDialogAsync<ConfirmationDialog, ConfirmationDialog.DialogButton>(dialogTitle, SetParameters);
    }
}
