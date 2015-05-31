using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RandomMonsterArena
{
    public class Charge : Ability
    {
        public Charge(int range, float damage, float speed, String abilityDescription, Sprite icon)
            : base(range, damage, speed, false, abilityDescription, icon)
        {
            name = Constant.a_chargeName;
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
                if (!debuff.AllowedDiceAbilities()[2])
                {
                    usageDescription = user.ownedByPlayer.playerName + "'s " + user.name + " is disabled!";
                    return;
                }
            }
            if (target.isAlive)
            {
                Vector2 moveToLoc = Vector2.Zero;
                List<Vector2> locsNearTarget = new List<Vector2>();
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (!(i == 0 && j == 0))
                        {
                            locsNearTarget.Add(new Vector2(target.BoardLocX + i, target.BoardLocY + j));
                        }
                    }
                }
                float minDist = BattleBoard.GetRealDistanceBetweenTileLocations(new Vector2(0,0), new Vector2(BattleBoard.board.GetLength(0), BattleBoard.board.GetLength(1)));
                for (int i = 0; i < locsNearTarget.Count; i++)
                {
                    if (BattleBoard.GetRealDistanceBetweenTileLocations(user.BoardLocation, locsNearTarget[i]) < minDist)
                    {
                        minDist = BattleBoard.GetRealDistanceBetweenTileLocations(user.BoardLocation, locsNearTarget[i]);
                        moveToLoc = locsNearTarget[i];
                    }
                }
                locsNearTarget.Remove(moveToLoc); 
                while (user.BoardLocation!=moveToLoc && !user.Move(moveToLoc))
                {
                    if (locsNearTarget.Count != 0)
                    {
                        minDist = BattleBoard.board.Length;
                        for (int i = 0; i < locsNearTarget.Count; i++)
                        {
                            if (BattleBoard.GetRealDistanceBetweenTileLocations(user.BoardLocation, locsNearTarget[i]) < minDist)
                            {
                                minDist = BattleBoard.GetRealDistanceBetweenTileLocations(user.BoardLocation, locsNearTarget[i]);
                                moveToLoc = locsNearTarget[i];
                            }
                        }
                        locsNearTarget.Remove(moveToLoc);
                    }
                    else
                    {
                        usageDescription = Constant.a_chargeTargetSurrounded;
                        return;
                    }
                }
                float damageDealt = user.strength * damage;
                usageDescription = user.ownedByPlayer.playerName + "'s " + user.name + " charged and\n";
                usageDescription += target.DamageDice(damageDealt);                
                usageDescription += ".";                
            }
            else
            {
                usageDescription = "Target is already dead.";
            }
        }
    }
}
