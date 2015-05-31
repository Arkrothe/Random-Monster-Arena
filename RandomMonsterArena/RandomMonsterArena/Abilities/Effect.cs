using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RandomMonsterArena
{
    public abstract class Effect : Ability
    {
        public int turnDuration;
        public int remainingDuration;
        public bool isStackable;

        protected Effect(int range, int turnDuration, float damage, float speed, bool isLocationBased, bool isStackable, Sprite icon) :
            base(range, damage, speed, isLocationBased, "", icon)
        {
            this.turnDuration = turnDuration;
            remainingDuration = turnDuration;
            this.isStackable = isStackable;
        }

        /// <summary>
        /// Tells whether the dice having an ability can use it or not.
        /// </summary>
        /// <returns>Returns an array of bool, in which 0 - move, 1 - basic attack, 2 - special ability.</returns>
        public abstract bool[] AllowedDiceAbilities();        
    }
}
