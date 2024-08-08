![TheStack_small](https://github.com/user-attachments/assets/4fa72f89-d02a-4971-97eb-fbb7f3ca3175)

# **The Stack Game**

**The Stack Game** is a remake of the popular stack game, designed for mobile platforms using Unity. This game emphasizes efficient resource utilization and performance optimization.

##

- **Tile Stacking:** Stack tiles to build a high tower with precise alignment mechanics.
- **Object Pooling:** Utilizes an object pool for rubble to minimize runtime overhead and improve performance.
- **Dynamic Score & Combo:** Tracks and displays the player's score and combo streak with responsive UI updates.
- **Gradient-Based Color Changes:** Applies color gradients to tiles based on the score to enhance visual appeal.

##
- **GameState.cs:** Manages and broadcasts score and combo changes.
- **RubblePool.cs:** Implements object pooling for rubble, handling reuse and deactivation.
- **TheStack.cs:** Controls tile placement, movement, and score updates, integrating game mechanics with visual changes.
- **UIManager.cs:** Updates UI elements based on game state events.
## *Future Plans*
- sound effects
- implementing placement effects
- improving ui
