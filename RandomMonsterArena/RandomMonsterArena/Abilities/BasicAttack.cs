using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RandomMonsterArena
{
    public class BasicAttack : Ability
    {
        public BasicAttack(int range, float damage, float speed, String abilityDescription, Sprite icon)
            : base(range, damage, speed, false, abilityDescription, icon)
        {
            name = "ATTACK";
        }

        public override bool Target(Diceman user, Diceman target, Vector2 firedBoardLocation)
        {
            if (target != null &&
                target.ownedByPlayer != user.ownedByPlayer &&
                user.isAlive &&
                BattleBoard.GetBlockDistanceBetweenDice(user, target) <= range)
            {
                return true;
            }
            else
            {
                if (target != null)
                {
                    String failString = "";
                    if (user.isAlive == false)
                    {
                        failString = " as " + user.name + " is dead.";
                    }
                    else if (BattleBoard.GetBlockDistanceBetweenDice(user, target) > range)
                    {
                        failString = " as target is out of range.";
                    }
                    usageDescription = name + " failed" + failString;
                }
                return false;
            }
        }

        public override void Use(Diceman user, Diceman target, Vector2 firedBoardLocation)
        {
            foreach (Effect debuff in user.activeEffects)
            {
                if (!debuff.AllowedDiceAbilities()[1])
                {
                    usageDescription = user.ownedByPlayer.playerName + "'s " + user.name + " is disabled!";
                    return;
                }
            }
            if (target.isAlive)
            {
                float damageDealt = user.strength * damage;
                usageDescription = target.DamageDice(damageDealt);
                usageDescription += ".";
            }
            else
            {
                usageDescription = "Target is already dead.";
            }
        }
    }
}
