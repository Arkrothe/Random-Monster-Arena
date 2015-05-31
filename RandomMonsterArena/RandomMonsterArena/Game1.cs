using System;
using System.Collections.Generic;
using System.Linq;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace RandomMonsterArena
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        enum GameState
        {            
            MainMenu,
            MultiplayerLobby,
            Playing
        }
        GraphicsDeviceManager               graphics;
        SpriteBatch                         spriteBatch;
        SpriteFont                          mainUIFont;
        BattleBoard                         board;
        Texture2D                           boardTileTextures;
        Player                              loggedInPlayer,opponentPlayer;
        GameState                           currentGameState = GameState.MainMenu;
        GameHost                            host;
        GameClient                          client;
        
        //test;       
        public static TextSprite multiplayerLobbyTest;
        public static Move move;
        public static Bash bash;
        public static Bite bite;
        public static Charge charge;
        public static Multishot multishot;
        public static VaultAttack vaultAttack;
        public static PoisonAttack poisonAttack;
        public static BasicAttack meleeAttack;
        public static BasicAttack rangedAttack;               
        //end test
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            this.graphics.PreferredBackBufferWidth = 1024;
            this.graphics.PreferredBackBufferHeight = 600;
            this.IsMouseVisible = true;
            this.graphics.ApplyChanges();
            base.Initialize();
        }        
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);            
            mainUIFont = Content.Load<SpriteFont>(@"MainUIFont");
            multiplayerLobbyTest = new TextSprite(mainUIFont, new Vector2(50, 50), "", Color.White, 1, Constant.l_diceDescript);
            multiplayerLobbyTest.text = "";
            boardTileTextures = Content.Load<Texture2D>(@"Sprites\\iso_grass_tileset");
            Constant.t_icons = Content.Load<Texture2D>(@"Sprites\\iconTextures");
            //board = new BattleBoard(
            //    boardTileTextures,
            //    mainUIFont,
            //    new Vector2(320,-192),
            //    12, 
            //    player1,
            //    player2,
            //    new Vector2(this.Window.ClientBounds.Width, this.Window.ClientBounds.Height));           
            //test                                      
            // TODO: use this.Content to load your game content here
        }
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            if (host != null)
            {
                host.StopServer();
            }
            if (client != null)
            {
                client.StopClient();
            }
            // TODO: Unload any non ContentManager content here
        }
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            InputManager.Update(gameTime);
            switch (currentGameState)
            {                
                case GameState.MainMenu:                    
                    if (InputManager.keyboardState.IsKeyDown(Keys.H))
                    {
                        host = new GameHost();
                        client = new GameClient("localhost", new Player("Host"));
                        currentGameState = GameState.MultiplayerLobby;
                    }
                    else if (InputManager.keyboardState.IsKeyDown(Keys.J))
                    {
                        client = new GameClient("", new Player("Client"));
                        currentGameState = GameState.MultiplayerLobby;
                    }
                    break;
                case GameState.MultiplayerLobby:
                    if (client.IsGameJoined && InputManager.keyboardState.IsKeyDown(Keys.S))
                    {                        
                        if (host != null)
                        {
                            client.SendGameStartMsg();
                        }                        
                    }
                    if (client.isGameStarted)
                    {
                        opponentPlayer = client.OpponentPlayer;
                        loggedInPlayer = client.LoggedInPlayer;
                        board = new BattleBoard(
                            boardTileTextures,
                            mainUIFont,
                            new Vector2(320, -192),
                            Constant.o_boardSize,
                            loggedInPlayer,
                            opponentPlayer,
                            client,
                            new Vector2(this.Window.ClientBounds.Width, this.Window.ClientBounds.Height));
                        client.SendDiceConfiguration(new Diceman.Class[] 
                        {
                            Diceman.Class.Bruiser, 
                            Diceman.Class.Bruiser,
                            Diceman.Class.Bruiser,
                            Diceman.Class.Ranger,
                            Diceman.Class.Ranger
                        });
                        currentGameState = GameState.Playing;
                    }
                    break;
                case GameState.Playing:
                    if (client.IsGameJoined)
                    {
                        board.Update(gameTime);
                    }
                    else
                    {
                        if (host != null)
                        {
                            host.StopServer();
                            host = null;
                        }                        
                        client = null;
                        board = null;                        
                        opponentPlayer = null;
                        loggedInPlayer = new Player(loggedInPlayer.playerName);
                        currentGameState = GameState.MainMenu;
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                    break;
            }
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            switch (currentGameState)
            {                
                case GameState.MainMenu:
                    break;
                case GameState.MultiplayerLobby:
                    multiplayerLobbyTest.Draw(spriteBatch);
                    break;
                case GameState.Playing:
                    board.Draw(spriteBatch);
                    //multiplayerLobbyTest.Draw(spriteBatch);
                    for (int i = 0; i < 5; i++)
                    {
                        if (i < loggedInPlayer.ownedDicemen.Count)
                            loggedInPlayer.ownedDicemen[i].Draw(spriteBatch);
                        if (i < opponentPlayer.ownedDicemen.Count)
                            opponentPlayer.ownedDicemen[i].Draw(spriteBatch);
                    }
                    break;
            }
            spriteBatch.End();            
            base.Draw(gameTime);
        }
    }
}
