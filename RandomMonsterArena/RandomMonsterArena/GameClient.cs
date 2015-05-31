using System;
using System.Threading;
using System.Collections.Generic;
using Lidgren.Network;
using Lidgren.Network.Xna;
using Microsoft.Xna.Framework;


namespace RandomMonsterArena
{
    public class GameClient
    {
        #region data members
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
        private static NetClient client;
        private static Thread clientThread;
        private static Player loggedInPlayer;
        private static Player opponentPlayer;
        public bool isGameStarted = false;        
        private static List<AbilityInput> opponentInputs;
        #endregion

        #region Properties
        public Player LoggedInPlayer
        {
            get { return loggedInPlayer; }
        }
        public Player OpponentPlayer
        {
            get { return opponentPlayer; }
        }
        public bool IsGameJoined
        {
            get
            {
                if (opponentPlayer != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion

        #region methods
        public GameClient(string hostIP, Player loggedInPlayer)
        {
            opponentInputs = new List<AbilityInput>();
            GameClient.loggedInPlayer = loggedInPlayer;
            NetPeerConfiguration config = new NetPeerConfiguration("Summoners Academy");
            client = new NetClient(config);
            client.Start();            
            if (hostIP != "")
            {
                NetOutgoingMessage outTrans = client.CreateMessage();
                outTrans.Write((Byte)PacketTypes.ConnectionRequest);
                Game1.multiplayerLobbyTest.text += "attempting connection to " + hostIP;
                client.Connect(hostIP, Constant.o_networkPort, outTrans);
            }
            else
            {
                client.DiscoverLocalPeers(Constant.o_networkPort);
            }
            clientThread = new Thread(HandleMessages);
            clientThread.Start();
        }
        private void HandleMessages()
        {
            while (true)
            {
                NetIncomingMessage msg;
                while ((msg = client.ReadMessage()) != null)
                {
                    switch (msg.MessageType)
                    {
                        case NetIncomingMessageType.DiscoveryResponse:
                            NetOutgoingMessage outTrans = client.CreateMessage();
                            outTrans.Write((Byte)PacketTypes.ConnectionRequest);
                            client.Connect(msg.SenderEndPoint,outTrans);
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
                                NetOutgoingMessage playerStatusMsg = client.CreateMessage();
                                playerStatusMsg.Write((Byte)PacketTypes.PlayerLogin);
                                playerStatusMsg.Write(loggedInPlayer.playerName);
                                client.SendMessage(playerStatusMsg, NetDeliveryMethod.ReliableOrdered);
                                //Game1.multiplayerLobbyTest.text += "\nclient connected";
                                //Console.WriteLine(NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier) + " connected!");
                            }
                            break;
                        case NetIncomingMessageType.Data:
                            //Game1.multiplayerLobbyTest.text += "\nReceiving incoming transmission.";
                            switch (msg.ReadByte())
                            {
                                case (Byte)PacketTypes.PlayerLogin:
                                    loggedInPlayer.playerID = msg.ReadInt32();                                    
                                    if (loggedInPlayer.playerID == 1)
                                    {
                                        loggedInPlayer.playerColor = Constant.player1Color;
                                    }
                                    else
                                    {
                                        loggedInPlayer.playerColor = Constant.player2Color;
                                    }
                                    Game1.multiplayerLobbyTest.text += "\nMe = " + loggedInPlayer.playerName + " " + loggedInPlayer.playerColor.ToString();
                                    if (msg.ReadBoolean())
                                    {
                                        int oppId = msg.ReadInt32();
                                        String oppName = msg.ReadString();
                                        opponentPlayer = new Player(oppName, oppId, (oppId == 1 ? Constant.player1Color : Constant.player2Color));
                                        Game1.multiplayerLobbyTest.text += "\nOther = " + opponentPlayer.playerName + " " + opponentPlayer.playerID.ToString();
                                    }
                                    break;
                                case (Byte)PacketTypes.PlayerJoinedGameUpdate:
                                    int oppId2 = msg.ReadInt32();
                                    String oppName2 = msg.ReadString();
                                    opponentPlayer = new Player(oppName2, oppId2, (oppId2 == 1 ? Constant.player1Color : Constant.player2Color));
                                    Game1.multiplayerLobbyTest.text += "\nOther = " + opponentPlayer.playerName + " " + opponentPlayer.playerID.ToString();
                                    break;
                                case (Byte)PacketTypes.DiceConfiguration:
                                    int playerID = msg.ReadInt32();
                                    int diceCount = msg.ReadInt32();
                                    Player ownedByPlayer = null;
                                    if (LoggedInPlayer.playerID == playerID)
                                    {
                                        ownedByPlayer = LoggedInPlayer;
                                    }
                                    else if (OpponentPlayer.playerID == playerID)
                                    {
                                        ownedByPlayer = OpponentPlayer;
                                    }
                                    for (int i = 0; i < diceCount; i++)
                                    {
                                        Diceman.Class diceClass = (Diceman.Class)msg.ReadByte();
                                        int diceID = msg.ReadInt32();
                                        float diceHp = msg.ReadFloat();
                                        float diceStr = msg.ReadFloat();
                                        float diceSpd = msg.ReadFloat();
                                        Vector2 dicePos = msg.ReadVector2();
                                        String diceAbility = msg.ReadString();
                                        Diceman newDice = new Diceman(diceClass, ownedByPlayer, diceAbility, diceID);
                                        ownedByPlayer.ownedDicemen.Add(newDice);
                                        while (BattleBoard.board == null) ;
                                        newDice.Summon(dicePos, diceHp, diceStr, diceSpd);
                                        newDice.sprite.tintColor = ownedByPlayer.playerColor;
                                    }
                                    break;
                                case (Byte)PacketTypes.GameStartSync:
                                    isGameStarted = true;
                                    break;
                                case (Byte)PacketTypes.Disconnecting:
                                    StopClient();
                                    break;
                                case (Byte)PacketTypes.MovesList:
                                    Game1.multiplayerLobbyTest.text += "\nGot opponent moves list.!!!!!!!!!!!!!!!";
                                    int count = msg.ReadInt32();
                                    List<Diceman> diceInGame = new List<Diceman>();
                                    diceInGame.AddRange(LoggedInPlayer.ownedDicemen);
                                    diceInGame.AddRange(OpponentPlayer.ownedDicemen);
                                    for (int i = 0; i < count; i++)
                                    {
                                        int selectedDiceID = msg.ReadInt32();
                                        String selectedSpellName = msg.ReadString();
                                        int targetedDiceID = -1;
                                        if (msg.ReadBoolean())
                                        {
                                            targetedDiceID = msg.ReadInt32();
                                        }
                                        Vector2 fireLoc = msg.ReadVector2();
                                        Diceman selectedDice = null;
                                        Diceman targetedDice = null;
                                        foreach (Diceman dice in diceInGame)
                                        {
                                            if (dice.DiceID == selectedDiceID)
                                            {
                                                selectedDice = dice;                                                
                                            }
                                            if (dice.DiceID == targetedDiceID)
                                            {
                                                targetedDice = dice;
                                            }
                                        }
                                        Ability selectedSpell = null;
                                        foreach (Ability spell in selectedDice.activeAbilities)
                                        {
                                            if (spell.name == selectedSpellName)
                                            {
                                                selectedSpell = spell;                                                
                                            }
                                        }
                                        opponentInputs.Add(new AbilityInput(selectedDice, selectedSpell, targetedDice, fireLoc));
                                    }
                                    break;
                                default:
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
        public List<AbilityInput> GetOpponentMoves()
        {
            if (opponentInputs.Count > 0)
            {
                return opponentInputs;
            }
            else
            {
                return null;
            }
        }
        public void FlushOpponentMoves()
        {
            opponentInputs.Clear();
        }
        public void SendPlayerMoves(List<AbilityInput> playerMoves)
        {
            NetOutgoingMessage outTrans = client.CreateMessage();
            outTrans.Write((Byte)PacketTypes.MovesList);
            outTrans.Write(playerMoves.Count);
            for (int i = 0; i < playerMoves.Count; i++)
            {
                outTrans.Write(playerMoves[i].selectedDice.DiceID);
                outTrans.Write(playerMoves[i].selectedSpell.name);
                if (playerMoves[i].targetedDice != null)
                {
                    outTrans.Write(true);
                    outTrans.Write(playerMoves[i].targetedDice.DiceID);
                }
                else
                {
                    outTrans.Write(false);
                }
                outTrans.Write(playerMoves[i].fireLocation);
            }
            client.SendMessage(outTrans, NetDeliveryMethod.ReliableOrdered);
        }
        public void SendDiceConfiguration(Diceman.Class[] diceConfig)
        {
            NetOutgoingMessage outTrans = client.CreateMessage();
            outTrans.Write((Byte)PacketTypes.DiceConfiguration);
            outTrans.Write(LoggedInPlayer.playerID);
            outTrans.Write(diceConfig.Length);
            for (int i = 0; i < diceConfig.Length; i++)
            {
                outTrans.Write((Byte)diceConfig[i]);
            }
            client.SendMessage(outTrans, NetDeliveryMethod.ReliableOrdered);
        }
        public void SendGameStartMsg()
        {            
            NetOutgoingMessage outTrans = client.CreateMessage();
            outTrans.Write((Byte)PacketTypes.GameStartSync);
            client.SendMessage(outTrans, NetDeliveryMethod.ReliableOrdered);
        }
        public void StopClient()
        {
            loggedInPlayer = null;
            opponentInputs = null;
            opponentPlayer = null;
            isGameStarted = false;
            NetOutgoingMessage outTrans = client.CreateMessage();
            outTrans.Write((Byte)PacketTypes.Disconnecting);
            client.SendMessage(outTrans, NetDeliveryMethod.ReliableOrdered);
            clientThread.Abort();
            clientThread = null;
            client.Shutdown("Sayonara Server!");
            client = null;
        }
        #endregion
    }
}

