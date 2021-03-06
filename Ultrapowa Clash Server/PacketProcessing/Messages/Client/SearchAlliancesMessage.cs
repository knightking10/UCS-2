﻿/*
 * Program : Ultrapowa Clash Server
 * Description : A C# Writted 'Clash of Clans' Server Emulator !
 *
 * Authors:  Jean-Baptiste Martin <Ultrapowa at Ultrapowa.com>,
 *           And the Official Ultrapowa Developement Team
 *
 * Copyright (c) 2016  UltraPowa
 * All Rights Reserved.
 */

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers;
using UCS.Logic;
using UCS.PacketProcessing.Messages.Server;

namespace UCS.PacketProcessing.Messages.Client
{
    //Packet 14324
    internal class SearchAlliancesMessage : Message
    {
        #region Public Constructors

        public SearchAlliancesMessage(PacketProcessing.Client client, BinaryReader br) : base(client, br)
        {
        }

        #endregion Public Constructors

        #region Private Fields

        private const int m_vAllianceLimit = 60;
        private int m_vAllianceOrigin;
        private int m_vAllianceScore;
        private int m_vMaximumAllianceMembers;
        private int m_vMinimumAllianceLevel;
        private int m_vMinimumAllianceMembers;
        private string m_vSearchString;
        private byte m_vShowOnlyJoinableAlliances;
        private int m_vWarFrequency;

        #endregion Private Fields

        //00 00 00 03
        //61 61 61
        //00 00 00 01
        //00 00 00 00
        //00 00 00 01
        //00 00 00 29
        //00 00 07 D0
        //01
        //00 00 00 00 //???
        //00 00 00 06

        #region Public Methods

        public override void Decode()
        {
            using (var br = new BinaryReader(new MemoryStream(GetData())))
            {
                m_vSearchString = br.ReadScString();
                m_vWarFrequency = br.ReadInt32WithEndian();
                m_vAllianceOrigin = br.ReadInt32WithEndian();
                m_vMinimumAllianceMembers = br.ReadInt32WithEndian();
                m_vMaximumAllianceMembers = br.ReadInt32WithEndian();
                m_vAllianceScore = br.ReadInt32WithEndian();
                m_vShowOnlyJoinableAlliances = br.ReadByte();
                br.ReadInt32WithEndian();
                m_vMinimumAllianceLevel = br.ReadInt32WithEndian();
            }
        }

        public override void Process(Level level)
        {
            var alliances = ObjectManager.GetInMemoryAlliances();
            var joinableAlliances = new List<Alliance>();
            var i = 0;
            var j = 0;
            while (j < m_vAllianceLimit && i < alliances.Count)
            {
                if (alliances[i].GetAllianceMembers().Count != 0
                    && alliances[i].GetAllianceName().Contains(m_vSearchString))
                {
                    joinableAlliances.Add(alliances[i]);
                    j++;
                }
                i++;
            }
            joinableAlliances = joinableAlliances.ToList();

            var p = new AllianceListMessage(Client);
            p.SetAlliances(joinableAlliances);
            p.SetSearchString(m_vSearchString);
            PacketManager.ProcessOutgoingPacket(p);
        }

        #endregion Public Methods
    }
}