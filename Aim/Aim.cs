using System.Collections.Generic;
using System.Linq;
using Oxide.Core;
using Rust;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Aim", "birthdates", "1.0.0")]
    [Description("Aim assit testing")]
    public class Aim : RustPlugin
    {
        #region Variables

        private const string permission_use = "aim.use";

        #endregion

        #region Hooks

        private void Init()
        {
            LoadConfig();
            _data = Interface.Oxide.DataFileSystem.ReadObject<Data>(Name);
            permission.RegisterPermission(permission_use, this);
            cmd.AddChatCommand("aim", this, SettingsCommand);
        }

        private void Unload()
        {
            SaveData();
        }

        private void SettingsCommand(BasePlayer Player, string Label, string[] Args)
        {
            if (!permission.UserHasPermission(Player.UserIDString, permission_use)) return;
            if (Args.Length < 1)
            {
                Player.ChatMessage("/" + Label + " MaxDistance|FOV|toggle");
                return;
            }

            if (!_data.PlayerData.ContainsKey(Player.UserIDString))
                _data.PlayerData.Add(Player.UserIDString, new PlayerData());
            var PlayerData = _data.PlayerData[Player.UserIDString];
            var Setting = Args[0].ToLower();
            if (Setting != "toggle")
                if (Args.Length < 2)
                {
                    Player.ChatMessage("/" + Label + " MaxDistance|FOV|toggle");
                    return;
                }

            switch (Setting)
            {
                case "toggle":
                    var New = !PlayerData.Enabled;
                    PlayerData.Enabled = New;
                    Player.ChatMessage($"Aim toggled {(New ? "On" : "Off")}");
                    break;
                case "maxdistance":
                    var Value = Args[1];
                    float Dist;
                    if (!float.TryParse(Value, out Dist))
                    {
                        Player.ChatMessage("Invalid distance #");
                        return;
                    }

                    PlayerData.maxDistance = Dist;
                    Player.ChatMessage("Max distance updated to " + Dist);
                    break;
                case "fov":
                    var FOVV = Args[1];
                    float FOV;
                    if (!float.TryParse(FOVV, out FOV))
                    {
                        Player.ChatMessage("Invalid FOV #");
                        return;
                    }

                    PlayerData.FOV = FOV;
                    Player.ChatMessage("FOV updated to " + FOV);
                    break;
                default:
                    Player.ChatMessage("/" + Label + " MaxDistance|FOV");
                    break;
            }
        }

        private object CanCreateWorldProjectile(HitInfo info)
        {
            var player = info.Initiator as BasePlayer;
            if (player == null) return null;
            if (!_data.PlayerData.ContainsKey(player.UserIDString)) return null;
            var PlayerData = _data.PlayerData[player.UserIDString];
            
            return !PlayerData.Enabled;
        }

        private void OnWeaponFired(BaseProjectile projectile, BasePlayer player, ItemModProjectile mod)
        {
            if (!permission.UserHasPermission(player.UserIDString, permission_use)) return;
            if (!_data.PlayerData.ContainsKey(player.UserIDString))
                _data.PlayerData.Add(player.UserIDString, new PlayerData());
            var PlayerData = _data.PlayerData[player.UserIDString];
            if (!PlayerData.Enabled) return;
            var Hit = ReturnAimbotPlayer(player, PlayerData);
            if (Hit == null) return;

            var proj = mod.projectileObject.Get().GetComponent<Projectile>();
            var num = proj.damageTypes.Sum(damageTypeEntry => damageTypeEntry.amount);
            player.Hurt(num * projectile.damageScale, DamageType.Bullet, Hit);
            
        }

        private static BaseCombatEntity ReturnAimbotPlayer(BasePlayer player, PlayerData Data)
        {
            return Physics
                .BoxCastAll(player.transform.position, new Vector3(Data.FOV / 2, Data.FOV / 2, Data.FOV / 2),
                    Quaternion.Euler(player.serverInput.current.aimAngles) * Vector3.forward,
                    Quaternion.Euler(player.serverInput.current.aimAngles), Data.maxDistance)
                .Select(hit => hit.GetEntity() as BasePlayer)
                .FirstOrDefault(e => e != null && !e.PrefabName.Contains("corpse") && e != player);
        }

        #endregion

        #region Configuration & Language

        private Data _data;

        public class Data
        {
            public Dictionary<string, PlayerData> PlayerData = new Dictionary<string, PlayerData>();
        }

        public class PlayerData
        {
            public bool Enabled = true;
            public float FOV = 10f;
            public float maxDistance = 75f;

            public uint targetBone = 698017942;
        }


        private void SaveData()
        {
            Interface.Oxide.DataFileSystem.WriteObject(Name, _data);
        }

        #endregion
    }
}
//Generated with birthdates' Plugin Maker