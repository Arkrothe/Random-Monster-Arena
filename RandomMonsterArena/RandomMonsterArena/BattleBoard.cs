using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RandomMonsterArena
{  
    public class BattleBoard
    {
        #region Data Members
        public enum State { Input, Waiting, Playing};
        public enum InputState { Waiting, DiceSelected, AbilitySelected }        
               
        private const int                       movesPerTurn = Constant.o_movesPerTurn;
        private static State                    currentState;
        private static InputState               inputState;
        private static int                      inputMoveIndex;
        private static int                      spellPlayingIndex;
        private static Player                   loggedInPlayer;
        private static Player                   opponentPlayer;
        private static List<AbilityInput>       abilityInputs;
        private static List<AbilityInput>       spellPlayList;
        public static List<AbilityInput>        activeEffects;

        public static FloorTile[,]              board;
        public static Vector2                   topTileLoc;        
        private const int                       abilityIconOffset = 32;
        private static List<FloorTile>          highLightedTiles;
        private static TextSprite               dicemanDescription;        
        private static TextSprite               abilityDescription;
        private static float                    spellUseDisplayTimer;
        private static bool                     isCurrentSpellUsed;
        private static GameClient               client;
        
        #endregion

        #region ability related methods
        public static float GetRealDistanceBetweenTileLocations(Vector2 loc1, Vector2 loc2)
        {
            return (float)Math.Sqrt(Math.Pow(loc2.X - loc1.X, 2) + Math.Pow(loc2.Y - loc1.Y, 2));
        }
        public static int GetBlockDistanceBetweenDice(Diceman dice1, Diceman dice2)
        {
            //int dice1X = (int)GetBoardLocationFromScreenPosition(dice1.sprite.IsometricLocation).X;
            //int dice1Y = (int)GetBoardLocationFromScreenPosition(dice1.sprite.IsometricLocation).Y;
            //int dice2X = (int)GetBoardLocationFromScreenPosition(dice2.sprite.IsometricLocation).X;
            //int dice2Y = (int)GetBoardLocationFromScreenPosition(dice2.sprite.IsometricLocation).Y;
            return Math.Max(Math.Abs(dice1.BoardLocX - dice2.BoardLocX), Math.Abs(dice1.BoardLocY - dice2.BoardLocY));
        }
        public static bool AddEffect(AbilityInput effect)
        {
            for (int i = 0; i < activeEffects.Count; i++)
            {
                if (effect.selectedSpell.name == activeEffects[i].selectedSpell.name)
                {
                    if (((effect.selectedSpell.isLocationBased && effect.fireLocation == activeEffects[i].fireLocation) ||
                        (!effect.selectedSpell.isLocationBased && effect.targetedDice == activeEffects[i].targetedDice)) &&
                        !((effect.selectedSpell as Effect).isStackable))
                    {
                        if (spellPlayList.IndexOf(activeEffects[i]) > spellPlayingIndex)
                        {
                            spellPlayList.Remove(activeEffects[i]);
                        }
                        if (!effect.selectedSpell.isLocationBased)
                        {
                            activeEffects[i].targetedDice.activeEffects.Remove(activeEffects[i].selectedSpell as Effect);
                        }
                        activeEffects.RemoveAt(i);                        
                        break;
                    }
                }
            }
            activeEffects.Add(effect);
            if (!effect.selectedSpell.isLocationBased)
            {
                effect.targetedDice.activeEffects.Add(effect.selectedSpell as Effect);
            }
            int x = spellPlayingIndex + 1;
            while (x < spellPlayList.Count && GetAbilitySpeed(effect) < GetAbilitySpeed(spellPlayList[x]))
            {
                x++;
            }
            spellPlayList.Insert(x, effect);
            return true;
        }
        #endregion

        #region game update helper methods
        private static void SetAbilityTargetDescription()
        {
            if (spellPlayingIndex >= spellPlayList.Count) 
            {
                return;
            }
            if (!(spellPlayList[spellPlayingIndex].selectedSpell is Effect))
            {
                if (!spellPlayList[spellPlayingIndex].selectedSpell.isLocationBased)
                {                    
                    abilityDescription.text = spellPlayList[spellPlayingIndex].selectedDice.ownedByPlayer.playerName
                        + " used " + spellPlayList[spellPlayingIndex].selectedSpell.name + " on " +
                        spellPlayList[spellPlayingIndex].targetedDice.ownedByPlayer.playerName + ".";
                }
                else
                {
                    abilityDescription.text = spellPlayList[spellPlayingIndex].selectedDice.ownedByPlayer.playerName
                        + " used " + spellPlayList[spellPlayingIndex].selectedSpell.name + ".";
                }
            }
            else
            {
                if (!spellPlayList[spellPlayingIndex].selectedSpell.isLocationBased)
                {
                    abilityDescription.text = spellPlayList[spellPlayingIndex].targetedDice.ownedByPlayer.playerName + "'s " +
                        spellPlayList[spellPlayingIndex].targetedDice.name + " is affected by " +
                        spellPlayList[spellPlayingIndex].selectedSpell.name + ".";
                }
            }
        }            
        private static bool CheckDiceMadeTurn(Diceman checkDice)
        {
            bool diceMadeTurn = true;
            for (int i = 0; i <= inputMoveIndex; i++)
            {
                if (abilityInputs[i].selectedDice != null && abilityInputs[i].selectedDice == checkDice)
                {
                    diceMadeTurn = false;
                }
            }
            return diceMadeTurn;
        }
        private static void SetDiceDescriptText(Diceman selectedDice)
        {
            if (selectedDice != null)
            {
                dicemanDescription.text = selectedDice.name +
                            "\nHealth : " + Math.Round(selectedDice.Health, Constant.o_roundToDigits).ToString() + "/" + Math.Round(selectedDice.MaxHealth, Constant.o_roundToDigits).ToString() +
                            "\nStrength : " + Math.Round(selectedDice.strength, Constant.o_roundToDigits).ToString() +
                            "\nSpeed : " + Math.Round(selectedDice.speed, Constant.o_roundToDigits).ToString();
            }
            else
            {
                dicemanDescription.text = "";
            }
        }
        private static float GetAbilitySpeed(AbilityInput ability)
        {
            return ability.selectedDice.speed * ability.selectedSpell.speed;
        }
        private static int CompareAbilitySpeeds(AbilityInput x, AbilityInput y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                else
                {
                    return +1;
                }
            }
            else
            {
                if (y == null)
                {
                    return -1;
                }
                else
                {
                    if (GetAbilitySpeed(x) < GetAbilitySpeed(y))
                    {
                        return +1;
                    }
                    else if (GetAbilitySpeed(x) > GetAbilitySpeed(y))
                    {
                        return -1;
                    }                    
                    else
                    {
                        return 0;
                    }
                }
            }
        }
        #endregion

        #region board location and coordinate methods
        public static FloorTile GetBoardTile(int i, int j)
        {
            if (i < 0 || j < 0 || i >= board.GetLength(0) || j >= board.GetLength(1))
            {
                return null;
            }
            else
            {
                return board[i, j];
            }
        }
        public static FloorTile GetBoardTile(Vector2 boardCoord)
        {
            return GetBoardTile((int)boardCoord.X, (int)boardCoord.Y);
        }
        private static Vector2 GetIsoLocFromCart(Vector2 cartLoc)
        {
            Vector2 isoLoc = Vector2.Zero;
            isoLoc.X = cartLoc.X - cartLoc.Y;
            isoLoc.Y = (cartLoc.X + cartLoc.Y) / 2;
            return isoLoc;
        }
        private static Vector2 GetCartLocFromIso(Vector2 isoLoc)
        {
            Vector2 cartLoc = Vector2.Zero;
            cartLoc.X = (2 * isoLoc.Y + isoLoc.X) / 2;
            cartLoc.Y = (2 * isoLoc.Y - isoLoc.X) / 2;
            return cartLoc;
        }
        private static Vector2 GetBoardLocationFromScreenPosition(Vector2 screenPos)
        {
            Vector2 boardLoc = Vector2.Zero;            
            Vector2 cartPos = GetCartLocFromIso(screenPos);
            if (board == null)
            {
                return new Vector2(-1,-1);
            }            
            // if the screen pos is outside of the board's boundaries, then return -1
            // checked by getting the lines for the board boundaries, and then checking on which side of those 4 lines
            // does the given point lie.
            /* Isometric boundary checking
             * (screenPos.Y - screenPos.X / 2 < board[0, 0].sprite.Location.Y + floorIsoLengthBy2) ||
                (screenPos.Y + screenPos.X / 2 < board[0, 0].sprite.Location.X + floorIsoLengthBy2) ||
                (screenPos.Y - screenPos.X / 2 > board[board.GetLength(0) - 1, board.GetLength(1) - 1].sprite.Location.Y + floorIsoLengthBy2) ||
                (screenPos.Y + screenPos.X / 2 > board[board.GetLength(0) - 1, board.GetLength(1) - 1].sprite.Location.X + floorIsoLengthBy2)*/
            // Cartesian boundary checking
            if (cartPos.X < (board[0, 0].sprite.Location.X - board[0, 0].isoLengthBy2) ||
                cartPos.X >= (board[board.GetLength(0) - 1, 0].sprite.Location.X + board[0, 0].isoLengthBy2) ||
                cartPos.Y < (board[0, 0].sprite.Location.Y - board[0, 0].isoLengthBy2) ||
                cartPos.Y >= (board[0, board.GetLength(1) - 1].sprite.Location.Y + board[0, 0].isoLengthBy2))
            {
                return new Vector2(-1, -1);
            }

            boardLoc.X = (int)Math.Abs(cartPos.X - board[0, 0].sprite.Location.X + board[0, 0].isoLengthBy2) / 32;
            boardLoc.Y = (int)Math.Abs(cartPos.Y - board[0, 0].sprite.Location.Y + board[0, 0].isoLengthBy2) / 32;
            return boardLoc;
        }
        #endregion                        
        
        #region Main methods
        /// <summary>
        /// Creates an array of floortiles having varying tile nos. 
        /// Location starts from topTileLoc and go to boardsize * 32.
        /// </summary>
        /// <param name="floorTileSet"></param>
        /// <param name="topTileLocation"></param>
        /// <param name="boardSize"></param>
        public BattleBoard(Texture2D floorTileSet, SpriteFont font, Vector2 topTileLocation, int boardSize, Player Player1, Player Player2, GameClient Client, Vector2 screenSize)
        {
            int tileNo = 0;
            inputMoveIndex = 0;
            spellPlayingIndex = 0;
            spellUseDisplayTimer = 0;
            isCurrentSpellUsed = false;
            client = Client;
            currentState = State.Input;
            inputState = InputState.Waiting;
            abilityInputs = new List<AbilityInput>(movesPerTurn);
            activeEffects = new List<AbilityInput>();
            spellPlayList = new List<AbilityInput>();
            for (int i = 0; i < movesPerTurn; i++)
            {
                abilityInputs.Add(new AbilityInput());
            }
            topTileLoc = topTileLocation;            
            highLightedTiles = new List<FloorTile>();
            loggedInPlayer = Player1;
            opponentPlayer = Player2;
            dicemanDescription = new TextSprite(font, Constant.v_diceDescript, "", Color.White, 1f, Constant.l_diceDescript);
            abilityDescription = new TextSprite(font, screenSize + Constant.v_abilityDescriptOffset, "", Color.White, 1f, Constant.l_diceDescript);
            board = new FloorTile[boardSize, boardSize];            
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    if (RandomGenerator.Random.NextDouble() < 0.2)
                    {
                        tileNo = (tileNo + 1) % 8;
                    }
                    board[i, j] = new FloorTile(floorTileSet, tileNo, new Vector2(topTileLocation.X + i * 32, topTileLocation.Y + j * 32));
                }
            }
        }
        ~BattleBoard()
        {            
            inputMoveIndex = 0;
            spellPlayingIndex = 0;
            spellUseDisplayTimer = 0;
            isCurrentSpellUsed = false;
            client = null;
            currentState = State.Input;
            inputState = InputState.Waiting;            
            abilityInputs = null;
            activeEffects = null;
            spellPlayList = null;
            highLightedTiles = null;
            loggedInPlayer = null;
            opponentPlayer = null;
            dicemanDescription = null;
            abilityDescription = null;
            board = null;

        }
        //CHECK null exceptions for spellplaylist when it gets empty in end game situations.
        private void ChangeInputState(InputState toState)
        {
            switch (toState)
            {
                case InputState.DiceSelected:
                    for (int i = 0; i < abilityInputs[inputMoveIndex].selectedDice.activeAbilities.Count; i++)
                    {
                        abilityInputs[inputMoveIndex].selectedDice.activeAbilities[i].icon.Location = new Vector2(
                        abilityInputs[inputMoveIndex].selectedDice.sprite.Location.X + abilityIconOffset * (int)((i + 1) / 2),
                        abilityInputs[inputMoveIndex].selectedDice.sprite.Location.Y + abilityIconOffset * (int)((i + 1) % 2));
                    }
                    abilityInputs[inputMoveIndex].selectedDice.Highlight();
                    inputState = toState;
                    break;
                case InputState.AbilitySelected:
                    for (int i = abilityInputs[inputMoveIndex].selectedDice.BoardLocX - abilityInputs[inputMoveIndex].selectedSpell.range;
                        i <= abilityInputs[inputMoveIndex].selectedDice.BoardLocX + abilityInputs[inputMoveIndex].selectedSpell.range;
                        i++)
                    {
                        for (int j = abilityInputs[inputMoveIndex].selectedDice.BoardLocY - abilityInputs[inputMoveIndex].selectedSpell.range;
                            j <= abilityInputs[inputMoveIndex].selectedDice.BoardLocY + abilityInputs[inputMoveIndex].selectedSpell.range;
                            j++)
                        {
                            if (GetBoardTile(i, j) != null)
                            {
                                GetBoardTile(i, j).HighlightTile();
                                highLightedTiles.Add(GetBoardTile(i, j));
                            }
                        }
                    }
                    abilityDescription.text = abilityInputs[inputMoveIndex].selectedSpell.abilityDescription;
                    inputState = toState;
                    break;
                case InputState.Waiting:
                    if (abilityInputs[inputMoveIndex].selectedDice != null)
                    {
                        abilityInputs[inputMoveIndex].selectedDice.UnHighlight();
                    }
                    abilityInputs[inputMoveIndex].selectedDice = null;
                    abilityInputs[inputMoveIndex].selectedSpell = null;                    
                    abilityInputs[inputMoveIndex].targetedDice = null;
                    abilityDescription.text = "";
                    for (int i = 0; i < highLightedTiles.Count; i++)
                    {
                        highLightedTiles[i].UnhighlightTile();
                    }
                    highLightedTiles.Clear();
                    inputState = toState;
                    break;
            }
        }
        private void ChangeGameState(State toState)
        {
            switch (toState)
            {
                case State.Input:
                    for (int i = spellPlayList.Count - 1; i >= 0; i--)
                    {
                        if (spellPlayList[i].selectedSpell is Effect)
                        {
                            if (((Effect)spellPlayList[i].selectedSpell).remainingDuration == 0)
                            {
                                activeEffects.Remove(spellPlayList[i]);
                                spellPlayList[i].targetedDice.activeEffects.Remove((Effect)spellPlayList[i].selectedSpell);
                                spellPlayList.RemoveAt(i);
                            }
                        }
                    }
                    spellPlayList.Clear();
                    client.FlushOpponentMoves();
                    for (int i = loggedInPlayer.ownedDicemen.Count - 1; i >= 0; i--)
                    {
                        if (!loggedInPlayer.ownedDicemen[i].isAlive)
                        {
                            GetBoardTile(loggedInPlayer.ownedDicemen[i].BoardLocation).occupyingDiceman = null;
                            loggedInPlayer.ownedDicemen.RemoveAt(i);
                        }
                    }
                    for (int i = opponentPlayer.ownedDicemen.Count - 1; i >= 0; i--)
                    {
                        if (!opponentPlayer.ownedDicemen[i].isAlive)
                        {
                            GetBoardTile(opponentPlayer.ownedDicemen[i].BoardLocation).occupyingDiceman = null;
                            opponentPlayer.ownedDicemen.RemoveAt(i);
                        }
                    }
                    for (int i = 0; i < movesPerTurn; i++)
                    {                        
                        ChangeInputState(InputState.Waiting);
                        inputMoveIndex++;
                    }
                    inputMoveIndex = 0;
                    spellPlayingIndex = 0;
                    spellUseDisplayTimer = 0;
                    isCurrentSpellUsed = false;
                    currentState = State.Input;                    
                    break;
                case State.Waiting:
                    for (int i = 0; i < highLightedTiles.Count; i++)
                    {
                        highLightedTiles[i].UnhighlightTile();
                    }
                    highLightedTiles.Clear();
                    dicemanDescription.text = "";
                    abilityDescription.text = "Waiting for Opponent's moves.";
                    List<AbilityInput> playerMoves = new List<AbilityInput>();
                    foreach (AbilityInput ai in abilityInputs)
                    {
                        if (ai.selectedSpell != null)
                        {
                            playerMoves.Add(ai);
                        }
                    }
                    client.SendPlayerMoves(playerMoves);
                    currentState = State.Waiting;
                    break;
                case State.Playing:                    
                    foreach (AbilityInput ai in abilityInputs)
                    {
                        if (ai.selectedSpell != null)
                        {
                            spellPlayList.Add(ai);
                        }
                    }
                    spellPlayList.AddRange(activeEffects);
                    if (spellPlayList.Count > 0)
                    {                        
                        //is something wrong with sort (when 2 guys bash each other at speeds 6.something on both of em)???
                        spellPlayList.Sort(CompareAbilitySpeeds);
                        SetAbilityTargetDescription();
                        SetDiceDescriptText(spellPlayList[spellPlayingIndex].selectedDice);
                        spellPlayList[spellPlayingIndex].selectedDice.Highlight();
                        if (!spellPlayList[spellPlayingIndex].selectedSpell.isLocationBased)
                        {
                            spellPlayList[spellPlayingIndex].targetedDice.Highlight();
                        }
                        currentState = State.Playing;
                    }
                    else
                    {
                        ChangeGameState(State.Input);
                    }
                    break;
            }
        }
        private void GetBoardInput()
        {
            Vector2 boardMouseLoc = GetBoardLocationFromScreenPosition(InputManager.MouseCoordinates);
            if (InputManager.WasMouseRightClicked() && inputState == InputState.AbilitySelected)
            {
                ChangeInputState(InputState.Waiting);
            }
            if (InputManager.WasMouseLeftClicked())
            {
                switch (inputState)
                {
                    case (InputState.Waiting):
                        if (GetBoardTile(boardMouseLoc) != null && GetBoardTile(boardMouseLoc).occupyingDiceman != null)
                        {
                            if (CheckDiceMadeTurn(GetBoardTile(boardMouseLoc).occupyingDiceman) && GetBoardTile(boardMouseLoc).occupyingDiceman.ownedByPlayer == loggedInPlayer)
                            {
                                abilityInputs[inputMoveIndex].selectedDice = GetBoardTile(boardMouseLoc).occupyingDiceman;
                                ChangeInputState(InputState.DiceSelected);
                            }
                        }
                        break;
                    case (InputState.DiceSelected):
                        foreach (Ability ability in abilityInputs[inputMoveIndex].selectedDice.activeAbilities)
                        {
                            if (ability.icon.ScaledIsoDestRec.Contains(
                                (int)InputManager.MouseCoordinates.X,
                                (int)InputManager.MouseCoordinates.Y))
                            {
                                abilityInputs[inputMoveIndex].selectedSpell = ability;
                                ChangeInputState(InputState.AbilitySelected);
                            }
                        }
                        if (inputState != InputState.AbilitySelected)
                        {
                            if (GetBoardTile(boardMouseLoc) == null ||
                                (GetBoardTile(boardMouseLoc) != null && GetBoardTile(boardMouseLoc).occupyingDiceman == null))
                            {
                                ChangeInputState(InputState.Waiting);
                            }
                            else
                            {
                                if (CheckDiceMadeTurn(GetBoardTile(boardMouseLoc).occupyingDiceman) && GetBoardTile(boardMouseLoc).occupyingDiceman.ownedByPlayer == loggedInPlayer)
                                {
                                    abilityInputs[inputMoveIndex].selectedDice.UnHighlight();
                                    abilityInputs[inputMoveIndex].selectedDice = GetBoardTile(boardMouseLoc).occupyingDiceman;
                                    ChangeInputState(InputState.DiceSelected);
                                }
                            }
                        }
                        break;
                    case (InputState.AbilitySelected):
                        for (int i = 0; i < highLightedTiles.Count; i++)
                        {
                            if (highLightedTiles[i] == GetBoardTile(boardMouseLoc))
                            {
                                if (abilityInputs[inputMoveIndex].selectedSpell.Target(
                                    abilityInputs[inputMoveIndex].selectedDice, 
                                    (GetBoardTile(boardMouseLoc).occupyingDiceman),
                                    boardMouseLoc))
                                {
                                    abilityInputs[inputMoveIndex].fireLocation = boardMouseLoc;
                                    abilityInputs[inputMoveIndex].targetedDice = GetBoardTile(boardMouseLoc).occupyingDiceman;
                                    abilityInputs[inputMoveIndex].selectedDice.UnHighlight();
                                    inputMoveIndex++;
                                    if (inputMoveIndex == movesPerTurn || inputMoveIndex == loggedInPlayer.ownedDicemen.Count)
                                    {
                                        inputMoveIndex = 0;
                                        ChangeGameState(State.Waiting);                                        
                                    }
                                    else
                                    {
                                        ChangeInputState(InputState.Waiting);
                                    }
                                }
                            }
                        }
                        break;
                }
            }
        }               
        public void Update(GameTime gameTime)
        {
            switch (currentState)
            {
                case State.Input:                    
                    Vector2 boardMouseLoc = GetBoardLocationFromScreenPosition(InputManager.MouseCoordinates);
                    bool abilityDescriptionChanged = false;
                    if (inputState == InputState.DiceSelected)
                    {
                        foreach (Ability ability in abilityInputs[inputMoveIndex].selectedDice.activeAbilities)
                        {
                            if (ability.icon.ScaledIsoDestRec.Contains(
                                (int)InputManager.MouseCoordinates.X,
                                (int)InputManager.MouseCoordinates.Y))
                            {
                                abilityDescription.text = ability.abilityDescription;
                                abilityDescriptionChanged = true;
                            }                            
                        }
                        if (!abilityDescriptionChanged)
                        {
                            abilityDescription.text = "";
                        }
                    }                    
                    if (!(boardMouseLoc.X == -1 || boardMouseLoc.Y == -1) && GetBoardTile(boardMouseLoc).occupyingDiceman != null && !abilityDescriptionChanged)
                    {
                        SetDiceDescriptText(board[(int)boardMouseLoc.X, (int)boardMouseLoc.Y].occupyingDiceman);
                    }
                    else if (abilityInputs[inputMoveIndex].selectedDice != null)
                    {
                        SetDiceDescriptText(abilityInputs[inputMoveIndex].selectedDice);
                    }
                    else
                    {
                        SetDiceDescriptText(null);
                    }                    
                    GetBoardInput();
                    break;
                case State.Waiting:
                    List<AbilityInput> opponentInputs = client.GetOpponentMoves();
                    if (opponentInputs != null)
                    {
                        spellPlayList.AddRange(opponentInputs);
                        ChangeGameState(State.Playing);
                    }
                    break;
                case State.Playing:
                    float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                    spellUseDisplayTimer += elapsedTime;                    
                    if (spellUseDisplayTimer >= Constant.o_spellUsageDisplayDelay)
                    {
                        if (!isCurrentSpellUsed)
                        {
                            if (spellPlayList[spellPlayingIndex].selectedSpell.Target(spellPlayList[spellPlayingIndex].selectedDice, spellPlayList[spellPlayingIndex].targetedDice, spellPlayList[spellPlayingIndex].fireLocation))
                            {
                                spellPlayList[spellPlayingIndex].selectedSpell.Use(spellPlayList[spellPlayingIndex].selectedDice, spellPlayList[spellPlayingIndex].targetedDice, spellPlayList[spellPlayingIndex].fireLocation);
                            }
                            spellUseDisplayTimer = 0f;
                            abilityDescription.text = spellPlayList[spellPlayingIndex].selectedSpell.usageDescription;
                            if (!spellPlayList[spellPlayingIndex].selectedSpell.isLocationBased)
                            {
                                SetDiceDescriptText(spellPlayList[spellPlayingIndex].targetedDice);
                            }
                            isCurrentSpellUsed = true;
                        }
                        else
                        {
                            spellPlayList[spellPlayingIndex].selectedDice.UnHighlight();
                            if (!spellPlayList[spellPlayingIndex].selectedSpell.isLocationBased)
                            {
                                spellPlayList[spellPlayingIndex].targetedDice.UnHighlight();
                            }                            
                            spellPlayingIndex++;
                            while (spellPlayingIndex < spellPlayList.Count &&
                                spellPlayList[spellPlayingIndex].selectedSpell is Effect &&
                                !(spellPlayList[spellPlayingIndex].selectedSpell.Target(
                                    spellPlayList[spellPlayingIndex].selectedDice,
                                    spellPlayList[spellPlayingIndex].targetedDice,
                                    spellPlayList[spellPlayingIndex].fireLocation)))
                            {
                                spellPlayingIndex++;
                            }
                            if (!(spellPlayingIndex < spellPlayList.Count))
                            {
                                ChangeGameState(State.Input);
                            }
                            else
                            {
                                SetAbilityTargetDescription();
                                SetDiceDescriptText(spellPlayList[spellPlayingIndex].selectedDice);
                                spellPlayList[spellPlayingIndex].selectedDice.Highlight();
                                if (!spellPlayList[spellPlayingIndex].selectedSpell.isLocationBased)
                                {
                                    spellPlayList[spellPlayingIndex].targetedDice.Highlight();
                                }
                            }
                            spellUseDisplayTimer = 0f;
                            isCurrentSpellUsed = false;
                        }
                    }                     
                    break;                
            }                                      
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (FloorTile tile in board)
            {
                tile.Draw(spriteBatch);
            }
            if (/*dicemanDescription.text != null &&*/ dicemanDescription.text != "")
            {
                dicemanDescription.Draw(spriteBatch);
            }
            if (/*abilityDescription.text != null &&*/ abilityDescription.text != "")
            {
                abilityDescription.Draw(spriteBatch);
            }
            switch (currentState)
            {
                case State.Input:                    
                    switch (inputState)
                    {                        
                        case InputState.DiceSelected:
                            for (int i = 0; i < abilityInputs[inputMoveIndex].selectedDice.activeAbilities.Count; i++)
                            {                                
                                abilityInputs[inputMoveIndex].selectedDice.activeAbilities[i].icon.Draw(spriteBatch);
                            }
                            break;
                        case InputState.AbilitySelected:
                            break;                        
                    }
                    break;
                case State.Playing:                                    
                    break;
            }
        }
        #endregion
    }
}
