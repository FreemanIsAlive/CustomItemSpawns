using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Player;

namespace CustomItemSpawns
{
    public class Main : Plugin<Config>
    {
        public override string Name => "CustomItemSpawns";

        public override string Prefix => Name;

        public override string Author => "FreemanIsAlive";
        
        public override Version Version { get; } = new Version(0, 1);
        
        private static List<SpawnItemData> _items = new List<SpawnItemData>();
        
        // I think it's best not to reassign this list on reload, as it won't become irrelevant, and the commands will not work as expected
        public static readonly List<Pickup> SpawnedItems = new List<Pickup>();
        
        public static Main Instance;
        
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once EmptyConstructor
        public Main() {}
        
        public override void OnEnabled()
        {
            Instance = this;
            
            _items = RegisterItems();
            
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
            
            if (Instance.Config.EnableItemDataSaving)
                Exiled.Events.Handlers.Player.PickingUpItem += OnPickingUpItem;
                
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            if (Instance.Config.Debug)
                Log.Debug("Plugin has been disabled...\nCleaning up...");
            
            _items = new List<SpawnItemData>();
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
            
            if (Instance.Config.EnableItemDataSaving)
                Exiled.Events.Handlers.Player.PickingUpItem -= OnPickingUpItem;

            Instance = null;
            
            base.OnDisabled();
        }

        // Spawns all items from _items.
        public static void OnWaitingForPlayers()
        {
            if (_items == null || _items.Count <= 0) return;
            foreach (var item in _items)
            {
                var room = Room.Get(item.Room);
                if (room is null)
                {
                    if (Instance.Config.Debug)
                        Log.Debug($"Failed to spawn {item.Type} in {item.Room} at {item.Offset} because the room is null.");
                    continue;
                }
                
                var offsetPosition = Instance.Config.EnableExperimental ? room.Position + room.Rotation * item.Offset : room.Position + item.Offset;
                
                var pickup = Pickup.CreateAndSpawn(item.Type, offsetPosition, room.Rotation);
                
                if (Instance.Config.EnableItemDataSaving) 
                    SpawnedItems.Add(pickup);
                
                // past builds had null reference exception here so I've added logging for it
                if (!Instance.Config.Debug) return;
                Log.Debug(pickup != null
                    ? $"Spawned {item.Type} in {item.Room} at {item.Offset} with experimental mode '{Instance.Config.EnableExperimental}'. String representation: {item}\nPickup data: {pickup}"
                    : $"Failed to spawn {item.Type} in {item.Room} at {item.Offset} with experimental mode '{Instance.Config.EnableExperimental}'. String representation: {item}");
            }
        }

        // Registers all items from config by taking elements at same indices from each list.
        // Doesn't directly assign value to the _items for the sake of code readability and possible expansion of usage scenarios.
        private static List<SpawnItemData> RegisterItems()
        {
            var items = new List<SpawnItemData>();
            var config = Instance.Config; // almost unreadable to use Instance.Config directly everywhere here
            
            if (config.Items == null || config.Items.Count <= 0) return items;
            
            foreach (var item in config.Items)
            {
                items.Add(new SpawnItemData(item, config.Rooms[config.Items.IndexOf(item)], config.Offsets[config.Items.IndexOf(item)]));
                
                if (config.Debug)
                    Log.Debug($"Registered item {item} in room {config.Rooms[config.Items.IndexOf(item)]} with offset {config.Offsets[config.Items.IndexOf(item)]}. String representation: {items[items.Count - 1]}, index is {config.Items.IndexOf(item)}");
            }

            return items;
        }

        // Removes pickups from list when they are picked up to prevent exceptions and unexpected behaviour.
        private static void OnPickingUpItem(PickingUpItemEventArgs ev)
        {
            if (!SpawnedItems.Contains(ev.Pickup)) return;
            
            if (Instance.Config.Debug)
                Log.Debug($"Player {ev.Player.Nickname} picked up spawned pickup ({ev.Pickup.Info.ItemId}), so it will be removed from list.");
            SpawnedItems.Remove(ev.Pickup);
        }
        
        // I think it is simpler to use enum in config instead of string access.
        public enum Permissions { SpawnItems, RespawnItems, ClearSpawnedItems, All }
    }
}