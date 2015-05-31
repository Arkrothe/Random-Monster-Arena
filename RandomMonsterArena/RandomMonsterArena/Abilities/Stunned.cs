using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RandomMonsterArena
{
    public class Stunned : Effect
    {
        public Stunned(int range, int turnDuration, float damage, float speed, Sprite icon)
            : base(range, turnDuration, damage, speed, false, false, icon)
        {
            name = Constant.e_stunName;            
        }

        public override bool Target(Diceman user, Diceman target, Vector2 firedBoardLocation)
        {
            if (target != null && target.isAlive)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool[] AllowedDiceAbilities()
        {
            if (remainingDuration > 0)
            {
                return new bool[] { false, false, false };
            }
            else
            {
                return new bool[] { true, true, true };
            }
        }

        public override void Use(Diceman user, Diceman target, Vector2 firedBoardLocation)
        {
            remainingDuration = Math.Max(0, remainingDuration - 1);
            if (remainingDuration > 0)
            {
                usageDescription = target.ownedByPlayer.playerName + "'s " + target.name + " is still " + Constant.e_stunPastName + ".";
            }
            else
            {
                usageDescription = "The " + name + " wore off!";
            }
        }
    }
}
