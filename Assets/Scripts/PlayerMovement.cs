using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // The new postion the player GameObject should move to based on player input (Assuming they're allowed to move to said location)
    public Vector2 newPosition;

    // A reference to the GameController script
    public GameController gameController;

    // The speed the player moves from one grid cell to the next
    public float speed;

    // The animator controller for the player (Controls the player's sprites)
    public Animator anim;

    private void Start()
    {
        // The new position is in the Start function is the initial position
        transform.localPosition = newPosition;
    }

    // Update is called once per frame
    void Update()
    {
        // Store the Vertical and Horizontal input values
        float v = Input.GetAxisRaw(StringValues.INPUT_VERTICAL);
        float h = Input.GetAxisRaw(StringValues.INPUT_HORIZONTAL);

        // Take the player input axis and apply it to the animator controller
        anim.SetFloat("DirX", h);
        anim.SetFloat("DirY", v);

        // If player presses arrow keys
        Move();

        // If the grid cell the player tries to move to IS NOT a danger zone (i.e. the player is allowed to go there)
        if (!gameController.dangerZones.Contains(newPosition))
        {
            // Move the player towards the new position. MoveTowards is better than lerp because with lerp, we'll never reach the desired
            // Vector2 and the rate at which the the object will move doesn't remain contstant. But with MoveTowards the player will move
            // at a constant speed to the destination
            transform.localPosition = Vector2.MoveTowards(transform.localPosition, newPosition, speed);

            // We need to make sure that the Traversed Tiles counter increments ONLY if we reached the desired location. Otherwise the counter
            // will count every Vector2 value between the current position to the newPosition. In other words, instead of (0, 0) to (1, 0) it'll
            // count (0, 0), (0.1, 0), (0.2, 0)....(1, 0).
            //
            // To solve this problem, the x and y values on the Vector2 are converted to Integer values to get a precise value
            if (Convert.ToInt32(transform.localPosition.x) == Convert.ToInt32(newPosition.x) && Convert.ToInt32(transform.localPosition.y) == Convert.ToInt32(newPosition.y))
            {
                // Check whether the current tile is contained inside the Traversed tile list. If it's not in the list then add it
                if (!gameController.traversedCells.Contains(new Vector2(Convert.ToInt32(newPosition.x), Convert.ToInt32(newPosition.y))))
                {
                    gameController.traversedCells.Add(new Vector2(Convert.ToInt32(newPosition.x), Convert.ToInt32(newPosition.y)));
                }
            }
        }
    }

    // This function handles value of the Vector2 newPosition
    // newPostion is only updated if it's new value doesn't match any of the Vector2 values
    // in the danger zone list or if the value isn't outside the grid's boundaries
    private void Move()
    {
        // If the player hasn't won yet then check for player keyboard input.
        // If the player won then ignore the player's keyboard input.
        // As stated previously, newPostion WILL NOT be updated if the value
        // matches one of the danger zones or if it's outside the bounds of the grid
        if (!gameController.playerWon)
        {
            // Check if the player press the 'W' key or the Up Arrow
            if (Input.GetButtonDown(StringValues.INPUT_VERTICAL))
            {
                // If the raw input (-1, 0, or 1) is positive move up. If it's negative, move down.
                // The same rules apply for Horizontal input
                if (Input.GetAxisRaw(StringValues.INPUT_VERTICAL) == 1)
                {
                    if (newPosition.y + 1 <= 0 && !gameController.dangerZones.Contains(new Vector2(newPosition.x, newPosition.y + 1)))
                    {
                        newPosition.y += 1;
                    }
                }
                else if (Input.GetAxisRaw(StringValues.INPUT_VERTICAL) == -1)
                {
                    if (newPosition.y - 1 > gameController.yMax && !gameController.dangerZones.Contains(new Vector2(newPosition.x, newPosition.y - 1)))
                    {
                        newPosition.y -= 1;
                    }
                }
            }
            else if (Input.GetButtonDown(StringValues.INPUT_HORIZONTAL))
            {
                if (Input.GetAxisRaw(StringValues.INPUT_HORIZONTAL) == 1)
                {
                    if (newPosition.x + 1 < gameController.xMax && !gameController.dangerZones.Contains(new Vector2(newPosition.x + 1, newPosition.y)))
                    {
                        newPosition.x += 1;
                    }
                }
                else if (Input.GetAxisRaw(StringValues.INPUT_HORIZONTAL) == -1 && !gameController.dangerZones.Contains(new Vector2(newPosition.x - 1, newPosition.y)))
                {
                    if (newPosition.x - 1 >= 0)
                    {
                        newPosition.x -= 1;
                    }
                }
            }
        }
    }
}
