namespace Oxide.Plugins
{
    [Info("Infinite Tool", "birthdates", "2.0.2")]
    [Description("Allows player with permission to obtain unlimited ammo / durability")]
    public class InfiniteTool : RustPlugin
    {
        #region Hooks

        private void Init()
        {
            LoadConfig();
            permission.RegisterPermission(permission_durability, this);
            permission.RegisterPermission(permission_explosives, this);
            permission.RegisterPermission(permission_rockets, this);
            permission.RegisterPermission(permission_ammo, this);
        }

        #endregion

        private void OnWeaponFired(BaseProjectile projectile, BasePlayer player)
        {
            if (!player.IPlayer.HasPermission(permission_ammo)) return;
            if (projectile.primaryMagazine.contents != 1) return;
            projectile.primaryMagazine.contents = projectile.primaryMagazine.capacity;
            projectile.SendNetworkUpdateImmediate();
        }

        private void OnExplosiveThrown(BasePlayer player)
        {
            if (!player.IPlayer.HasPermission(permission_explosives)) return;
            if (player.GetActiveItem()?.info.shortname.Contains("signal") == true) return;
            var weapon = player.GetActiveItem().GetHeldEntity() as ThrownWeapon;
            if (weapon == null) return;
            weapon.GetItem().amount += 1;
        }

        private void OnLoseCondition(Item item, ref float amount)
        {
            if (item?.GetOwnerPlayer()?.IPlayer?.HasPermission(permission_durability) == true) amount = 0f;
        }

        private void OnRocketLaunched(BasePlayer player)
        {
            if (!player.IPlayer.HasPermission(permission_rockets)) return;
            var weapon = player.GetActiveItem().GetHeldEntity() as BaseProjectile;
            if (weapon == null) return;
            weapon.primaryMagazine.contents = weapon.primaryMagazine.capacity;
            weapon.SendNetworkUpdateImmediate();
        }

        #region Variables

        private const string permission_durability = "infinitetool.durability";
        private const string permission_explosives = "infinitetool.explosives";
        private const string permission_rockets = "infinitetool.rockets";
        private const string permission_ammo = "infinitetool.ammo";

        #endregion
    }
}
//Generated with birthdates' Plugin Maker