# Random-Monster-Arena
A turn-based pvp game where two players fight each other with procedurally generated units on an isometric tile map.


Requires XNA game studio 4.0 to be compiled. For visual studio 2012, XNA 4.0 Refresh, which contains all the dependencies for it, can be used (https://mxa.codeplex.com/releases/view/117564).

For running the game, Host IP will have to be changed in line number 130 in Game1.cs. If the two instances of the game are run on the same network, it should work without any changes by auto discovery.

One player needs to press the H key to start the host server. Then the second player can press the J key to join. The host can then start the game by pressing the S key.

Each player needs to make three moves in each turn. Once both players have decided on the moves for a turn, they play out, and the next turn begins. The player who kills all the units of the other player wins.