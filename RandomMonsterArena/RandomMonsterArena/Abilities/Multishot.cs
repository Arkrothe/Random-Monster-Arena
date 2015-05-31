using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RandomMonsterArena
{
    public class Multishot : Ability
    {
        public Multishot(int range, float damage, float speed, String abilityDescription, Sprite icon)
            : base(range, damage, speed, false, abilityDescription, icon)
        {
            name = Constant.a_multishotName;
        }

        public override bool Target(Diceman user, Diceman target, Vector2 firedBoardLocation)
        {
            if (target != null &&
                target.ownedByPlayer != user.ownedByPlayer &&
                user.isAlive)
            {
                return true;
            }
            else
            {                
                if (user.isAlive == false)
                {
                    String failString = "";
                    failString = " as " + user.name + " is dead.";
                    usageDescription = name + " failed" + failString;
                }                                
                return false;
            }
        }

        public override void Use(Diceman user, Diceman target, Vector2 firedBoardLocation)
        {
            foreach (Effect debuff in user.activeEffects)
            {
                if (!debuff.AllowedDiceAbilities()[2])
                {
                    usageDescription = user.ownedByPlayer.playerName + "'s " + user.name + " is disabled!";
                    return;
                }
            }
            int hitsCount = Constant.a_multishotHits;
            List<Diceman> diceHit = new List<Diceman>();
            if (target.isAlive && BattleBoard.GetBlockDistanceBetweenDice(user, target) <= range)
            {
                float damageDealt = user.strength * damage;
                usageDescription = target.DamageDice(damageDealt);                
                hitsCount--;
                diceHit.Add(target);
            }            
            for (int i = -range; i <= range; i++)
            {
                for (int j = -range; j <= range; j++)
                {
                    if (BattleBoard.GetBoardTile(user.BoardLocX + i, user.BoardLocY + j) != null && hitsCount > 0)
                    {
                        target = BattleBoard.GetBoardTile(user.BoardLocX + i, user.BoardLocY + j).occupyingDiceman;
                        foreach (Diceman dice in diceHit)
                        {
                            if (dice == target)
                            {
                                target = null;
                            }
                        }
                        if (target != null && target.isAlive && target.ownedByPlayer != user.ownedByPlayer &&
                            BattleBoard.GetBlockDistanceBetweenDice(user, target) <= range)
                        {
                            float damageDealt = user.strength * damage;
                            if (hitsCount == Constant.a_multishotHits)
                            {
                                usageDescription = "";
                            }
                            usageDescription += target.DamageDice(damageDealt);                            
                            hitsCount--;                                                        
                        }
                    }
                }
            }
            if (hitsCount == Constant.a_multishotHits)
            {
                usageDescription = name + " failed as no enemies \nwere in range.";
            }
            else
            {
                usageDescription += ".";
            }
        }
    }
}
