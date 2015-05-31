using System;
using Microsoft.Xna.Framework;
namespace RandomMonsterArena
{
    public class AbilityInput
    {
        public Diceman selectedDice;
        public Ability selectedSpell;
        public Vector2 fireLocation;
        public Diceman targetedDice;

        public AbilityInput()
        {
            selectedDice = null;
            selectedSpell = null;
            targetedDice = null;
            fireLocation = Vector2.Zero;
        }

        public AbilityInput(Diceman selectedDice, Ability selectedSpell, Diceman targetedDice, Vector2 fireLocation)
        {
            this.selectedDice = selectedDice;
            this.selectedSpell = selectedSpell;
            this.targetedDice = targetedDice;
            this.fireLocation = fireLocation;
        }
    }
}
