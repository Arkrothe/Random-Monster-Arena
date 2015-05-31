using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RandomMonsterArena
{
    public class Move : Ability
    {
        public Move(int range, float damage, float speed, String abilityDescription, Sprite icon)
            : base(range, damage, speed, true, abilityDescription, icon)
        {
            name = "MOVE";
        }

        public override bool Target(Diceman user, Diceman target, Vector2 firedBoardLocation)
        {
            if (BattleBoard.GetBoardTile(firedBoardLocation) != null &&
                BattleBoard.GetBoardTile(firedBoardLocation).occupyingDiceman == null &&
                user.isAlive)
            {
                return true;                
            }
            String failString = "";
            if (user.isAlive == false)
            {
                failString = " as " + user.name + " is dead.";
            }
            else if (BattleBoard.GetBoardTile(firedBoardLocation).occupyingDiceman != null)
            {
                failString = " as position is occupied.";
            }
            usageDescription = name + " failed" + failString;
            return false;
        }

        public override void Use(Diceman user, Diceman target, Vector2 firedBoardLocation)
        {
            foreach (Effect debuff in user.activeEffects)
            {
                if (!debuff.AllowedDiceAbilities()[0])
                {
                    usageDescription = user.ownedByPlayer.playerName + "'s " + user.name + " is disabled!";
                    return;
                }
            }
            user.Move(firedBoardLocation);
            usageDescription = user.ownedByPlayer.playerName + " moved a " + user.name + ".";
        }
    }
}
