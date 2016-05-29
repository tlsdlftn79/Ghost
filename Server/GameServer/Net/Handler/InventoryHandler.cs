﻿using Server.Common.Constants;
using Server.Common.IO.Packet;
using Server.Ghost;
using Server.Ghost.Characters;
using Server.Ghost.Provider;
using Server.Net;
using Server.Packet;

namespace Server.Handler
{
    class InventoryHandler
    {
        public static void MoveItem_Req(InPacket lea, Client gc)
        {
            byte SourceType = lea.ReadByte();
            byte SorceSlot = lea.ReadByte();
            byte TargetType = lea.ReadByte();
            byte TargetSlot = lea.ReadByte();
            int Quantit = lea.ReadInt();
            Item Source = gc.Character.Items.GetItem(SourceType, SorceSlot);
            Item Target = gc.Character.Items.GetItem(TargetType, TargetSlot);
            var chr = gc.Character;

            if (TargetType == 0x63 && TargetSlot == 0x63)
            {
                if (SourceType == 0xFF && SorceSlot == 0xFF)
                    return;
                Map map = MapFactory.GetMap(chr.MapX, chr.MapY);
                InventoryPacket.charDropItem(gc, map.DropOriginalID, Source.ItemID, chr.PlayerX, (short)(chr.PlayerY - 50), Quantit);
                map.DropItem.Add(map.DropOriginalID, Source);
                map.DropOriginalID++;
                chr.Items.RemoveItem(SourceType, SorceSlot);
            }
            else
            {
                if (gc.Character.Items.GetItem(TargetType, TargetSlot) == null)
                {
                    Source.type = TargetType;
                    Source.slot = TargetSlot;
                }
                else
                {   // 交換位置(swap)
                    chr.Items.RemoveItem(SourceType, SorceSlot);
                    chr.Items.RemoveItem(TargetType, TargetSlot);
                    byte swapSlot = Source.slot;
                    Source.slot = Target.slot;
                    Target.slot = swapSlot;
                    chr.Items.Add(Source);
                    chr.Items.Add(Target);
                }
            }
            UpdateEquip(gc, SourceType, TargetType);
        }

        public static void UseSpend_Req(InPacket lea, Client gc)
        {
            var chr = gc.Character;
            byte Type = lea.ReadByte();
            byte Slot = lea.ReadByte();
            Item ItemID = chr.Items.GetItem(Type, Slot);
            Map map = MapFactory.GetMap(chr.MapX, chr.MapY);
            var use = ItemFactory.useData[ItemID.ItemID];
            // 使用回復HP 跟 MP 的物品
            if (use.Hp != -1)
            {
                if ((chr.MaxHp > chr.Hp + use.Hp))
                {
                    chr.Hp += (short)use.Hp;
                    StatusPacket.updateHpMp(gc, chr.Hp, chr.Mp, 0);
                    chr.Items.RemoveItem(Type, Slot);
                }
                else if (chr.MaxHp - chr.Hp < use.Hp)
                {
                    chr.Hp += (short)chr.MaxHp;
                    StatusPacket.updateHpMp(gc, chr.Hp, chr.Mp, 0);
                    chr.Items.RemoveItem(Type, Slot);
                }
            }
            if (use.Mp != -1)
            {
                if ((chr.MaxMp > chr.Mp + use.Mp))
                {
                    chr.Mp += (short)use.Mp;
                    StatusPacket.updateHpMp(gc, chr.Hp, chr.Mp, 0);
                    chr.Items.RemoveItem(Type, Slot);
                }
                else if (chr.MaxMp - chr.Mp < use.Mp)
                {
                    chr.Mp += (short)chr.MaxMp;
                    StatusPacket.updateHpMp(gc, chr.Hp, chr.Mp, 0);
                    chr.Items.RemoveItem(Type, Slot);
                }
            }
            // 其他
            switch (ItemID.ItemID)
            {
                case 8843030: // 豬大長召喚符
                    break;
            }
        }

        public static void InvenUseSpendShout_Req(InPacket lea, Client gc)
        {
            byte Slot = lea.ReadByte();
            string Message = lea.ReadString(60);
            lea.ReadInt();
            lea.ReadByte();
            lea.ReadByte();
            if (Slot >= 0 && Slot < 24 && Message.Length <= 60)
            {
                gc.Character.Items.RemoveItem(InventoryType.ItemType.Spend3, Slot);
                foreach (Character all in MapFactory.AllCharacters)
                {
                    MapPacket.InvenUseSpendShout(all.Client, Message);
                }
                InventoryPacket.getInvenSpend3(gc);
            }
        }

        public static void PickupItem(InPacket lea, Client gc)
        {
            int OriginalID = lea.ReadInt();
            int ItemID = lea.ReadInt();
            lea.ReadInt();
            byte Type = InventoryType.getItemType(ItemID);
            byte Slot = gc.Character.Items.GetNextFreeSlot((InventoryType.ItemType)Type);
            var chr = gc.Character;

            Item oItem = new Item(ItemID, Slot, (byte)Type, 1);
            Map map = MapFactory.GetMap(chr.MapX, chr.MapY);
            if (!map.DropItem.ContainsKey(OriginalID))
                return;
            chr.Items.Add(oItem);
            InventoryPacket.clearDropItem(gc, chr.CharacterID, OriginalID, ItemID, 1);
            map.DropItem.Remove(OriginalID);
            UpdateEquip(gc, Type);
        }

        public static void UpdateEquip(Client gc, byte SourceType, byte TargetType = 0xFF)
        {
            switch (SourceType)
            {
                case 0:
                    UpdateAvatar(gc);
                    switch (TargetType)
                    {
                        case 1:
                            InventoryPacket.getInvenEquip1(gc);
                            break;
                        case 2:
                            InventoryPacket.getInvenEquip2(gc);
                            break;
                    }
                    break;
                case 1:
                    if (TargetType == 0)
                        UpdateAvatar(gc);
                    InventoryPacket.getInvenEquip1(gc);
                    break;
                case 2:
                    if (TargetType == 0)
                        UpdateAvatar(gc);
                    InventoryPacket.getInvenEquip2(gc);
                    break;
                case 3:
                    InventoryPacket.getInvenSpend3(gc);
                    break;
                case 4:
                    InventoryPacket.getInvenOther4(gc);
                    break;
                case 5:
                    InventoryPacket.getInvenPet5(gc);
                    break;
            }
        }

        public static void UpdateAvatar(Client gc)
        {
            InventoryPacket.getInvenEquip(gc);
            StatusPacket.getStatusInfo(gc);
            InventoryPacket.getAvatar(gc);
        }
    }
}