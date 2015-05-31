using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RandomMonsterArena
{
    public class VaultAttack : Ability
    {
        public VaultAttack(int range, float damage, float speed, String abilityDescription, Sprite icon)
            : base(range, damage, speed, true, abilityDescription, icon)
        {
            name = Constant.a_vaultName;
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
                if (!debuff.AllowedDiceAbilities()[2])
                {
                    usageDescription = user.ownedByPlayer.playerName + "'s " + user.name + " is disabled!";
                    return;
                }
            }
            for (int i = -range; i <= range; i++)
            {
                for (int j = -range; j <= range; j++)
                {
                    if (BattleBoard.GetBoardTile(user.BoardLocX + i, user.BoardLocY + j) != null)
                    {
                        target = BattleBoard.GetBoardTile(user.BoardLocX + i, user.BoardLocY + j).occupyingDiceman;
                        if (target != null && target.isAlive && target.ownedByPlayer != user.ownedByPlayer)
                        {
                            float damageDealt = user.strength * damage;
                            usageDescription = target.DamageDice(damageDealt);
                            user.Move(firedBoardLocation);
                            usageDescription += " and \n" + user.ownedByPlayer.playerName + "'s " + user.name + " vaulted away.";
                            return;
                        }
                    }                                        
                }
            }
            usageDescription = name + " failed as no enemies \nwere in range.";
        }
    }
}