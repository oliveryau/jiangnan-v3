using DG.Tweening;
using JN.Client.Config;
using JN.Client.Manager;
using JN.Client.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JN.Client.UI
{
    /// <summary>
    /// 菜单切换弹窗数据。
    /// </summary>
    public class MenuSwitchPanelControllerData : QFramework.UIPanelData
    {
    }

    /// <summary>
    /// 酒楼菜单切换：当前 Menu 下 img_menu1~3 从左到右依次翻牌，切到另一套菜单。
    /// </summary>
    public class MenuSwitchPanelController : OverlayPanelController<MenuSwitchPanelControllerData>
    {
        private const float FlipHalfDuration = 0.18f;
        private const float FlipStagger = 0.12f;
        private const float CloseAfterFlipDelay = 0.5f;
        private static readonly Color SwitchCooldownTint = new Color32(0x64, 0x64, 0x64, 0xFF);
        private static readonly string[] MenuImageNames = { "img_menu1", "img_menu2", "img_menu3" };

        private RectTransform rootRect;
        private GameObject menu1Root;
        private GameObject menu2Root;
        private readonly RectTransform[] menu1Images = new RectTransform[3];
        private readonly RectTransform[] menu2Images = new RectTransform[3];
        private Button closeButton;
        private Button switchButton;
        private TMP_Text menuEffectText;
        private Tween flipTween;
        private bool flipping;
        private bool pendingToVipMenu;
        private bool pendingSwitchTips;
        private float nextSwitchVisualRefreshUnscaledTime;

        protected override void OnPanelInit()
        {
            EnsureNodes();
            EnsureClickBlocker();
            BindPanelButtons();
        }

        protected override void OnPanelOpen(MenuSwitchPanelControllerData data)
        {
            EnsureNodes();
            EnsureClickBlocker();
            BindPanelButtons();
            ResetFlipPose();
            ApplyMenuVisuals(DataManager.Instance != null && DataManager.Instance.IsVipMenuSelected());
            RefreshSwitchVisual();
        }

        protected override void OnPanelShow()
        {
            EnsureNodes();
            if (!flipping)
            {
                ApplyMenuVisuals(DataManager.Instance != null && DataManager.Instance.IsVipMenuSelected());
            }

            RefreshSwitchVisual();
        }

        protected override void OnPanelClose()
        {
            var wasFlipping = flipping;
            var showSwitchTips = pendingSwitchTips;
            var switchedToVipMenu = pendingToVipMenu;
            KillFlip();
            pendingSwitchTips = false;
            ResetFlipPose();
            if (wasFlipping)
            {
                ApplyMenuChange(pendingToVipMenu);
            }
            else
            {
                ApplyMenuVisuals(DataManager.Instance != null && DataManager.Instance.IsVipMenuSelected());
            }

            RefreshSwitchVisual();
            TavernTopStatusPanelController.RefreshOpenedMenuStatusUi();
            if (showSwitchTips)
            {
                HudOverlayService.ShowSwitchMenuTips(switchedToVipMenu);
            }
        }

        private void OnDestroy()
        {
            KillFlip();
        }

        private void EnsureNodes()
        {
            var root = ResolveTransform("Root", "Root");
            rootRect = root as RectTransform ?? root?.GetComponent<RectTransform>();
            menu1Root ??= ResolveNode("Root/Menu1", "Menu1");
            menu2Root ??= ResolveNode("Root/Menu2", "Menu2");
            closeButton ??= ResolveButton("Root/btn_Close", "btn_Close");
            switchButton ??= ResolveButton("Root/btn_Switch", "btn_Switch");
            menuEffectText ??= ResolveText("Root/txt_effect", "txt_effect");
            CacheMenuCardImages(menu1Root, menu1Images);
            CacheMenuCardImages(menu2Root, menu2Images);
        }

        private static void CacheMenuCardImages(GameObject menuRoot, RectTransform[] images)
        {
            if (menuRoot == null || images == null)
            {
                return;
            }

            var parent = menuRoot.transform;
            for (var index = 0; index < MenuImageNames.Length && index < images.Length; index++)
            {
                if (images[index] != null)
                {
                    continue;
                }

                var child = parent.Find(MenuImageNames[index]);
                images[index] = child as RectTransform ?? child?.GetComponent<RectTransform>();
            }
        }

        /// <summary>
        /// 全屏透明遮罩，避免点穿到场景。
        /// </summary>
        private void EnsureClickBlocker()
        {
            var image = GetComponent<Image>();
            if (image == null)
            {
                image = gameObject.AddComponent<Image>();
            }

            image.color = new Color(0f, 0f, 0f, 0.01f);
            image.raycastTarget = true;
        }

        private void BindPanelButtons()
        {
            BindButton(closeButton, CloseSelf);
            BindButton(switchButton, OnClickSwitch);
        }

        private void Update()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (Time.unscaledTime < nextSwitchVisualRefreshUnscaledTime)
            {
                return;
            }

            nextSwitchVisualRefreshUnscaledTime = Time.unscaledTime + 0.25f;
            RefreshSwitchVisual();
        }

        private void OnClickSwitch()
        {
            if (DataManager.Instance == null)
            {
                return;
            }

            if (!DataManager.Instance.IsMenuSwitchCooldownReady())
            {
                HudOverlayService.ShowFloatingWarning("冷却中");
                return;
            }

            if (flipping)
            {
                return;
            }

            pendingToVipMenu = !DataManager.Instance.IsVipMenuSelected();
            ApplyMenuChange(pendingToVipMenu);
            DataManager.Instance.StartMenuSwitchCooldown();
            RefreshSwitchVisual();
            PlayFlipTo(pendingToVipMenu);
        }

        /// <summary>
        /// 当前菜单三张图从左到右绕 Y 翻到侧面后换成目标菜单对应图，再翻回正面。
        /// </summary>
        private void PlayFlipTo(bool toVipMenu)
        {
            var fromMenu = toVipMenu ? menu1Root : menu2Root;
            var toMenu = toVipMenu ? menu2Root : menu1Root;
            var fromImages = toVipMenu ? menu1Images : menu2Images;
            var toImages = toVipMenu ? menu2Images : menu1Images;
            if (fromMenu == null || toMenu == null || !HasAnyCard(fromImages) || !HasAnyCard(toImages))
            {
                ApplyMenuChange(toVipMenu);
                ScheduleCloseAfterFlip();
                return;
            }

            KillFlip();
            flipping = true;
            ResetFlipPose();

            fromMenu.SetActive(true);
            toMenu.SetActive(true);
            SetMenuChromeVisible(fromMenu, true);
            SetMenuChromeVisible(toMenu, false);
            PrepareIncomingCards(toImages);

            var sequence = DOTween.Sequence().SetUpdate(true);
            for (var index = 0; index < MenuImageNames.Length; index++)
            {
                var delay = index * FlipStagger;
                sequence.InsertCallback(delay, GameAudioManager.PlayMenuPlaqueFlip);
                sequence.Insert(
                    delay,
                    CreateCardFlip(
                        fromImages[index],
                        toImages[index],
                        fromMenu,
                        toMenu,
                        swapChrome: index == 0,
                        onChromeSwapped: index == 0 ? () => RefreshMenuEffectText(toVipMenu) : null));
            }

            sequence.AppendCallback(() =>
            {
                ApplyMenuChange(toVipMenu);
                ResetFlipPose();
                RefreshSwitchVisual();
            });
            sequence.AppendInterval(CloseAfterFlipDelay);
            flipTween = sequence.OnComplete(() =>
            {
                pendingSwitchTips = true;
                flipping = false;
                CloseSelf();
            });
        }

        /// <summary>
        /// 无翻牌动画时：应用菜单后延迟关闭。
        /// </summary>
        private void ScheduleCloseAfterFlip()
        {
            KillFlip();
            flipping = true;
            flipTween = DOTween.Sequence()
                .SetUpdate(true)
                .AppendInterval(CloseAfterFlipDelay)
                .OnComplete(() =>
                {
                    pendingSwitchTips = true;
                    flipping = false;
                    CloseSelf();
                });
        }

        private static Tween CreateCardFlip(
            RectTransform fromImg,
            RectTransform toImg,
            GameObject fromMenu,
            GameObject toMenu,
            bool swapChrome,
            System.Action onChromeSwapped = null)
        {
            var card = DOTween.Sequence();
            if (fromImg != null)
            {
                fromImg.localEulerAngles = Vector3.zero;
                card.Append(fromImg.DOLocalRotate(new Vector3(0f, 90f, 0f), FlipHalfDuration).SetEase(Ease.InQuad));
            }

            card.AppendCallback(() =>
            {
                if (fromImg != null)
                {
                    fromImg.gameObject.SetActive(false);
                }

                if (toImg != null)
                {
                    toImg.gameObject.SetActive(true);
                    toImg.localEulerAngles = new Vector3(0f, -90f, 0f);
                }

                if (swapChrome)
                {
                    SetMenuChromeVisible(fromMenu, false);
                    SetMenuChromeVisible(toMenu, true);
                    onChromeSwapped?.Invoke();
                }
            });

            if (toImg != null)
            {
                card.Append(toImg.DOLocalRotate(Vector3.zero, FlipHalfDuration).SetEase(Ease.OutQuad));
            }

            return card;
        }

        private static void PrepareIncomingCards(RectTransform[] images)
        {
            if (images == null)
            {
                return;
            }

            for (var index = 0; index < images.Length; index++)
            {
                var img = images[index];
                if (img == null)
                {
                    continue;
                }

                img.localEulerAngles = Vector3.zero;
                img.gameObject.SetActive(false);
            }
        }

        private static bool HasAnyCard(RectTransform[] images)
        {
            if (images == null)
            {
                return false;
            }

            for (var index = 0; index < images.Length; index++)
            {
                if (images[index] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static void SetMenuChromeVisible(GameObject menuRoot, bool visible)
        {
            if (menuRoot == null)
            {
                return;
            }

            var parent = menuRoot.transform;
            var top = parent.Find("top");
            var des = parent.Find("des");
            if (top != null)
            {
                top.gameObject.SetActive(visible);
            }

            if (des != null)
            {
                des.gameObject.SetActive(visible);
            }
        }

        private void ApplyMenuChange(bool toVipMenu)
        {
            DataManager.Instance?.TrySetTavernMenuType(toVipMenu ? TavernMenuType.Vip : TavernMenuType.Popular);
            ApplyMenuVisuals(toVipMenu);
            TavernTopStatusPanelController.RefreshOpenedMenuStatusUi();
        }

        private void ApplyMenuVisuals(bool showVipMenu)
        {
            if (menu1Root != null && menu1Root.activeSelf == showVipMenu)
            {
                menu1Root.SetActive(!showVipMenu);
            }

            if (menu2Root != null && menu2Root.activeSelf != showVipMenu)
            {
                menu2Root.SetActive(showVipMenu);
            }

            RefreshMenuEffectText(showVipMenu);
        }

        /// <summary>菜单效果文案统一隐藏（大众/贵客均不展示）。</summary>
        private void RefreshMenuEffectText(bool vipMenu)
        {
            if (menuEffectText == null)
            {
                return;
            }

            menuEffectText.text = string.Empty;
            if (menuEffectText.gameObject.activeSelf)
            {
                menuEffectText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 冷却中按钮保持可点，仅图标/文案置灰；点击由 OnClickSwitch 弹出 Tips。
        /// </summary>
        private void RefreshSwitchVisual()
        {
            if (switchButton == null)
            {
                return;
            }

            switchButton.interactable = true;
            var onCooldown = DataManager.Instance != null && !DataManager.Instance.IsMenuSwitchCooldownReady();
            var tint = onCooldown ? SwitchCooldownTint : Color.white;
            var graphics = switchButton.GetComponentsInChildren<Graphic>(true);
            for (var index = 0; index < graphics.Length; index++)
            {
                if (graphics[index] != null)
                {
                    graphics[index].color = tint;
                }
            }
        }

        private void ResetFlipPose()
        {
            if (rootRect != null)
            {
                rootRect.localEulerAngles = Vector3.zero;
            }

            ResetMenuCards(menu1Root, menu1Images);
            ResetMenuCards(menu2Root, menu2Images);
        }

        private static void ResetMenuCards(GameObject menuRoot, RectTransform[] images)
        {
            SetMenuChromeVisible(menuRoot, true);
            if (images == null)
            {
                return;
            }

            for (var index = 0; index < images.Length; index++)
            {
                var img = images[index];
                if (img == null)
                {
                    continue;
                }

                img.localEulerAngles = Vector3.zero;
                img.gameObject.SetActive(true);
            }
        }

        private void KillFlip()
        {
            flipping = false;
            if (flipTween != null && flipTween.IsActive())
            {
                flipTween.Kill();
            }

            flipTween = null;
        }
    }
}
