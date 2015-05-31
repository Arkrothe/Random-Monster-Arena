using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RandomMonsterArena
{
    public static class Constant
    {
        #region Other Floats and Ints
        public const float o_spellUsageDisplayDelay = 5f;
        public const int o_roundToDigits = 2;
        public const int o_movesPerTurn = 3;
        public const int o_networkPort = 42001;
        public const int o_boardSize = 12;
        #endregion

        #region Layers
        public const float l_abilityIcon = 0.41f;
        public const float l_dicemen = 0.45f;
        public const float l_floorTile = 0.5f;
        public const float l_diceDescript = 0.3f;
        #endregion

        #region Locations
        public static Vector2 v_diceDescript = new Vector2(10, 10);
        public static Vector2 v_abilityDescriptOffset = new Vector2(-400, -80);
        #endregion

        #region Colors
        public static Color player1Color = Color.LightBlue;
        public static Color player1ColorHighlighted = Color.BlueViolet;
        public static Color player2Color = Color.IndianRed;
        public static Color player2ColorHighlighted = Color.Red;
        #endregion

        #region Abilites and Effects
        public const int e_stunDuration = 2; //only one actual turn.
        public const float e_stunSpeed = 0.9f;
        public static Sprite e_stunIcon;
        public const String e_stunName = "STUN";
        public const String e_stunPastName = "STUNNED";
        public const float a_bashDamage = 0.7f;
        public const float a_bashSpeed = 0.9f;
        public const String a_bashDescript = "Attacks enemy with ferocious strength, \nstunning and dealing 0.7x damage. \nHas average speed.";
        public const String a_bashName = "BASH";

        public const float a_biteDamage = 0.4f;
        public const float a_biteSpeed = 1f;
        public const String a_biteDescript = "Bites an enemy, stealing life. \nSteals 0.4x life from target. \nHas average speed.";
        public const String a_biteName = "BITE";

        public const int a_chargeRange = 3;
        public const float a_chargeSpeed = 0.3f;
        public const float a_chargeDamage = 0.5f;
        public const String a_chargeDescript = "Charges at an enemy, moving near it, \nand dealing 0.5x damage. \nHas low speed.";
        public const String a_chargeTargetSurrounded = "CHARGE failed! Target is surrounded.";
        public const String a_chargeName = "CHARGE";

        public const int a_vaultRange = 1;
        public const float a_vaultSpeed = 0.8f;
        public const float a_vaultDamage = 0.4f;
        public const String a_vaultDescript = "Pounces an enemy, damaging it and \nthen jumps to target location.";
        public const String a_vaultName = "VAULT ATTACK";

        public const int e_poisonDuration = 4; //lasts for 3 turns.
        public const float e_poisonSpeed = 0.8f;
        public const double a_poisonDisableChance = 0.5;
        public const float a_poisonDamage = 0.2f;   
        public static Sprite e_poisonIcon;
        public const String e_poisonName = "POISON";
        public const String e_poisonPastName = "POISONED";
        
        public const int a_poisonAttackRange = 1;
        public const float a_poisonAttackSpeed = 0.8f;             
        public const String a_poisonAttackDescript = "Poisons an enemy for 3 turns, \nDeals damage each turn and has a \nchance to disable abilities.";
        public const String a_poisonAttackName = "POISON ATTACK";

        public const String a_multishotName = "MULTISHOT";
        public const String a_multishotDescript = "Attacks upto 3 nearby enemies, \ndealing 0.5x damage to each target. \nHas average speed.";
        public const float a_multishotDamage = 0.5f;
        public const float a_multishotSpeed = 0.95f;
        public const int a_multishotRange = 3;
        public const int a_multishotHits = 3;

        public const String a_basicAttackDescript = "Attacks enemy dealing 1.0x damage. \nHas average speed.";
        public const String a_moveDescript = "Moves creature to targeted location. \nHas average speed.";
        #endregion

        #region Dicemen Stats
        public static Dictionary<Diceman.Class, float> d_baseHealths = new Dictionary<Diceman.Class, float>
        {
            {Diceman.Class.Bruiser, 6},
            {Diceman.Class.Ranger, 4}, 
            {Diceman.Class.Mage, 4},
        };
        public static Dictionary<Diceman.Class, float> d_baseStrengths = new Dictionary<Diceman.Class, float>
        {
            {Diceman.Class.Bruiser, 6},
            {Diceman.Class.Ranger, 6}, 
            {Diceman.Class.Mage, 4},
        };
        public static Dictionary<Diceman.Class, float> d_baseSpeeds = new Dictionary<Diceman.Class, float>
        {
            {Diceman.Class.Bruiser, 6},
            {Diceman.Class.Ranger, 8}, 
            {Diceman.Class.Mage, 4},
        };
        public const int d_statAllocationPoints = 10;
        #endregion

        #region Textures and accompanying stuff
        public static Texture2D t_icons;
        public static Rectangle t_SourceRectangle(int index)
        {
            return new Rectangle((index % 10) * 32, (index / 10) * 32, 32, 32);
        }
        #endregion
    }
}
