﻿using Server.Ghost.Characters;
using Server.Common.Data;
using System;

namespace Server.Ghost
{
    public class Item
    {
        public CharacterItems Parent { get; set; }

        public int ID { get; private set; }
        public int ItemID { get; private set; }
        private short maxPerStack;
        private short quantity;
        public byte type { get; set; }
        public byte slot { get; set; }

        public bool Assigned { get; set; }

        public Character Character
        {
            get
            {
                try
                {
                    return this.Parent.Parent;
                }
                catch
                {
                    return null;
                }
            }
        }

        public short MaxPerStack
        {
            get
            {
                if (maxPerStack == 0)
                {
                    maxPerStack = 100;
                }
                return maxPerStack;
            }
            set
            {
                maxPerStack = value;
            }
        }

        public short Quantity
        {
            get
            {
                return quantity;
            }
            set
            {
                if (value > this.MaxPerStack)
                {
                    throw new ArgumentException("Quantity too high.");
                }
                else
                {
                    quantity = value;
                }
            }
        }

        public Item(int itemID, byte slot, byte type, short quantity = 1)
        {
            this.ItemID = itemID;
            this.MaxPerStack = 1;
            switch (type)
            {
                case 0:
                case 1:
                case 2:
                case 5:
                    this.MaxPerStack = 1;
                    break;
                case 3:
                case 4:
                    this.MaxPerStack = 100;
                    break;
                case 0x63:
                    this.MaxPerStack = Int16.MaxValue;
                    break;
            }
            this.Quantity = quantity;
            this.slot = slot;
            this.type = type;
        }

        public Item(dynamic datum)
        {
            this.ID = datum.id;
            this.Assigned = true;

            this.ItemID = datum.itemId;
            this.MaxPerStack = 1;
            switch ((byte)(datum.type))
            {
                case 0:
                case 1:
                case 2:
                case 5:
                    this.MaxPerStack = 1;
                    break;
                case 3:
                case 4:
                    this.MaxPerStack = 100;
                    break;
            }
            this.Quantity = (short)datum.quantity;
            this.slot = (byte)datum.slot;
            this.type = (byte)datum.type;
        }

        public void Save()
        {
            dynamic datum = new Datum("Items");

            datum.cid = this.Character.ID;
            datum.itemId = this.ItemID;
            datum.quantity = this.Quantity;
            datum.slot = this.slot;
            datum.type = this.type;

            if (this.Assigned)
            {
                datum.Update("id = '{0}'", this.ID);
            }
            else
            {
                datum.Insert();

                this.ID = Database.Fetch("Items", "id", "cid = '{0}' && itemId = '{1}' && quantity = '{2}' && slot = '{3}' && type = '{4}'", this.Character.ID, this.ItemID, this.quantity, this.slot, this.type);

                this.Assigned = true;
            }
        }

        public void Delete()
        {
            Database.Delete("Items", "id = '{0}'", this.ID);

            this.Assigned = false;
        }
    }
}
