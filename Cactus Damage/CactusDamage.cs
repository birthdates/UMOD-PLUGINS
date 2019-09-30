using Newtonsoft.Json;
using Oxide.Core;
using Rust;

namespace Oxide.Plugins
{
    [Info("Cactus Damage", "birthdates", "1.1.1")]
    [Description("Cacti deal damage to players harvesting/colliding with them.")]
    public class CactusDamage : RustPlugin
    {
        #region Hooks

        private void Init()
        {
            LoadConfig();
        }

        private void OnDispenserGather(ResourceDispenser dispenser, BasePlayer entity)
        {
            if (!dispenser.name.Contains("cactus")) return;
            Hurt(entity, _config.harvestingDamage, dispenser.GetComponent<BaseEntity>() ?? entity);
        }

        private void OnEntityTakeDamage(BasePlayer entity, HitInfo info)
        {
            if (info.Initiator?.ShortPrefabName.Contains("cactus") == false ||
                info.damageTypes.Get(DamageType.Slash) > 0 || info.damageTypes.Get(DamageType.Bleeding) > 0) return;
            Hurt(entity, _config.collisionDamage, info.Initiator ?? entity);
        }

        private static void Hurt(BasePlayer Player, Damage Damage, BaseEntity Initiator)
        {
            var Amount = Random.Range(Damage.MinDamage, Damage.MaxDamage);
            Player.Hurt(Amount, DamageType.Slash, Initiator);
            Player.metabolism.bleeding.value += Amount / 2;
        }

        #endregion

        #region Configuration

        public ConfigFile _config;

        public class Damage
        {
            [JsonProperty("Max Damage")] public float MaxDamage;

            [JsonProperty("Min Damage")] public float MinDamage;
        }

        public class ConfigFile
        {
            [JsonProperty("Collision Damage")] public Damage collisionDamage;

            [JsonProperty("Harvesting Damage")] public Damage harvestingDamage;

            public static ConfigFile DefaultConfig()
            {
                return new ConfigFile
                {
                    harvestingDamage = new Damage
                    {
                        MinDamage = 2f,
                        MaxDamage = 5f
                    },
                    collisionDamage = new Damage
                    {
                        MinDamage = 2f,
                        MaxDamage = 5f
                    }
                };
            }
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            _config = Config.ReadObject<ConfigFile>();
            if (_config == null) LoadDefaultConfig();
        }

        protected override void LoadDefaultConfig()
        {
            _config = ConfigFile.DefaultConfig();
            PrintWarning("Default configuration has been loaded.");
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(_config);
        }

        #endregion
    }
}
//Generated with birthdates' Plugin Maker