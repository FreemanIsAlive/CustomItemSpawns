using Exiled.API.Enums;
using UnityEngine;

namespace CustomItemSpawns
{
    public class SpawnItemData
    {
        public readonly ItemType Type;
        public readonly RoomType Room;
        public readonly Vector3 Offset;

        public SpawnItemData(ItemType item, RoomType room, Vector3 offset)
        {
            Type = item;
            Room = room;
            Offset = offset;
        }

        public override string ToString()
        {
            return $"Type: {Type}, Room: {Room}, Offset: {Offset}";
        }
    }
}