﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
namespace Oxide.Plugins
{
    [Info("Recycle Modifier", "birthdates", "1.0.2")]
    [Description("Ability to change the output of the recycler")]
    public class RecycleModifier : RustPlugin
    {
        private ConfigFile config;

        class ConfigFile
        {
            [JsonProperty("Blacklisted items (wont get the modifier)")]
            public List<string> bAP;

            [JsonProperty("Modifier")]
            public int mod;

            public static ConfigFile DefaultConfig()
            {
                return new ConfigFile()
                {
                    mod = 2,
                    bAP = new List<string>()
                    {
                        "rock",
                        "locker"
                    }

                };
            }

        }

        object OnRecycleItem(Recycler recycler, Item item)
        {

            if (config.bAP.Contains(item.info.shortname))
            {
                return null;
            }
            recycler.inventory?.Remove(item);
            foreach (var i in item.info.Blueprint.ingredients)
            {
                i.amount /= 2;
                i.amount *= config.mod;
                i.amount *= item.amount;
                var z = ItemManager.Create(i.itemDef, Convert.ToInt32(i.amount));
                if(z == null) continue;
                recycler.MoveItemToOutput(z);

            }
            return false;

        }

        protected override void LoadConfig()
        {
            base.LoadConfig();

            config = Config.ReadObject<ConfigFile>();
            if (config == null)
            {
                LoadDefaultConfig();
            }
        }


        protected override void LoadDefaultConfig()
        {
            config = ConfigFile.DefaultConfig();
            PrintWarning("Default configuration has been loaded.");
        }

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                {"NoPermission", "You don't have any permission."},
            }, this);
        }



        protected override void SaveConfig()
        {
            Config.WriteObject(config);
        }

        private void Init()
        {
            LoadConfig();
        }



    }
}