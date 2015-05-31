using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RandomMonsterArena
{
    public class Diceman
    {
        #region Data members
        public enum Class { Bruiser, Ranger, Mage };
                
        private int                     diceID;
        public Sprite                   sprite;
        private float                   health;
        private float                   maxHealth;
        public float                    strength;
        public float                    speed;        
        public Class                    diceClass;
        public List<Ability>            activeAbilities;
        public List<Effect>             activeEffects;        
        public bool                     isOnBoard;
        public Player                   ownedByPlayer;   
        public String                   name;     
        
        #endregion

        #region Properties and helper methods        
        public Vector2 BoardLocation
        {
            get
            {
                return (isOnBoard ? new Vector2(
                    Math.Abs((sprite.Location.X - BattleBoard.topTileLoc.X) / 32),
                    Math.Abs((sprite.Location.Y - BattleBoard.topTileLoc.Y) / 32))
                    : new Vector2(-1, -1));
            }
            set 
            {
                sprite.Location = new Vector2( 
                    BattleBoard.topTileLoc.X + (value.X * 32),
                    BattleBoard.topTileLoc.Y + (value.Y * 32));
            }
        }
        public int BoardLocX
        {
            get { return (int)BoardLocation.X; }
        }
        public int BoardLocY
        {
            get { return (int)BoardLocation.Y; }
        }
        public bool isAlive
        {
            get { return (health <= 0 ? false : true); }
        }
        public float Health
        {
            get { return health; }            
        }
        private float ChangeHealth
        {
            set { health = MathHelper.Clamp(value, 0f, maxHealth); }
        }
        public float MaxHealth
        {
            get { return maxHealth; }
        }
        public int DiceID
        {
            get { return diceID; }
        }
        public String DamageDice(float damageDealt)
        {
            ChangeHealth = Health - damageDealt;
            if (isAlive)
            {
                return ownedByPlayer.playerName + "'s " + name + " took\n" + Math.Round(damageDealt, Constant.o_roundToDigits).ToString() + " damage";
            }
            else
            {
                return ownedByPlayer.playerName + "'s " + name + " took\n" + Math.Round(damageDealt, Constant.o_roundToDigits).ToString() + " damage and died";
            }
        }
        public String HealDice(float healthHealed)
        {
            ChangeHealth = Health + healthHealed;
            return ownedByPlayer.playerName + "'s " + name + " healed " + Math.Round(healthHealed, Constant.o_roundToDigits).ToString() + " life";
        }        
        public bool Move(Vector2 targetBoardLocation)
        {
            if (BattleBoard.GetBoardTile((int)targetBoardLocation.X, (int)targetBoardLocation.Y)==null)
            {
                return false;
            }
            if (BattleBoard.GetBoardTile((int)targetBoardLocation.X, (int)targetBoardLocation.Y).occupyingDiceman != null)
            {
                return false;
            }            
            if (BoardLocation.X != -1 && BoardLocation.Y != -1)
            {
                BattleBoard.board[BoardLocX, BoardLocY].occupyingDiceman = null;
            }
            else
            {
                isOnBoard = true;
            }
            Vector2 tempLoc = BoardLocation;
            BoardLocation = targetBoardLocation;
            if (BattleBoard.GetBoardTile(BoardLocX, BoardLocY) != null)
            {
                BattleBoard.board[BoardLocX, BoardLocY].occupyingDiceman = this;
            }
            else
            {
                BoardLocation = tempLoc;
                return false;
            }
            return true;
        }
        #endregion

        #region main methods
        public Diceman(Class diceClass, Player ownedByPlayer, String ability, int diceID)
        {
            isOnBoard = false;            
            this.diceClass = diceClass;
            this.diceID = diceID;
            this.ownedByPlayer = ownedByPlayer;
            name = diceClass.ToString();
            activeAbilities = new List<Ability>();
            activeEffects = new List<Effect>();
            activeAbilities.Add(new Move(1, 0f, 1f, Constant.a_moveDescript, new Sprite(
                Constant.t_icons,
                Constant.t_SourceRectangle(3),
                Vector2.Zero,
                Constant.l_abilityIcon,
                0f)));
            maxHealth = 0;
            if (diceClass == Class.Bruiser)
            {
                activeAbilities.Add(new BasicAttack(1, 1f, 1f, Constant.a_basicAttackDescript, new Sprite(
                    Constant.t_icons,
                    Constant.t_SourceRectangle(4),
                    Vector2.Zero,
                    Constant.l_abilityIcon,
                    0f)));
                if (ability == Constant.a_bashName)
                {
                    activeAbilities.Add(new Bash(1, Constant.a_bashDamage, Constant.a_bashSpeed, Constant.a_bashDescript,
                        new Sprite(
                            Constant.t_icons,
                            Constant.t_SourceRectangle(10),
                            Vector2.Zero,
                            Constant.l_abilityIcon,
                            0)));
                }
                else if (ability == Constant.a_chargeName)
                {
                    activeAbilities.Add(new Charge(Constant.a_chargeRange, Constant.a_chargeDamage, Constant.a_chargeSpeed, Constant.a_chargeDescript,
                        new Sprite(
                            Constant.t_icons,
                            Constant.t_SourceRectangle(12),
                            Vector2.Zero,
                            Constant.l_abilityIcon,
                            0)));
                }
                else if (ability == Constant.a_biteName)
                {
                    activeAbilities.Add(new Bite(1, Constant.a_biteDamage, Constant.a_biteSpeed, Constant.a_biteDescript,
                        new Sprite(
                            Constant.t_icons,
                            Constant.t_SourceRectangle(11),
                            Vector2.Zero,
                            Constant.l_abilityIcon,
                            0f)));
                }
                sprite = new Sprite(
                    Constant.t_icons,
                    Constant.t_SourceRectangle(0),
                    Vector2.Zero,
                    Constant.l_dicemen,
                    0f);
                health = 6;
                strength = 6;
                speed = 6;
            }
            else if (diceClass == Class.Ranger)
            {
                activeAbilities.Add(new BasicAttack(3, 1f, 1f, Constant.a_basicAttackDescript, new Sprite(
                    Constant.t_icons,
                    Constant.t_SourceRectangle(5),
                    Vector2.Zero,
                    Constant.l_abilityIcon,
                    0f)));                
                if (ability == Constant.a_vaultName)
                {
                    activeAbilities.Add(new VaultAttack(Constant.a_vaultRange, Constant.a_vaultDamage, Constant.a_vaultSpeed, Constant.a_vaultDescript,
                        new Sprite(
                            Constant.t_icons,
                            Constant.t_SourceRectangle(15),
                            Vector2.Zero,
                            Constant.l_abilityIcon,
                            0f)));
                }
                else if (ability == Constant.a_multishotName)
                {
                    activeAbilities.Add(new Multishot(Constant.a_multishotRange, Constant.a_multishotDamage, Constant.a_multishotSpeed, Constant.a_multishotDescript,
                        new Sprite(
                            Constant.t_icons,
                            Constant.t_SourceRectangle(13),
                            Vector2.Zero,
                            Constant.l_abilityIcon,
                            0f)));
                }
                else if (ability == Constant.a_poisonAttackName)
                {
                    activeAbilities.Add(new PoisonAttack(Constant.a_poisonAttackRange, 0, Constant.a_poisonAttackSpeed, Constant.a_poisonAttackDescript,
                        new Sprite(
                            Constant.t_icons,
                            Constant.t_SourceRectangle(14),
                            Vector2.Zero,
                            Constant.l_abilityIcon,
                            0f)));
                }
                sprite = new Sprite(
                    Constant.t_icons,
                    Constant.t_SourceRectangle(1),
                    Vector2.Zero,
                    Constant.l_dicemen,
                    0f);
                health = 4;
                strength = 6;
                speed = 8;
            }
            else if (diceClass == Class.Mage)
            {
                activeAbilities.Add(Game1.rangedAttack);
                sprite = new Sprite(
                    Constant.t_icons,
                    Constant.t_SourceRectangle(2),
                    Vector2.Zero,
                    Constant.l_dicemen,
                    0f);
                health = 4;
                strength = 6;
                speed = 4;

            }
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (isAlive)
            {
                sprite.Draw(spriteBatch);
            }
        }
        public void Highlight()
        {
            if (sprite.tintColor == Constant.player1Color)
            {
                sprite.tintColor = Constant.player1ColorHighlighted;
            }
            else if (sprite.tintColor == Constant.player2Color)
            {
                sprite.tintColor = Constant.player2ColorHighlighted;
            }
        }
        public void UnHighlight()
        {
            if (sprite.tintColor == Constant.player1ColorHighlighted)
            {
                sprite.tintColor = Constant.player1Color;
            }
            else if (sprite.tintColor == Constant.player2ColorHighlighted)
            {
                sprite.tintColor = Constant.player2Color;
            }
        }        
        //have to move this function into Battleboard class as it is not something a dice should care about but the board/summoner.
        public bool Summon(Vector2 summonLoc, float health, float strength, float speed)
        {
            this.health = health;
            maxHealth = health;
            this.strength = strength;
            this.speed = speed;

            return Move(summonLoc);            
        }
        #endregion
    }
}
