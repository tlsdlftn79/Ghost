﻿using Server.Common.Constants;
using Server.Common.IO.Packet;
using Server.Common.Net;
using Server.Net;

namespace Server.Packet
{
    public static class StatusPacket
    {
        public static void getStatusInfo(Client c)
        {
            using (OutPacket plew = new OutPacket(ServerOpcode.CHAR_ALL))
            {
                var chr = c.Character;
                plew.WriteInt(0); // length + CRC
                plew.WriteInt(0);
                plew.WriteString(chr.Name, 20);
                plew.WriteString(chr.Title, 20);
                plew.WriteByte(chr.Gender);
                plew.WriteByte(chr.Level);
                plew.WriteByte(chr.Class);
                plew.WriteByte(chr.ClassLevel);
                plew.WriteByte(0);
                plew.WriteByte(0xFF);
                plew.WriteShort(chr.MaxHp);
                plew.WriteShort(chr.Hp);
                plew.WriteShort(chr.MaxMp);
                plew.WriteInt(chr.Mp);
                plew.WriteInt(GameConstants.getExpNeededForLevel(chr.Level));
                plew.WriteInt(0);
                plew.WriteInt(chr.Exp);
                plew.WriteInt(0);
                plew.WriteShort(chr.Fame);
                plew.WriteShort(chr.MaxFury); // 憤怒值(Max)
                plew.WriteShort(chr.Fury);    // 憤怒值
                plew.WriteByte(3);
                plew.WriteByte(chr.JumpHeight); // 跳躍高度
                plew.WriteShort(chr.Str); // 力量
                plew.WriteShort(chr.Dex); // 精力
                plew.WriteShort(chr.Vit); // 氣力
                plew.WriteShort(chr.Int); // 智力
                plew.WriteShort(chr.MaxAttack); // 攻擊力(Max)
                plew.WriteShort(chr.Attack); // 攻擊力(Min)
                plew.WriteShort(chr.MaxMagic); // 魔攻力(Max)
                plew.WriteShort(chr.Magic); // 魔攻力(Min)
                plew.WriteShort(chr.Defense); // 防禦力
                plew.WriteByte(3); // 攻擊速度
                plew.WriteByte(0);
                plew.WriteShort(0);
                plew.WriteShort(chr.AbilityBonus); // 能力上升值
                plew.WriteShort(chr.SkillBonus); // 技能上升值
                plew.WriteShort(chr.UpgradeStr); // 力量+
                plew.WriteShort(chr.UpgradeDex); // 敏捷+
                plew.WriteShort(chr.UpgradeVit); // 氣力+
                plew.WriteShort(chr.UpgradeInt); // 智力+
                plew.WriteShort(chr.UpgradeAttack); // 攻擊力+
                plew.WriteShort(chr.UpgradeMagic); // 魔攻力+
                plew.WriteShort(chr.UpgradeDefense); // 防禦力+
                plew.WriteShort(0);
                plew.WriteShort(0); // Not read
                c.Send(plew);
            }
        }

        public static void updateHpMp(Client c, int updateHp, short updateMp, short updateMaxFury)
        {
            using (OutPacket plew = new OutPacket(ServerOpcode.CHAR_HPSP))
            {
                plew.WriteInt(0); // length + CRC
                plew.WriteInt(0);
                plew.WriteShort(updateHp);
                plew.WriteShort(updateMp);
                plew.WriteShort(updateMaxFury);
                plew.WriteShort(0);
                c.Send(plew);
            }
        }

        public static void updateExp(Client c)
        {
            using (OutPacket plew = new OutPacket(ServerOpcode.CHAR_LVEXP))
            {
                var chr = c.Character;
                plew.WriteInt(0); // length + CRC
                plew.WriteInt(0);
                plew.WriteInt(chr.Level);
                plew.WriteInt(chr.Exp);
                plew.WriteInt(0);
                plew.WriteInt(0);
                plew.WriteInt(0);
                c.Send(plew);
            }
        }

        public static void levelUp(Client c, int level)
        {
            using (OutPacket plew = new OutPacket(ServerOpcode.CHAR_LEVELUP))
            {
                plew.WriteInt(0); // length + CRC
                plew.WriteInt(0);
                plew.WriteInt(c.Character.CharacterID);
                plew.WriteInt(level);
                c.Send(plew);
            }
        }

        public static void updateStat(Client c)
        {
            using (OutPacket plew = new OutPacket(ServerOpcode.CHAR_STATUP_ACK))
            {
                var chr = c.Character;
                plew.WriteInt(0); // length + CRC
                plew.WriteInt(0);
                plew.WriteShort(chr.MaxHp);
                plew.WriteShort(chr.MaxMp);
                plew.WriteShort(chr.Str);
                plew.WriteShort(chr.Dex);
                plew.WriteShort(chr.Vit);
                plew.WriteShort(chr.Int);
                plew.WriteShort(chr.MaxAttack);
                plew.WriteShort(chr.Attack);
                plew.WriteShort(chr.MaxMagic);
                plew.WriteShort(chr.Magic);
                plew.WriteShort(chr.Defense);
                plew.WriteByte(3); // 攻擊速度
                plew.WriteByte(0);
                plew.WriteShort(0);
                plew.WriteShort(chr.AbilityBonus);
                plew.WriteShort(chr.SkillBonus);
                plew.WriteShort(chr.UpgradeStr);
                plew.WriteShort(chr.UpgradeDex);
                plew.WriteShort(chr.UpgradeVit);
                plew.WriteShort(chr.UpgradeInt);
                plew.WriteShort(chr.UpgradeAttack);
                plew.WriteShort(chr.UpgradeMagic);
                plew.WriteShort(chr.UpgradeDefense);
                c.Send(plew);
            }
        }
    }
}
