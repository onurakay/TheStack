https://github.com/user-attachments/assets/eb3fbfee-7a3a-4f35-a805-8741f414028e

# **The Stack Game**

**The Stack Game** is a remake of the popular stack game, designed for mobile platforms using Unity. This game emphasizes efficient resource utilization and performance optimization.

##

- Stack tiles to build a high tower, using mechanics to keep them aligned.
- Used an object pool for rubble to cut down on performance issues.
- Real-time updates to show the player’s score and combo streak on the UI.
- Gradients to colors (tile, background, effects) based on the score.
- `Mathf.Round()` to snap block positions to two decimal places. ensures blocks align perfectly and there are no small, annoying shifts.

##
- **GameState.cs:** Manages and broadcasts score and combo changes.
- **RubblePool.cs:** Implements object pooling for rubble, handling reuse and deactivation.
- **TheStack.cs:** Controls tile placement, movement, and score updates, integrating game mechanics with visual changes.
- **UIManager.cs:** Updates UI elements based on game state events.
  
## *Future Plans*
- sound effects
- implementing placement effects
- improving ui
