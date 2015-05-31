using System;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Lidgren.Network;
using Lidgren.Network.Xna;

namespace RandomMonsterArena
{
    public class GameHost
    {
        enum PacketTypes
        {
            ConnectionRequest,
            PlayerLogin,
            PlayerJoinedGameUpdate,
            DiceConfiguration,
            GameStartSync,
            MovesList,
            Disconnecting
        }
        private static Thread serverThread;
        private static NetServer server;
        private static NetPeerConfiguration config;
        private static int idCount = 1;
        private static List<Player> connectedPlayers;
        private static List<Vector2> occupiedDicePositions;

        public GameHost()
        {
            occupiedDicePositions = new List<Vector2>();
            connectedPlayers = new List<Player>();
            config = new NetPeerConfiguration("Summoners Academy");
            config.Port = Constant.o_networkPort;
            config.EnableUPnP = true;
            config.MaximumConnections = 2;
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            server = new NetServer(config);
            server.Start();
            bool x = server.UPnP.ForwardPort(Constant.o_networkPort, "Summoners Academy");
            Game1.multiplayerLobbyTest.text += x;            
            serverThread = new Thread(HandleMessages);
            serverThread.Start();
        }
        private void HandleMessages()
        {
            while (true)
            {
                NetIncomingMessage msg;
                while ((msg = server.ReadMessage()) != null)
                {
                    switch (msg.MessageType)
                    {
                        case NetIncomingMessageType.DiscoveryRequest:                            
                            server.SendDiscoveryResponse(null, msg.SenderEndPoint);
                            break;
                        case NetIncomingMessageType.ConnectionApproval:
                            if (msg.ReadByte() == (Byte)PacketTypes.ConnectionRequest)
                            {
                                //Game1.multiplayerLobbyTest.text += "\nclient connecting";
                                msg.SenderConnection.Approve();
                            }
                            break;
                        case NetIncomingMessageType.VerboseDebugMessage:
                        case NetIncomingMessageType.DebugMessage:
                        case NetIncomingMessageType.WarningMessage:
                        case NetIncomingMessageType.ErrorMessage:                            
                            Game1.multiplayerLobbyTest.text += msg.ReadString();
                            break;
                        case NetIncomingMessageType.StatusChanged:
                            NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                            if (status == NetConnectionStatus.Connected)
                            {                                
                                //Console.WriteLine(NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier) + " connected!");
                            }
                            break;
                        case NetIncomingMessageType.Data:
                            switch (msg.ReadByte())
                            {
                                case (Byte)PacketTypes.PlayerLogin:
                                    UpdatePlayerLogin(msg);
                                    break;
                                case (Byte)PacketTypes.DiceConfiguration:
                                    UpdateDiceConfiguration(msg);
                                    break;
                                case (Byte)PacketTypes.GameStartSync:
                                    NetOutgoingMessage outTrans = server.CreateMessage();
                                    outTrans.Write((Byte)PacketTypes.GameStartSync);
                                    server.SendToAll(outTrans, NetDeliveryMethod.ReliableOrdered);
                                    break;
                                case (Byte)PacketTypes.Disconnecting:
                                    NetOutgoingMessage discmsg = server.CreateMessage();
                                    discmsg.Write((Byte)PacketTypes.Disconnecting);
                                    server.SendToAll(discmsg, NetDeliveryMethod.ReliableOrdered);
                                    break;
                                case (Byte)PacketTypes.MovesList:
                                    Game1.multiplayerLobbyTest.text += "\ntransferring opponent moves list.";
                                    NetOutgoingMessage outmsg = server.CreateMessage();
                                    outmsg.Write((Byte)PacketTypes.MovesList);
                                    int count = msg.ReadInt32();
                                    outmsg.Write(count);
                                    for(int i=0; i<count; i++)
                                    {
                                        int selectedDiceID = msg.ReadInt32();
                                        String selectedSpellName = msg.ReadString();
                                        outmsg.Write(selectedDiceID);
                                        outmsg.Write(selectedSpellName);
                                        if (msg.ReadBoolean())
                                        {
                                            int targetedDiceID = msg.ReadInt32();
                                            outmsg.Write(true);
                                            outmsg.Write(targetedDiceID);
                                        }
                                        else
                                        {
                                            outmsg.Write(false);
                                        }
                                        outmsg.Write(msg.ReadVector2());
                                    }
                                    foreach (NetConnection con in server.Connections)
                                    {
                                        //Game1.multiplayerLobbyTest.text += con.Tag.ToString();
                                        if (con != msg.SenderConnection)
                                        {
                                            server.SendMessage(outmsg, con, NetDeliveryMethod.ReliableOrdered);
                                            //Game1.multiplayerLobbyTest.text += "\nsent msg";
                                        }
                                    }
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                }                                
                Thread.Sleep(1);
            }            
        }
        private void UpdatePlayerLogin(NetIncomingMessage msg)
        {
            Color playerColor = Constant.player2Color;
            if (connectedPlayers.Count == 0)
            {
                playerColor = Constant.player1Color;
            }
            String playerName = msg.ReadString();
            connectedPlayers.Add(new Player(playerName, idCount, playerColor));
            msg.SenderConnection.Tag = idCount;
            NetOutgoingMessage playerLogInData = server.CreateMessage();
            playerLogInData.Write((Byte)PacketTypes.PlayerLogin);
            playerLogInData.Write(idCount);
            if (connectedPlayers.Count == 2)
            {
                playerLogInData.Write(true);
                foreach (Player player in connectedPlayers)
                {
                    if (player.playerID != idCount)
                    {
                        playerLogInData.Write(player.playerID);
                        playerLogInData.Write(player.playerName);
                    }
                }
                server.SendMessage(playerLogInData, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                NetOutgoingMessage playerJoinedGameUpdate = server.CreateMessage();
                playerJoinedGameUpdate.Write((Byte)PacketTypes.PlayerJoinedGameUpdate);
                playerJoinedGameUpdate.Write(idCount);
                playerJoinedGameUpdate.Write(playerName);
                foreach (NetConnection conn in server.Connections)
                {
                    if ((int)conn.Tag != idCount)
                    {
                        server.SendMessage(playerJoinedGameUpdate, conn, NetDeliveryMethod.ReliableOrdered);
                    }
                }
            }
            else
            {
                playerLogInData.Write(false);
                server.SendMessage(playerLogInData, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);
            }
            idCount++;
        }
        private void UpdateDiceConfiguration(NetIncomingMessage msg)
        {                       
            int playerID = msg.ReadInt32();            
            int diceCount = msg.ReadInt32();                                    
            NetOutgoingMessage outTrans = server.CreateMessage(); 
            outTrans.Write((Byte)PacketTypes.DiceConfiguration);
            outTrans.Write(playerID); 
            outTrans.Write(diceCount);
            for (int i = 0; i < diceCount; i++)
            {
                float randomDivider1 = RandomGenerator.RandomValue(0f, 10f);
                float randomDivider2 = RandomGenerator.RandomValue(0f, 10f);
                Diceman.Class diceClass = (Diceman.Class)msg.ReadByte();                
                float health = Constant.d_baseHealths[diceClass] + Math.Min(randomDivider1, randomDivider2);
                float strength = Constant.d_baseStrengths[diceClass] + Math.Max(randomDivider1, randomDivider2) - Math.Min(randomDivider1, randomDivider2);
                float speed = Constant.d_baseSpeeds[diceClass] + 10f - Math.Max(randomDivider1, randomDivider2);
                Vector2 dicePosition = Vector2.Zero;
                int startYIndex = 0;
                int endYIndex = (int)(0.6f * Constant.o_boardSize);
                if (playerID == 2)
                {
                    startYIndex = (int)(0.4f * Constant.o_boardSize);
                    endYIndex = Constant.o_boardSize;
                }
                bool positionAssigned=false;
                while (!positionAssigned)
                {
                    positionAssigned = true;
                    dicePosition.X = RandomGenerator.Random.Next(0, Constant.o_boardSize);
                    dicePosition.Y = RandomGenerator.Random.Next(startYIndex, endYIndex);
                    foreach (Vector2 vec in occupiedDicePositions)
                    {
                        if (dicePosition == vec)
                        {
                            positionAssigned = false;
                        }
                    }
                }
                String ability = "";
                double randomNo = RandomGenerator.Random.NextDouble();
                if (diceClass == Diceman.Class.Bruiser)
                {
                    if (randomNo <= 0.33)
                    {
                        ability = Constant.a_bashName;
                    }
                    else if (randomNo <= 0.66)
                    {
                        ability = Constant.a_biteName;
                    }
                    else
                    {
                        ability = Constant.a_chargeName;
                    }
                }
                else if (diceClass == Diceman.Class.Ranger)
                {
                    if (randomNo <= 0.33)
                    {
                        ability = Constant.a_multishotName;
                    }
                    else if (randomNo <= 0.66)
                    {
                        ability = Constant.a_poisonAttackName;
                    }
                    else
                    {
                        ability = Constant.a_vaultName;
                    }
                }
                else if (diceClass == Diceman.Class.Mage)
                {
                }
                occupiedDicePositions.Add(dicePosition);
                outTrans.Write((Byte)diceClass);
                outTrans.Write(idCount++);
                outTrans.Write(health);
                outTrans.Write(strength);
                outTrans.Write(speed);
                outTrans.Write(dicePosition);
                outTrans.Write(ability);
            }
            server.SendToAll(outTrans, NetDeliveryMethod.ReliableOrdered);            
        }
        public void StopServer()
        {
            NetOutgoingMessage outTrans = server.CreateMessage();
            outTrans.Write((Byte)PacketTypes.Disconnecting);
            server.SendToAll(outTrans, NetDeliveryMethod.ReliableOrdered);
            idCount = 1;
            serverThread.Abort();
            server.Shutdown("Ciao folks!");
            server = null;
            serverThread = null;
            config = null;
            connectedPlayers = null;
            occupiedDicePositions = null;
        }        
    }
}
