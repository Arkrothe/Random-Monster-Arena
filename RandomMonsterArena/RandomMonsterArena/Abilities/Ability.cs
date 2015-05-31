using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RandomMonsterArena
{
    public abstract class Ability
    {
        public int                  range;
        protected float             damage;
        public float                speed;                
        public bool                 isLocationBased;
        public Sprite               icon;
        public String               name;
        public String               abilityDescription;
        public String               usageDescription;        

        protected Ability(int range, float damage, float speed, bool isLocationBased, String abilityDescription, Sprite icon)
        {
            this.range = range;
            this.damage = damage;
            this.speed = speed;            
            this.icon = icon;
            this.isLocationBased = isLocationBased;
            this.abilityDescription = abilityDescription;
        }

        public abstract bool Target(Diceman user, Diceman target, Vector2 firedBoardLocation);

        public abstract void Use(Diceman user, Diceman target, Vector2 firedBoardLocation);                
    }
}
