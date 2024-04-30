using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Enums;
using Exiled.API.Interfaces;
using UnityEngine;
using Permissions = CustomItemSpawns.Main.Permissions;

namespace CustomItemSpawns
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        [Description("Enable experimental mode.")]
        public bool EnableExperimental { get; set; } = false;
        
        [Description("Enable permission check?")]
        public bool EnablePermissionCheck { get; set; } = true;
        
        [Description("Enable saving data of all items spawned by this plugin?\nIt may affect performance, if you have a lot of custom item spawns.")]
        public bool EnableItemDataSaving { get; set; } = true;
        
        [Description("List of permissions.")]
        public Dictionary<Permissions, string> PermissionsConfig { get; set; } = new Dictionary<Permissions, string>
        {
            { Permissions.SpawnItems, "custom_item_spawns.spawn_items" },
            { Permissions.RespawnItems, "custom_item_spawns.respawn_items" },
            { Permissions.ClearSpawnedItems, "custom_item_spawns.clear_spawned_items" },
            { Permissions.All, "custom_item_spawns.*" }
        };
        
        [Description("List of items to spawn.")]
        public List<ItemType> Items { get; set; } = new List<ItemType>
        {
            ItemType.KeycardContainmentEngineer,
            ItemType.GunCOM15
        };
        
        [Description("List of corresponding rooms.")]
        public List<RoomType> Rooms { get; set; } = new List<RoomType>
        {
            RoomType.Hcz079,
            RoomType.EzCafeteria
        };
        
        [Description("List of corresponding offsets.")]
        public List<Vector3> Offsets { get; set; } = new List<Vector3>
        {
            new Vector3(0, 1.5f, 2),
            new Vector3(0, 0, 0)
        };
    }
}