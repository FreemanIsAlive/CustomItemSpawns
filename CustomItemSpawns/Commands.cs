using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Permissions = CustomItemSpawns.Main.Permissions;

namespace CustomItemSpawns
{
    public static class Commands
    {
        private static readonly Config Config = Main.Instance.Config;
        
        [CommandHandler(typeof(ClientCommandHandler))]
        public class ItemOffset : ICommand
        {
            string ICommand.Command => "spawn_item_offset";

            string[] ICommand.Aliases => new[] { "io" };

            string ICommand.Description => "Shows offset of a command sender from a current room center.";

            bool ICommand.Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                var player = Player.Get(sender);
                var room = player.CurrentRoom;

                if (Config.Debug)
                    Log.Debug(player.Nickname + " in the room " + player.CurrentRoom.Name +
                              " called the 'item_offset' command with result offset " +
                              (player.Position - room.Position) + ".");

                response = $"Current room is {room.Name}, current offset is {player.Position - room.Position}.";
                return true;
            }
        }

        [CommandHandler(typeof(RemoteAdminCommandHandler))]
        public class CustomItemSpawnParentCommand : ParentCommand
        {
            public CustomItemSpawnParentCommand()
            {
                LoadGeneratedCommands();
            }
            
            public override string Command => "customitemspawns";

            public override string[] Aliases { get; } = { "is" };

            public override string Description => "This is a parent command for all other.";

            public override void LoadGeneratedCommands()
            {
                RegisterCommand(new SpawnItems());
                RegisterCommand(new RespawnItems());
                RegisterCommand(new ClearSpawnedItems());
                RegisterCommand(new Help());
            }
            
            protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                response = "You haven't entered correct subcommand! Use `is help` for help.";
                return true;
            }
        }

        private class SpawnItems : ICommand
        {
            public string Command => "spawn_items";
            public string[] Aliases => new[] { "si" };

            public string Description =>
                "Spawns all items that are specified in the config. WARNING! This command doesn't remove the items that spawned already by this plugin. Use 'respawn_items' command instead.";

            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                if (Config.Debug)
                    Log.Debug(Player.Get(sender).Nickname + " called the 'spawn_items' command.");

                if (Config.EnablePermissionCheck)
                {
                    var player = Player.Get(sender);
                    if (player.CheckPermission(Config.PermissionsConfig[Permissions.SpawnItems]) || player.CheckPermission(Config.PermissionsConfig[Permissions.All]))
                    {
                        response = "You don't have permission to use this command. You need: " + Config.PermissionsConfig[Permissions.SpawnItems];
                        if (Config.Debug)
                            Log.Debug("Player " + Player.Get(sender).Nickname + " doesn't have permission to use 'spawn_items' command.");
                        return false;
                    }
                }
                
                Main.OnWaitingForPlayers();

                response = "Items has been spawned.";
                return true;
            }
        }

        private class RespawnItems : ICommand
        {
            public string Command => "respawn_items";
            public string[] Aliases => new[] { "ri" };

            public string Description =>
                "Respawns all items that are specified in the config. \nThis command removes the items that spawned already by this plugin. Use 'spawn_items' command instead, if you don't want to remove them.";
            
            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                if (Config.Debug)
                    Log.Debug(Player.Get(sender).Nickname + " called the 'respawn_items' command.");

                if (Config.EnablePermissionCheck)
                {
                    var player = Player.Get(sender);
                    if (player.CheckPermission(Config.PermissionsConfig[Permissions.RespawnItems]) || player.CheckPermission(Config.PermissionsConfig[Permissions.All]))
                    {
                        response = "You don't have permission to use this command. You need: " + Config.PermissionsConfig[Permissions.RespawnItems];
                        if (Config.Debug)
                            Log.Debug("Player " + Player.Get(sender).Nickname + " doesn't have permission to use 'spawn_items' command.");
                        return false;
                    }
                }
                
                if (!Config.EnableItemDataSaving)
                {
                    if (Config.Debug)
                        Log.Debug("Saving item data is disabled in config. Enable it to use this command.");

                    response = "Saving item data is disabled in config. Enable it to use this command.";
                    return false;
                }

                if (Main.SpawnedItems.IsEmpty())
                {
                    if (Config.Debug)
                        Log.Debug("No items to respawn.");

                    response = "No items to respawn.";
                    return false;
                }

                foreach (var item in Main.SpawnedItems)
                {
                    if (Config.Debug)
                        Log.Debug("Removing item " + item + ".");
                    
                    item.UnSpawn();
                }

                response = "Items has been respawned.";
                return true;
            }
        }

        private class ClearSpawnedItems : ICommand
        {
            public string Command => "clear_spawned_items";
            public string[] Aliases => new[] { "csi" };
            public string Description => "Removes all spawned items.";

            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                if (Config.Debug)
                    Log.Debug(Player.Get(sender).Nickname + " called the 'clear_spawned_items' command.");
                
                if (Config.EnablePermissionCheck)
                {
                    var player = Player.Get(sender);
                    if (player.CheckPermission(Config.PermissionsConfig[Permissions.ClearSpawnedItems]) || player.CheckPermission(Config.PermissionsConfig[Permissions.All]))
                    {
                        response = "You don't have permission to use this command. You need: " + Config.PermissionsConfig[Permissions.ClearSpawnedItems];
                        if (Config.Debug)
                            Log.Debug("Player " + Player.Get(sender).Nickname + " doesn't have permission to use 'spawn_items' command.");
                        return false;
                    }
                }
                
                if (!Config.EnableItemDataSaving)
                {
                    if (Config.Debug)
                        Log.Debug("Saving item data is disabled in config. Enable it to use this command.");

                    response = "Saving item data is disabled in config. Enable it to use this command.";
                    return false;
                }
                
                if (Main.SpawnedItems.IsEmpty())
                {
                    if (Config.Debug)
                        Log.Debug("No items to clear.");
                    response = "No items to clear.";
                    return false;
                }
                
                foreach (var item in Main.SpawnedItems)
                {
                    if (Config.Debug)
                        Log.Debug("Removing item " + item + ".");
                    item.UnSpawn();
                }
                
                response = "Items has been cleared.";
                return true;
            }
        }

        private class Help : ICommand
        {
            public string Command => "help";
            public string[] Aliases => new[] { "h", "man", "manual" };

            public string Description => "Shows all available commands.";

            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                if (Config.Debug)
                    Log.Debug(Player.Get(sender).Nickname + " called the 'help' command.");
                
                response = "Available commands:\n" +
                           "   spawn_items (si) - Spawns all items that are specified in the config.\n" +
                           "   respawn_items (ri) - Respawns all items that are specified in the config.\n" +
                           "   clear_spawned_items (csi) - Removes all spawned items, which are not already picked up.\n" +
                           "   help (h, man, manual) - Shows all available commands.";
                return true;
            }
        }
    }
}