using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RandomMonsterArena
{
    public class Player
    {
        public List<Diceman>                ownedDicemen;
        public Color                        playerColor;
        public String                       playerName;
        public int                          playerID;

        public Player(String playerName)
        {
            this.playerColor = Color.White;
            this.playerName = playerName;
            ownedDicemen = new List<Diceman>();
        }

        public Player(String playerName, int playerID, Color playerColor)
        {
            this.playerID = playerID;
            this.playerName = playerName;
            this.playerColor = playerColor;
            ownedDicemen = new List<Diceman>();
        }
    }
}
