using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JN.Client.Protocols
{

    /// <summary>
    /// 负责网络协议相关的运行时逻辑。
    /// </summary>
    public sealed class Protocols : MonoBehaviour
    {
        private const string DefaultBaseUrl = "http://10.8.10.19:5000";
        private const string ProtocolsPrefabPath = "Assets/Res/Resources/Runtime/Protocols.prefab";
        private const string SendLogColor = "#FF1493";
        private const string ReceiveLogColor = "#00FF00";

        private static Protocols instance;

        [Header("API 配置")]
        [SerializeField] private string baseUrl = DefaultBaseUrl;
        [SerializeField] private bool dontDestroyOnLoad = true;
        [SerializeField] private bool ignoreSslErrorsInEditor = true;

        public static Protocols Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                var existing = FindFirstObjectByType<Protocols>();
                if (existing != null)
                {
                    instance = existing;
                    return instance;
                }

                var prefab = LoadProtocolsPrefab();
                if (prefab != null)
                {
                    var root = Instantiate(prefab);
                    root.name = nameof(Protocols);
                    instance = root.GetComponent<Protocols>();
                    return instance;
                }

                instance = CreateFallbackInstance();
                return instance;
            }
        }

        public string BaseUrl
        {
            get
            {
                var configuredBaseUrl = string.IsNullOrWhiteSpace(baseUrl) ? DefaultBaseUrl : baseUrl.TrimEnd('/');
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (configuredBaseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    return "http://" + configuredBaseUrl.Substring("https://".Length);
                }
#endif
                return configuredBaseUrl;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
           
        }

        public Coroutine Get<T>(string path, Action<T> onSuccess, Action<string> onError = null, string token = null)
        {
            return StartCoroutine(SendRequest<T>(UnityWebRequest.kHttpVerbGET, path, null, onSuccess, onError, token));
        }

        public Coroutine Post<T>(string path, object body, Action<T> onSuccess, Action<string> onError = null, string token = null)
        {
            return StartCoroutine(SendRequest<T>(UnityWebRequest.kHttpVerbPOST, path, body, onSuccess, onError, token));
        }

        public Coroutine Put<T>(string path, object body, Action<T> onSuccess, Action<string> onError = null, string token = null)
        {
            return StartCoroutine(SendRequest<T>(UnityWebRequest.kHttpVerbPUT, path, body, onSuccess, onError, token));
        }

        public Coroutine Delete<T>(string path, Action<T> onSuccess, Action<string> onError = null, string token = null)
        {
            return StartCoroutine(SendRequest<T>(UnityWebRequest.kHttpVerbDELETE, path, null, onSuccess, onError, token));
        }

        public Coroutine HealthCheck(Action<HealthResponse> onSuccess, Action<string> onError = null)
        {
            return Get("/health", onSuccess, onError);
        }

        public Coroutine Register(string username, string password, Action<AuthResponse> onSuccess, Action<string> onError = null)
        {
            return Post("/api/auth/register", new AuthRequest { username = username, password = password }, onSuccess, onError);
        }

        public Coroutine Login(string username, string password, Action<LoginResponse> onSuccess, Action<string> onError = null)
        {
            return Post("/api/auth/login", new AuthRequest { username = username, password = password }, onSuccess, onError);
        }

        public Coroutine GetPlayerData(string token, Action<PlayerDataResponse> onSuccess, Action<string> onError = null)
        {
            return Get("/api/player/data", onSuccess, onError, token);
        }

        public Coroutine UpdatePlayerData(string token, object body, Action<PlayerDataResponse> onSuccess, Action<string> onError = null)
        {
            return Put("/api/player/data", body, onSuccess, onError, token);
        }

        public Coroutine CreateRole(string token, string username, string playerPic, Action<CreateRoleResponse> onSuccess, Action<string> onError = null)
        {
            return Post("/api/player/create-role", new CreateRoleRequest { username = username, playerpic = playerPic }, onSuccess, onError, token);
        }

        public Coroutine GetPlayerCoins(string token, Action<CoinsResponse> onSuccess, Action<string> onError = null)
        {
            return Get("/api/player/coins", onSuccess, onError, token);
        }

        public Coroutine UpdatePlayerCoins(string token, int coins, Action<CoinsResponse> onSuccess, Action<string> onError = null)
        {
            return Put("/api/player/coins", new UpdateCoinsRequest { coins = coins }, onSuccess, onError, token);
        }

        public Coroutine GetAllPlayers(Action<List<PlayerDataResponse>> onSuccess, Action<string> onError = null)
        {
            return Get("/api/player/all", onSuccess, onError);
        }

        public Coroutine GetOccupiedLands(Action<List<OccupiedLandResponse>> onSuccess, Action<string> onError = null)
        {
            return Get("/api/player/land/occupied", onSuccess, onError);
        }

        public Coroutine ResetAllPlayers(Action<ResetPlayersResponse> onSuccess, Action<string> onError = null)
        {
            return Delete("/api/player/reset", onSuccess, onError);
        }

        public Coroutine SelectLand(string token, string id, Action<SelectLandResponse> onSuccess, Action<string> onError = null)
        {
            return Post("/api/player/land/select", new SelectLandRequest { id = id }, onSuccess, onError, token);
        }

        public Coroutine AddInventoryItem(string token, string itemId, string name, int quantity, Action<InventoryResponse> onSuccess, Action<string> onError = null)
        {
            return Post("/api/player/inventory", new InventoryItemRequest { itemId = itemId, name = name, quantity = quantity }, onSuccess, onError, token);
        }

        public Coroutine RemoveInventoryItem(string token, string itemId, Action<string> onSuccess, Action<string> onError = null)
        {
            return Delete<string>($"/api/player/inventory/{itemId}", onSuccess, onError, token);
        }

        public Coroutine AddFurniture(string token, FurnitureRequest body, Action<FurnitureActionResponse> onSuccess, Action<string> onError = null)
        {
            return Post("/api/player/restaurant/furniture", body, onSuccess, onError, token);
        }

        public Coroutine UpgradeFurniture(string token, string id, object body, Action<FurnitureUpgradeResponse> onSuccess, Action<string> onError = null)
        {
            return Put($"/api/player/restaurant/furniture/{id}", body, onSuccess, onError, token);
        }

        public Coroutine RemoveFurniture(string token, string id, Action<string> onSuccess, Action<string> onError = null)
        {
            return Delete<string>($"/api/player/restaurant/furniture/{id}", onSuccess, onError, token);
        }

        public Coroutine HireStaff(string token, StaffRequest body, Action<StaffActionResponse> onSuccess, Action<string> onError = null)
        {
            return Post("/api/player/restaurant/staff", body, onSuccess, onError, token);
        }

        public Coroutine UpgradeStaff(string token, string id, object body, Action<StaffActionResponse> onSuccess, Action<string> onError = null)
        {
            return Put($"/api/player/restaurant/staff/{id}", body, onSuccess, onError, token);
        }

        public Coroutine FireStaff(string token, string id, Action<string> onSuccess, Action<string> onError = null)
        {
            return Delete<string>($"/api/player/restaurant/staff/{id}", onSuccess, onError, token);
        }

        public Coroutine SaveCustomers(string token, object body, Action<CustomerSaveResponse> onSuccess, Action<string> onError = null)
        {
            return Put("/api/player/restaurant/customers", body, onSuccess, onError, token);
        }

        private IEnumerator SendRequest<T>(string method, string path, object body, Action<T> onSuccess, Action<string> onError, string token)
        {
            if (JN.Client.LocalSaveMode.Enabled)
            {
                var blockedMessage = $"本地存档模式已屏蔽网络请求：{method} {path}";
                Debug.LogWarning($"[Protocols] {blockedMessage}");
                onError?.Invoke(blockedMessage);
                yield break;
            }

            var url = BuildUrl(path);
            using var request = new UnityWebRequest(url, method);
            string requestJson = null;

            if (body != null)
            {
                requestJson = JsonConvert.SerializeObject(body);
                var bytes = Encoding.UTF8.GetBytes(requestJson);
                request.uploadHandler = new UploadHandlerRaw(bytes);
                request.uploadHandler.contentType = "application/json";
            }

            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Accept", "application/json");
            request.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.SetRequestHeader("Authorization", $"Bearer {token}");
            }

            LogSendRequest(method, url, requestJson, token);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (ignoreSslErrorsInEditor && url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                request.certificateHandler = new PermissiveCertificateHandler();
            }
#endif

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                var errorMessage = string.IsNullOrWhiteSpace(request.error) ? request.downloadHandler.text : request.error;
                LogReceiveResponse(method, url, request.responseCode, errorMessage, false);
                onError?.Invoke(errorMessage);
                yield break;
            }

            var responseText = request.downloadHandler.text;
            LogReceiveResponse(method, url, request.responseCode, responseText, true);
            if (typeof(T) == typeof(string))
            {
                onSuccess?.Invoke((T)(object)responseText);
                yield break;
            }

            try
            {
                var data = JsonConvert.DeserializeObject<T>(responseText);
                onSuccess?.Invoke(data);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }

        private string BuildUrl(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BaseUrl;
            }

            return path.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? path
                : $"{BaseUrl}/{path.TrimStart('/')}";
        }

        /// <summary>
        /// 输出带颜色的网络发送日志。
        /// </summary>
        /// <param name="method">HTTP 方法。</param>
        /// <param name="url">请求地址。</param>
        /// <param name="requestJson">请求体 JSON。</param>
        /// <param name="token">鉴权令牌。</param>
        private static void LogSendRequest(string method, string url, string requestJson, string token)
        {
            var tokenText = string.IsNullOrWhiteSpace(token) ? "无" : MaskToken(token);
            var bodyText = string.IsNullOrWhiteSpace(requestJson) ? "无请求体" : requestJson;
            Debug.Log($"<color={SendLogColor}>[Network Send] Method={method} Url={url} Token={tokenText} Body={bodyText}</color>");
        }

        /// <summary>
        /// 输出带颜色的网络接收日志。
        /// </summary>
        /// <param name="method">HTTP 方法。</param>
        /// <param name="url">请求地址。</param>
        /// <param name="statusCode">响应状态码。</param>
        /// <param name="responseText">响应文本。</param>
        /// <param name="isSuccess">是否成功。</param>
        private static void LogReceiveResponse(string method, string url, long statusCode, string responseText, bool isSuccess)
        {
            var resultText = isSuccess ? "成功" : "失败";
            var contentText = string.IsNullOrWhiteSpace(responseText) ? "空响应" : responseText;
            Debug.Log($"<color={ReceiveLogColor}>[Network Received] Result={resultText} Method={method} Url={url} Status={statusCode} Body={contentText}</color>");
        }

        /// <summary>
        /// 对令牌做简短脱敏，避免日志中完整暴露。
        /// </summary>
        /// <param name="token">原始令牌。</param>
        /// <returns>脱敏后的令牌文本。</returns>
        private static string MaskToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return "无";
            }

            if (token.Length <= 12)
            {
                return token;
            }

            return $"{token.Substring(0, 6)}...{token.Substring(token.Length - 4)}";
        }

        /// <summary>
        /// 读取网络协议单例预制体，避免运行时用代码创建节点。
        /// </summary>
        /// <returns>读取成功返回预制体，否则返回 null。</returns>
        private static GameObject LoadProtocolsPrefab()
        {
            return GameplayResourceStore.LoadAsset<GameObject>(ProtocolsPrefabPath);
        }

        /// <summary>
        /// 当网络协议预制体在打包环境中未成功加载时，创建兜底实例，避免流程直接中断。
        /// </summary>
        /// <returns>创建完成的网络协议实例。</returns>
        private static Protocols CreateFallbackInstance()
        {
            Debug.LogWarning($"[Protocols] 未能加载预制体：{ProtocolsPrefabPath}，已改为运行时创建兜底实例。请同步检查 Addressables 构建内容。");
            var root = new GameObject(nameof(Protocols));
            return root.AddComponent<Protocols>();
        }

#if UNITY_EDITOR
        private sealed class PermissiveCertificateHandler : CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData)
            {
                return true;
            }
        }
#endif

        [Serializable]
        public class AuthRequest
        {
            public string username;
            public string password;
        }

        [Serializable]
        public class CreateRoleRequest
        {
            public string username;
            public string playerpic;
        }

        [Serializable]
        public class SelectLandRequest
        {
            public string id;
        }

        [Serializable]
        public class InventoryItemRequest
        {
            public string itemId;
            public string name;
            public int quantity;
        }

        [Serializable]
        public class FurnitureRequest
        {
            public string id;
            public string name;
            public string category;
            public int level;
            public int comfort;
            public int? cost;
        }

        [Serializable]
        public class StaffRequest
        {
            public string id;
            public string role;
            public string name;
            public int level;
            public int salary;
        }

        [Serializable]
        public class HealthResponse
        {
            public bool ok;
            public DbInfo db;
        }

        [Serializable]
        public class DbInfo
        {
            public int readyState;
            public string stateText;
        }

        [Serializable]
        public class AuthResponse
        {
            public string token;
            public UserInfo user;
            public string message;
            public string error;
        }

        [Serializable]
        public class LoginResponse : AuthResponse
        {
            public bool isNewUser;
        }

        [Serializable]
        public class UserInfo
        {
            public int id;
            public string username;
        }

        [Serializable]
        public class PlayerDataResponse
        {
            public string id;
            public string userId;
            public string userName;
            public string playerPic;
            public int coins;
            public int level;
            public int experience;
            [JsonConverter(typeof(LandInfoConverter))]
            public LandInfo land;
            public RestaurantInfo restaurant;
            public List<object> inventory;
            public string lastLogin;
            public string updatedAt;
        }

        [Serializable]
        public class RestaurantInfo
        {
            public int level;
            public int reputation;
            public List<object> furniture;
            public List<object> staff;
            public int customerCount;
            public List<object> customers;
        }

        [Serializable]
        public class CoinsResponse
        {
            public int coins;
        }

        [Serializable]
        public class UpdateCoinsRequest
        {
            public int coins;
        }

        [Serializable]
        public class SelectLandResponse
        {
            public string id;
            public string areaCode;
            public DateTime selectedAt;
        }

        [Serializable]
        public class InventoryResponse
        {
            public string message;
        }

        [Serializable]
        public class FurnitureActionResponse
        {
            public int cost;
            public int coins;
            public RestaurantInfo restaurant;
        }

        [Serializable]
        public class FurnitureUpgradeResponse
        {
            public FurnitureInfo furniture;
            public int cost;
            public int coins;
            public RestaurantInfo restaurant;
        }

        [Serializable]
        public class StaffActionResponse
        {
            public int cost;
            public int coins;
            public RestaurantInfo restaurant;
        }

        [Serializable]
        public class CustomerSaveResponse
        {
            public int customerCount;
            public List<object> customers;
        }

        [Serializable]
        public class CreateRoleResponse
        {
            public string userName;
            public string playerPic;
        }

        [Serializable]
        public class ResetPlayersResponse
        {
            public bool ok;
            public int deletedCount;
        }

        [Serializable]
        public class OccupiedLandResponse
        {
            public string userName;
            public LandInfo land;
        }

        [Serializable]
        public class LandInfo
        {
            public string id;
            public string areaCode;
            public DateTime selectedAt;
        }

        private sealed class LandInfoConverter : JsonConverter<LandInfo>
        {
            public override LandInfo ReadJson(JsonReader reader, Type objectType, LandInfo existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null)
                {
                    return null;
                }

                if (reader.TokenType == JsonToken.Integer || reader.TokenType == JsonToken.Float)
                {
                    var idValue = Convert.ToString(reader.Value);
                    return string.IsNullOrWhiteSpace(idValue) || idValue == "0"
                        ? null
                        : new LandInfo { id = idValue };
                }

                if (reader.TokenType == JsonToken.String)
                {
                    var idValue = reader.Value?.ToString();
                    return string.IsNullOrWhiteSpace(idValue) || idValue == "0"
                        ? null
                        : new LandInfo { id = idValue };
                }

                if (reader.TokenType == JsonToken.StartObject)
                {
                    var jo = Newtonsoft.Json.Linq.JObject.Load(reader);
                    var land = new LandInfo
                    {
                        id = jo["id"]?.ToString(),
                        areaCode = jo["areaCode"]?.ToString()
                    };

                    if (DateTime.TryParse(jo["selectedAt"]?.ToString(), out var selectedAt))
                    {
                        land.selectedAt = selectedAt;
                    }

                    return string.IsNullOrWhiteSpace(land.id) && string.IsNullOrWhiteSpace(land.areaCode)
                        ? null
                        : land;
                }

                return null;
            }

            public override void WriteJson(JsonWriter writer, LandInfo value, JsonSerializer serializer)
            {
                serializer.Serialize(writer, value);
            }
        }

        [Serializable]
        public class FurnitureInfo
        {
            public string id;
            public string name;
            public string category;
            public int level;
            public int comfort;
        }
    }
}
