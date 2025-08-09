🎲 Ludo Game (Unity)

📌 Overview

This is a Unity-based Ludo featuring dice rolls, pieces movement, capture mechanics, flexible board setup and win conditions.
Built with modular scripts for easy maintenance and expansion.


✨ Current Features

Turn System – Automatic turn switching between players if no valid move is present.
Dice System – Give random number, animated dice roll, Option to manually set dice numbers via getDiceNumManually (debug/testing).
Piece Movement – Step-by-step animated movement, Only starts tokens after rolling a 6, Handles extra turns on rolling 6.
Capture Mechanic – Landing on an opponent's piece sends it back to start except safe tiles.
Piece Depth Management – Active player’s pieces rendered on top (helpful to select piece when on same tiles).
Flexible Board Setup – Board pieces/ path are easy to change in BoardHandler.


🧪 Developer Mode

getDiceNumManually
Location: DiceController script.
If set to Yes, you can manually input the dice number in the Inspector before rolling.
Useful for testing specific scenarios, debugging movement and capture logic.


🚀 How to Play

Roll the dice on your turn.
If you roll a 6, you can start a token from your base.
Tokens move step-by-step based on the dice roll.
Rolling a 6 grants you an extra turn.
Landing on an opponent's token sends it back to base.
First player to get all tokens to the home wins.


📂 Scripts Overview

Script	Responsibility
MainMenuUI       Handles Mainmenu UI.
DataManager      Brings game mode(Singleplayer, Paidmatch) information between scenes.
GameSceneUI      Handles Gamescene UI (Win panel, buttons)
GameManager      Manages Game state, Player turn.
TurnSystem	 Manages turn order, extra turns, player switching and all rules.
PlayerController Handles piece selection, gives steps to move.
PieceController	 Controls individual Piece movement, owner and interactions.
DiceController	 Handles dice rolling, animations, and manual debug mode.
BoardHandler	 Holds board tile references, path points, pieces and player pieces.


Future Improvements (Code is clean, easily maintainable and expandable)

-Online Multiplayer (using Unity Netcode).
-Chat System (text-based for players).
-Voice Chat for real-time communication.
-Customizable Rules (change winning conditions, token counts).
-AI Opponents for offline play.
-Better Animations and special effects for wins, captures, and dice rolls.

