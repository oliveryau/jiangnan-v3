using DG.Tweening;
using JN.Client.UI;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanelControllerrData : UIPanelData
{
}

/// <summary>
/// 登录
/// </summary>
public class LoginPanelController : QFrameworkPanel<LoginPanelControllerrData>
{

    [SerializeField] private Button btn_Login;
    [SerializeField] private Button btn_Register;
    [SerializeField] private Slider slider_Loading;
    [SerializeField] private TextMeshProUGUI text_Loading;


    /// <summary>
    /// 面板初始化时绑定控件和事件。
    /// </summary>
    protected override void OnPanelInit()
    {
        if (btn_Login != null)
        {
            btn_Login.onClick.AddListener(OnClickBtnLogin);
        }

        if (btn_Register != null)
        {
            btn_Register.onClick.AddListener(OnClickBtnRegister);
        }

        slider_Loading.onValueChanged.AddListener(OnLoadingValueChange);
        slider_Loading.DOValue(1f, UnityEngine.Random.Range(2, 4)).SetEase(Ease.InOutQuad);
    }

    private void OnLoadingValueChange(float value)
    {
        text_Loading.text = $"{(int)(value * 100)}%";
        if (value >= 1)
        {
            CloseSelf();
            UIKit.OpenPanel<CreatePlayerPanelController>(UILevel.Common);
        }
    }
    /// <summary>
    /// 面板关闭时清理临时状态和监听。
    /// </summary>
    protected override void OnPanelClose()
    {
        if (btn_Login != null)
        {
            btn_Login.onClick.RemoveListener(OnClickBtnLogin);
        }

        if (btn_Register != null)
        {
            btn_Register.onClick.RemoveListener(OnClickBtnRegister);
        }

        slider_Loading.onValueChanged.RemoveAllListeners();
    }

    /// <summary>
    /// 登录
    /// </summary>
    private void OnClickBtnLogin()
    {
        CloseSelf();
        UIKit.OpenPanel<CreatePlayerPanelController>(UILevel.Common);
    }

    /// <summary>
    /// 注册
    /// </summary>
    private void OnClickBtnRegister()
    {
        CloseSelf();
        UIKit.OpenPanel<CreatePlayerPanelController>(UILevel.Common);
    }
}