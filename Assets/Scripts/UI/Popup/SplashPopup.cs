using TMPro;

public class SplashPopup : PopupBase
{
    public TextMeshProUGUI message;

    public override void Open(UIBaseParameters parameters = null)
    {
        // base.Open(parameters);
        // if (parameters is SplashPopupParameters splashPopupParameters)
        // {
        //     message.text = splashPopupParameters.message;
        // }

        CoroutineDispatcher.Invoke(Close, 2f);
    }
}