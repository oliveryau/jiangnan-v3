using cfg;
using UnityEngine;

namespace JN.Client.Config
{
    /// <summary>
    /// 单个员工在运行时的合成画像：配表静态属性 + 存档成长 + 等级表。
    /// </summary>
    public sealed class StaffRuntimeProfile
    {
        public const float DefaultMoveSpeed = 2f;
        public const float DefaultEmotion = 100f;

        public int StaffId { get; }
        public string Name { get; }
        public StaffPosition Position { get; }
        public StaffRole RuntimeRole { get; }

        public bool CanOrder { get; }
        public bool CanServe { get; }
        public bool CanCheckout { get; }

        public float MoveSpeed { get; }
        public int ServiceAttitude { get; }
        public int Personality { get; }
        public float Emotion { get; }
        public int Level { get; }
        public bool Temporary { get; }

        public float CookSpeedMul { get; }
        public float OrderTimeMul { get; }
        public float ServeTimeMul { get; }
        public float CheckoutTimeMul { get; }
        public float CleanTimeMul { get; }
        public int TipBonusPercent { get; }
        public float StaminaDrainMul { get; }

        public StaffRuntimeProfile(
            int staffId,
            string name,
            StaffPosition position,
            StaffRole runtimeRole,
            bool canOrder,
            bool canServe,
            bool canCheckout,
            float moveSpeed,
            int serviceAttitude,
            int personality,
            float emotion,
            int level,
            bool temporary,
            float cookSpeedMul = 1f,
            float orderTimeMul = 1f,
            float serveTimeMul = 1f,
            float checkoutTimeMul = 1f,
            float cleanTimeMul = 1f,
            int tipBonusPercent = 0,
            float staminaDrainMul = 1f)
        {
            StaffId = staffId;
            Name = name ?? string.Empty;
            Position = position;
            RuntimeRole = runtimeRole;
            CanOrder = canOrder;
            CanServe = canServe;
            CanCheckout = canCheckout;
            MoveSpeed = Mathf.Max(0.1f, moveSpeed);
            ServiceAttitude = serviceAttitude;
            Personality = personality;
            Emotion = Mathf.Clamp(emotion, 0f, 100f);
            Level = Mathf.Max(1, level);
            Temporary = temporary;
            CookSpeedMul = Mathf.Max(0.1f, cookSpeedMul);
            OrderTimeMul = Mathf.Max(0.1f, orderTimeMul);
            ServeTimeMul = Mathf.Max(0.1f, serveTimeMul);
            CheckoutTimeMul = Mathf.Max(0.1f, checkoutTimeMul);
            CleanTimeMul = Mathf.Max(0.1f, cleanTimeMul);
            TipBonusPercent = Mathf.Max(0, tipBonusPercent);
            StaminaDrainMul = Mathf.Clamp(staminaDrainMul, 0.1f, 2f);
        }

        public float MoveSpeedMultiplier
        {
            get
            {
                var baseMul = MoveSpeed / DefaultMoveSpeed;
                var emotionMul = 0.85f + Emotion / 400f;
                return Mathf.Clamp(baseMul * emotionMul, 0.4f, 2.5f);
            }
        }

        public float CookSpeedFactor
        {
            get
            {
                var emotionPart = Emotion / 200f;
                var personalityPart = Personality / 500f;
                var factor = (0.7f + emotionPart + personalityPart) * CookSpeedMul;
                return Mathf.Clamp(factor, 0.5f, 2.0f);
            }
        }

        public bool CanHandleWaiterTaskKey(string taskKey)
        {
            if (string.IsNullOrEmpty(taskKey))
            {
                return true;
            }

            return taskKey switch
            {
                "Order" => CanOrder,
                "Serve" => CanServe,
                "Checkout" => CanCheckout,
                "Clean" => true,
                _ => false
            };
        }
    }
}
