using System;

public class ConfirmPopupParameters : UIBaseParameters
{
    public readonly string Title;
    public readonly string Message;
    public readonly string ConfirmText;
    public readonly string CancelText;
    public readonly Action ConfirmAction;
    public readonly Action CancelAction;
    
    public ConfirmPopupParameters(string title, string message, string confirmText, string cancelText, Action confirmAction = null, Action cancelAction = null)
    {
        Title = title;
        Message = message;
        ConfirmText = confirmText;
        CancelText = cancelText;
        ConfirmAction = confirmAction;
        CancelAction = cancelAction;
    }
}