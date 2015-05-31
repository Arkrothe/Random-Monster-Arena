using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RandomMonsterArena
{
    public class Poisoned : Effect
    {
        public Poisoned(int range, int turnDuration, float damage, float speed, Sprite icon)
            : base(range, turnDuration, damage, speed, false, false, icon)
        {
            name = Constant.e_poisonName;            
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
                double randomNo = RandomGenerator.Random.NextDouble();
                if (randomNo <= Constant.a_poisonDisableChance)
                {
                    return new bool[] { true, false, false };
                }
            }
            return new bool[] { true, true, true };
        }

        public override void Use(Diceman user, Diceman target, Vector2 firedBoardLocation)
        {            
            remainingDuration = Math.Max(0, remainingDuration - 1);
            if (remainingDuration > 0 && target.isAlive)
            {
                float damageDealt = user.strength * damage;
                usageDescription = target.DamageDice(damageDealt);
                if (target.isAlive)
                {
                    usageDescription += " and is still " + Constant.e_poisonPastName + ".";
                }
            }
            if (remainingDuration == 0 && target.isAlive)
            {
                usageDescription += " and is no longer " + Constant.e_poisonPastName + ".";
            }
            if (!target.isAlive)
            {
                usageDescription += ".";
            }
        }
    }
}
