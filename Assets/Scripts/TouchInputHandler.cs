﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchInputHandler : MonoBehaviour
{
    public static bool inputEnabled = false;
    public static bool touchEnabled = false;
    public static bool touchSupported = false;
    private GameManagerScript gameManager;

    private void Awake()
    {
        gameManager = GetComponent<GameManagerScript>();
    }

    /**
     * Returns the tile's coordinates that matches the touch input's pixel coordinates
     * 
     * returns Vector2(-1, -1) if no candidate tile is found
     */
    private Vector2 GetTilePositionFromTouchInput(Vector2 touchPos)
    {
        for (int i = 0; i < BoxScript.grid.GetLength(0); ++i)
        {
            for (int j = 0; j < BoxScript.grid.GetLength(1); ++j)
            {
                if (BoxScript.grid[i, j] != null)
                {
                    BoxScript boxObj = BoxScript.grid[i, j].gameObject.GetComponent<BoxScript>();

                    if (boxObj.IsInsideTile(touchPos))
                    {
                        return new Vector2(i, j);
                    }
                }
            }
        }

        return new Vector2(-1, -1);
    }

    private bool IsInsideGameBoard(Vector2 touchPos)
    {
        Vector2 realPos = Camera.main.ScreenToWorldPoint(touchPos);
        if (realPos.y < 5 && realPos.y > -4.5)
        {
            return true;
        }

        return false;
    }

    // Update is called once per frame
    void Update()
    {
        // if input is disabled (pop up window, etc) then skip
        if (!inputEnabled) return;

        bool touch = touchSupported && touchEnabled && Input.touchCount > 0;

        Vector2 myPos = new Vector2();
        Vector2 myPosPixel = new Vector2();

        // touch input
        if (touch)
        {
            myPosPixel = Input.GetTouch(0).position;
        }
        // mouse input
        else if (!touchSupported && (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0)))
        {
            myPosPixel = Input.mousePosition;
        }
        //else
        //{
        //    // end the function
        //    return;
        //}

        // convert pixel position to tile coordinate
        myPos = GetTilePositionFromTouchInput(myPosPixel);

        //=================Productive/Unproductive Button touch=================
        if (GameManagerScript.OBSTRUCTION_PRODUCTIVE || 
            GameManagerScript.OBSTRUCTION_UNPRODUCTIVE)
        {
            if ((touch && Input.GetTouch(0).phase == TouchPhase.Began) ||
                Input.GetMouseButtonDown(0))
            {
                if (myPos.x >= 0)
                {
                    // there is no previously clicked box
                    if (BoxScript.currentSelection.Count == 0)
                    {
                        BoxScript.SelectTile(myPos);
                        BoxScript.LogAction("WF_LetterSelected", myPos);
                    }
                    else if (BoxScript.IsNextTo(myPos, BoxScript.currentSelection[BoxScript.currentSelection.Count - 1]) &&
                             !BoxScript.currentSelection.Contains(myPos))
                    {
                        // add on to the current selection 
                        BoxScript.SelectTile(myPos);
                        BoxScript.LogAction("WF_LetterSelected", myPos);
                    }
                    // deselect previous letters if clicking on already highlighted letters
                    else if (BoxScript.currentSelection.Contains(myPos))
                    {
                        // deselect if this is the most recently selected letter
                        if (BoxScript.currentSelection[BoxScript.currentSelection.Count - 1] == myPos)
                        {
                            BoxScript.RemoveLastSelection();
                        }
                        // go through backwards and remove all letters in selection until the letter that was tapped
                        else 
                        {
                            for (int i = BoxScript.currentSelection.Count - 1; i > 0; --i)
                            {
                                if (BoxScript.currentSelection[i] != myPos)
                                {
                                    BoxScript.RemoveLastSelection();
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        // de-select what has already been selected
                        BoxScript.ClearAllSelectedTiles();
                        BoxScript.LogAction("WF_DeselectAll");
                    }
                }
                else if (IsInsideGameBoard(myPosPixel))
                {
                    // remove all selected letters, unless clicking outside game area
                    BoxScript.ClearAllSelectedTiles();
                }

            }
            //=====Only for PRODUCTIVE versions of button, not unproductive=====
            else if (GameManagerScript.OBSTRUCTION_PRODUCTIVE &&
                     ((touch && Input.GetTouch(0).phase == TouchPhase.Moved) ||
                      (!touchSupported && 
                       !Input.GetMouseButtonDown(0) && 
                       !Input.GetMouseButtonUp(0) && 
                       Input.GetMouseButton(0))))
            {
                if (myPos.x >= 0)
                {
                    // selected tile and it isn't already selected)
                    if (BoxScript.currentSelection.Count > 0 &&
                       BoxScript.IsNextTo(myPos, BoxScript.currentSelection[BoxScript.currentSelection.Count - 1]) &&
                        !BoxScript.currentSelection.Contains(myPos))
                    {
                        BoxScript.SelectTile(myPos);
                        BoxScript.LogAction("WF_LetterSelected", myPos);
                    }
                    // do nothing if you moved within the most recently selected tile
                    else if (BoxScript.currentSelection.Count > 0 
                             && BoxScript.currentSelection[BoxScript.currentSelection.Count - 1] == myPos)
                    {
                        // do nothing
                    }
                    else if (BoxScript.currentSelection.Contains(myPos))
                    {
                        // de-select the most recent tile(s) if you move back to an old one
                        for (int i = BoxScript.currentSelection.Count - 1; i > 0; --i)
                        {
                            if (BoxScript.currentSelection[BoxScript.currentSelection.Count - 1] != myPos)
                            {
                                BoxScript.RemoveLastSelection();
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        //=========================SwipeUI touch input==========================
        else
        {
            // If SwipeUI, automatically play word when lifting the finger, and cancel if canceled for all UI's
            if (((touch && Input.GetTouch(0).phase == TouchPhase.Ended) || 
                 !touchSupported && Input.GetMouseButtonUp(0))
                && BoxScript.currentSelection.Count > 2)
            {
                gameManager.PlayWord();
            }
            else if ((touch && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Canceled)
                     || ((touch && Input.GetTouch(0).phase == TouchPhase.Ended) || 
                         (!touchSupported && Input.GetMouseButtonUp(0))
                    && BoxScript.currentSelection.Count < 3))
            {
                BoxScript.ClearAllSelectedTiles();
                BoxScript.LogAction("WF_DeselectAll");
            }

            if (myPos.x >= 0)
            {
                if ((touch && Input.GetTouch(0).phase == TouchPhase.Began) ||
                    (!touchSupported && Input.GetMouseButtonDown(0)))
                {
                    // there is no previously clicked box
                    if (BoxScript.currentSelection.Count == 0)
                    {
                        BoxScript.SelectTile(myPos);
                        BoxScript.LogAction("WF_LetterSelected", myPos);
                    }
                    else
                    {
                        // de-select what has already been selected
                        BoxScript.ClearAllSelectedTiles();
                        BoxScript.LogAction("WF_DeselectAll");
                    }
                }
                else if ((touch && Input.GetTouch(0).phase == TouchPhase.Moved) ||
                         (!touchSupported && Input.GetMouseButton(0)))
                {
                    // new tile and it isn't already selected)
                    if (BoxScript.currentSelection.Count > 0 && 
                        BoxScript.IsNextTo(myPos, BoxScript.currentSelection[BoxScript.currentSelection.Count - 1]) &&
                        !BoxScript.currentSelection.Contains(myPos))
                    {
                        BoxScript.SelectTile(myPos);
                        BoxScript.LogAction("WF_LetterSelected", myPos);
                    }
                    // most recently selected tile (then do nothing, since you just selected it)
                    else if (BoxScript.currentSelection.Count > 0 && 
                             BoxScript.currentSelection[BoxScript.currentSelection.Count - 1] == myPos)
                    {
                        // do nothing
                    }
                    // you've moved back to a previous tile that was already selected
                    else if (BoxScript.currentSelection.Contains(myPos))
                    {
                        // de-select the most recent tile(s) if you move back to an old one
                        for (int i = BoxScript.currentSelection.Count - 1; i > 0; --i)
                        {
                            if (BoxScript.currentSelection[BoxScript.currentSelection.Count - 1] != myPos)
                            {
                                BoxScript.RemoveLastSelection();
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
