using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Oxide.Core;
using Newtonsoft.Json;

namespace Oxide.Plugins
{
    [Info("CupboardProtection", "Wulf/lukespragg and birthdates", "1.5.0")]
    [Description("Makes cupboards and their foundations invulnerable, unable to be destroyed.")]

    class CupboardProtection : RustPlugin
    {
        private int Mask = LayerMask.GetMask("Construction");

        #region Hooks
        object OnEntityTakeDamage(DecayEntity entity, HitInfo info)
        {
            var Initiator = info.Initiator;
            if (!Initiator) return null;
            
            if (entity.name.Contains("cupboard"))
            {
                return CHook("CanDamageTc", Initiator, entity);
            }

            if (!Configuration.foundation || !entity.name.Contains("foundation")) return null;
            if (IDData.IDs.Values.ToList().Exists(id => id == entity.net.ID))
            {
                return CHook("CanDamageTcFloor", Initiator, entity);
            }
            return null;
        }

        object CHook(string Name, BaseEntity Player, DecayEntity Entity)
        {
            var Hook = Interface.CallHook(Name, Player, Entity);
            return Hook is bool ? Hook : false;
        }

        void Init()
        {
            LoadConfig();
            if (!Configuration.foundation)
            {
                Unsubscribe("OnEntityBuilt");
                Unsubscribe("OnEntityKill");
            }
            IDData = Interface.Oxide.DataFileSystem.ReadObject<Data>(Name);
        }

        void Unload() => SaveData();

        void OnEntityBuilt(Planner plan, GameObject go)
        {
            var Priv = go.GetComponent<BaseEntity>() as BuildingPrivlidge;
            if (!Priv) return;
            var Foundation = GetFoundation(Priv);
            if (!Foundation) return;
            IDData.IDs.Add(Priv.net.ID, Foundation.net.ID);
        }

        BuildingBlock GetFoundation(BuildingPrivlidge Priv)
        {
            foreach (var Hit in Physics.RaycastAll(Priv.transform.position, Vector3.down, 2f, Mask,
                QueryTriggerInteraction.Ignore))
            {
                var E = Hit.GetEntity() as BuildingBlock;
                if (E) return E;
            }

            return null;
        }

        void OnEntityKill(BuildingPrivlidge entity)
        {
            if (IDData.IDs.ContainsKey(entity.net.ID))
            {
                IDData.IDs.Remove(entity.net.ID);
            }
        }
        #endregion

        #region Data
        private Data IDData;

        private class Data
        {
            public readonly Dictionary<uint, uint> IDs = new Dictionary<uint, uint>();
        }

        void SaveData()
        {
            Interface.Oxide.DataFileSystem.WriteObject(Name, IDData);
        }
        #endregion

        #region Configuration

        private ConfigFile Configuration;

        public class ConfigFile
        {

            [JsonProperty("Foundation Invincible?")]
            public bool foundation;

            public static ConfigFile DefaultConfig()
            {
                return new ConfigFile()
                {
                    foundation = true
                };
            }
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            Configuration = Config.ReadObject<ConfigFile>();
            if (Config == null)
            {
                LoadDefaultConfig();
            }
        }

        protected override void LoadDefaultConfig()
        {
            Configuration = ConfigFile.DefaultConfig();
            PrintWarning("Default configuration has been loaded.");
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(Configuration);
        }
        #endregion
    }
}