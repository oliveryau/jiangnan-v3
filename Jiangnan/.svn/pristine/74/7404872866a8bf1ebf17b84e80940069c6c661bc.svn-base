using DG.Tweening;
using JN.Client;
using JN.Client.Manager;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ApiProtocols = global::JN.Client.Protocols.Protocols;

namespace JN.Client.UI
{
    public class CreatePlayerPanelControllerData : UIPanelData
    {
    }

    /// <summary>
    /// 负责创建玩家相关的运行时逻辑。
    /// </summary>
    public class CreatePlayerPanelController : QFrameworkPanel<CreatePlayerPanelControllerData>
    {
        private static readonly string[] RandomSurnames =
        {
            "赵", "钱", "孙", "李", "周", "吴", "郑", "王",
            "冯", "陈", "褚", "卫", "蒋", "沈", "韩", "杨",
            "朱", "秦", "尤", "许", "何", "吕", "施", "张"
        };

        private static readonly string[] RandomGivenNames =
        {
            "子轩", "雨桐", "若景", "景然", "子墨", "若汐", "明轩", "沐阳",
            "星河", "清风", "云起", "晨曦", "青岚", "南风", "知夏", "听澜",
            "千雪", "安然", "书瑶", "亦凡", "嘉树", "一诺", "若云", "天佑"
        };

        [SerializeField] private Button btn_CreatePlayer;
        [SerializeField] private TMP_InputField input_PlayerName;
        [SerializeField] private Button btn_suiji;
        [SerializeField] private Button btn_ResetData;
        [SerializeField] private GameObject warningPrefab;

        private const string WarningPrefabPath = "Assets/Res/Resources/UI/Menu/WarningPrefab.prefab";

        /// <summary>
        /// 面板初始化时绑定控件和事件。
        /// </summary>
        protected override void OnPanelInit()
        {
            if (btn_CreatePlayer != null)
            {
                btn_CreatePlayer.onClick.AddListener(OnClickBtnCreatePlayer);
            }

            if (btn_suiji != null)
            {
                btn_suiji.onClick.AddListener(OnClickBtnRandomPlayerName);
            }
            if (btn_ResetData != null)
            {
                btn_ResetData.onClick.AddListener(OnClickBtnResetData);
            }

            ResolveWarningPrefab();
        }

        /// <summary>
        /// 面板打开时读取数据并刷新显示。
        /// </summary>
        /// <param name="data">数据。</param>
        protected override void OnPanelOpen(CreatePlayerPanelControllerData data)
        {
            if (input_PlayerName == null)
            {
                return;
            }

            var lastPlayerName = DataManager.Instance.PlayerData != null
                ? DataManager.Instance.PlayerData.playerName
                : string.Empty;
            input_PlayerName.text = lastPlayerName ?? string.Empty;
            input_PlayerName.MoveTextEnd(false);
        }

        /// <summary>
        /// 面板关闭时清理临时状态和监听。
        /// </summary>
        protected override void OnPanelClose()
        {
            if (btn_CreatePlayer != null)
            {
                btn_CreatePlayer.onClick.RemoveListener(OnClickBtnCreatePlayer);
            }

            if (btn_suiji != null)
            {
                btn_suiji.onClick.RemoveListener(OnClickBtnRandomPlayerName);
            }
            if (btn_ResetData != null)
            {
                btn_ResetData.onClick.RemoveListener(OnClickBtnResetData);
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// 处理按钮创建玩家点击事件。
        /// </summary>
        private void OnClickBtnCreatePlayer()
        {
            GameAudioManager.PlayButtonClick();
            var playerName = input_PlayerName != null ? input_PlayerName.text : string.Empty;
            if (string.IsNullOrWhiteSpace(playerName))
            {
                SpawnWarning("请输入玩家名字", transform);
                return;
            }

            if (LocalSaveMode.Enabled)
            {
                DebugLogUtility.LogSuccess("本地存档模式：跳过服务器登录");
                EnterTownWithLocalProfile(playerName, LocalSaveMode.DefaultPlayerId);
                return;
            }

            ApiProtocols.Instance.Login(playerName, "123", OnLoginSuccess, OnLoginError);
        }

        /// <summary>
        /// 响应登录成功事件并同步状态。
        /// </summary>
        /// <param name="response">参数值。</param>
        private void OnLoginSuccess(ApiProtocols.LoginResponse response)
        {
            DataManager.Instance.SetAuthToken(response != null ? response.token : null);
            DebugLogUtility.LogSuccess("登录成功");
            DebugLogUtility.LogSuccess(response);
            if (response != null) EnterTownWithLocalProfile(response.user.username, response.user.id);
        }

        /// <summary>
        /// 响应登录失败事件并同步状态。
        /// </summary>
        /// <param name="error">参数值。</param>
        private void OnLoginError(string error)
        {
            SpawnWarning("登录失败", transform);
            Debug.LogWarning($"[CreatePlayerPanelController] 网络登录失败。原因: {error}");

            // 服务器不可用时回退本地存档，保证单机可进游戏。
            EnterTownWithLocalProfile(
                input_PlayerName != null ? input_PlayerName.text : string.Empty,
                LocalSaveMode.DefaultPlayerId);
        }

        /// <summary>
        /// 重置本地存档（测试用）。本地模式下清理磁盘存档；联网模式仍走服务器重置。
        /// </summary>
        private void OnClickBtnResetData()
        {
            if (LocalSaveMode.Enabled)
            {
                var deleted = DataManager.Instance.DeleteAllLocalSaves();
                if (input_PlayerName != null)
                {
                    input_PlayerName.text = string.Empty;
                }

                SpawnWarning($"已删除 {deleted} 个本地存档", transform, isRed: false);
                return;
            }

            ApiProtocols.Instance.ResetAllPlayers(
                response => { print("清理数量:" + response.deletedCount); },
                result => { print("清理失败" + result); });
        }

        /// <summary>
        /// 使用本地玩家档案进入大地图。
        /// </summary>
        /// <param name="playerName">名称。</param>
        private void EnterTownWithLocalProfile(string playerName, int userId)
        {
            if (!DataManager.Instance.CreatePlayer(playerName, userId))
            {
                return;
            }

            StartCoroutine(SceneFlowCoordinator.EnterTown(() =>
            {
                CloseSelf();
            }));
        }


        /// <summary>
        /// 处理按钮随机玩家名点击事件。
        /// </summary>
        private void OnClickBtnRandomPlayerName()
        {
            if (input_PlayerName == null)
            {
                return;
            }

            var surname = RandomSurnames[Random.Range(0, RandomSurnames.Length)];
            var givenName = RandomGivenNames[Random.Range(0, RandomGivenNames.Length)];
            input_PlayerName.text = surname + givenName;
            input_PlayerName.MoveTextEnd(false);
        }

        private void SpawnWarning(string text, Transform parent, GameObject obj = null, bool isRed = true)
        {
            var prefab = ResolveWarningPrefab();
            if (prefab == null)
            {
                Debug.LogWarning($"[CreatePlayerPanelController] 缺少提示预制体：{WarningPrefabPath}，内容：{text}");
                return;
            }

            var warning = Instantiate(prefab, parent);
            var warningText = warning.GetComponent<TMP_Text>();
            if (warningText == null)
            {
                warningText = warning.GetComponentInChildren<TMP_Text>(true);
            }

            if (warningText != null)
            {
                warningText.text = text;
                if (!isRed)
                {
                    warningText.color = Color.white;
                }
            }

            var currentPos = warning.transform.position;
            if (obj == null)
            {
                warning.transform.position = new Vector3(currentPos.x + 25, currentPos.y + 45, currentPos.z);
            }
            else
            {
                var pos = Camera.main.WorldToScreenPoint(obj.transform.position);
                warning.transform.position = new Vector3(pos.x + 25, pos.y + 45, pos.z);
            }

            var newPos = warning.transform.position;

            warning.transform.DOMove(new Vector3(newPos.x + 45f, newPos.y + 65f, newPos.z), 1.5f).SetEase(Ease.InQuad);
            var canvas = warning.GetComponent<CanvasGroup>();
            if (canvas != null)
            {
                canvas.DOFade(0f, 1f).SetDelay(.5f);
            }

            Destroy(warning.gameObject, 2f);
        }

        /// <summary>
        /// 获取提示预制体：优先用 Inspector 引用，丢失时按路径回退加载。
        /// </summary>
        private GameObject ResolveWarningPrefab()
        {
            if (warningPrefab != null)
            {
                return warningPrefab;
            }

            warningPrefab = GameplayResourceStore.LoadAsset<GameObject>(WarningPrefabPath);
            return warningPrefab;
        }
    }
}
