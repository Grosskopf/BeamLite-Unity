﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;


public class Whiteboard : MonoBehaviour
{
    private int _penSize = 5; //the width and height of the pen-tip in pixels
    public Color32 resetColor; //the blank color of the Whiteboard, set in Start
    
    private Texture2D[] layers;//have to do it this way
    private Color32 color; //needs to be handled independently
    private Color32[] brushTexture; //stores an rgba bitmap of the pen texture pensize*pensize big, might be interesting to replace with something round

    private bool touching, touchingLast; //checks if pen is touching the whiteboard or has been touching the whiteboard in the last frame at least 
    private float lastX, lastY; //position in the last frame, will replace with buffer

    private List<Vector2> UndriedList; //queue of uncleaned positions, usually 4-5 points long
    private Queue<Vector2> pointsqueue;

    private int width, height; //width and height of Whiteboard
    private Color32[] resetColorArray; //an array for resetting the pixels of the texture, set in Start to resetColor

    // Use this for initialization
    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        // create a new 2d texture for the whiteboard
        width = (int)(this.gameObject.transform.localScale.x * 1000);
        height = (int)(this.gameObject.transform.localScale.y * 1000);
        this.layers = new Texture2D[] { new Texture2D(width, height), new Texture2D(width, height) };//first is smooth, second is rough
        renderer.material.SetTexture("_CleanTex", layers[0]);
        renderer.material.SetTexture("_RoughTex", layers[1]);
        
        // this is the color for writing (equals a square of penSize * penSize in black)
        this.brushTexture = Enumerable.Repeat<Color32>( new Color32(0,0,0,255), _penSize * _penSize).ToArray<Color32>();
        this.color = new Color32(0, 0, 0, 255);

        // defining a reset color
        resetColor = new Color32(255, 255, 255, 255);

        //create a array for reseting the whiteboard
        resetColorArray = layers[0].GetPixels32();
        for (int i = 0; i < resetColorArray.Length; i++)
        {
            resetColorArray[i] = resetColor;
        }
        foreach (Texture2D layer in layers)
        {
            layer.SetPixels32(resetColorArray);
            layer.Apply();
        }
        UndriedList=new List<Vector2>();
        pointsqueue = new Queue<Vector2>();
    }

    /// <summary>
    /// clears the whiteboard
    /// </summary>
    public void Clear()
    {
        foreach (Texture2D layer in layers)
        {
            layer.SetPixels32(resetColorArray);
            layer.Apply();
        }
    }

    // Update is called once per frame
    void Update()
    {
        while (pointsqueue.Any())
        {
            Vector2 pointxy = pointsqueue.Dequeue();
            int x = (int)(pointxy.x * width - (_penSize / 2));
            int y = (int)(pointxy.y * height - (_penSize / 2));
            if (ValidPosition(x, y))
            {
                if (touchingLast)
                {
                    UndriedList.Add(new Vector2(x, y));
                    
                    layers[1].SetPixels32(x, y, _penSize, _penSize, brushTexture);
                    for (float time = 0.01f; time < 1.00f; time += 0.01f) // sets 100 points between old position and new position
                    {
                        int lerpX = (int)Mathf.Lerp(lastX, (float)x, time);
                        int lerpY = (int)Mathf.Lerp(lastY, (float)y, time);
                        layers[1].SetPixels32(lerpX, lerpY, _penSize, _penSize, brushTexture);
                    }
                    layers[1].Apply();


                }
                this.lastX = (float)x;
                this.lastY = (float)y;
            }
            

        }
        if (!touching && touchingLast)//pen is lifted, dry paint now
            {
                for (int i = UndriedList.Count - 1; i > 0; i--)
                {
                    if (i % 3 != 0)
                    {
                        UndriedList.RemoveAt(i);
                    }
                }
                
                for (int i = 0; i < UndriedList.Count - 3; i += 3)
                {
                    Vector2 P1 = UndriedList.ElementAt(i);
                    Vector2 P2 = UndriedList.ElementAt(i + 3);
                    Vector2 P1Handler2vec = UndriedList.ElementAt(i + 1) - UndriedList.ElementAt(i);//directionA * (perpendicularFootFactor(P1, UndriedQueue.ElementAt(2), directionA) * -(1.0f / 3.0f)); //especially here it breaks with the perpendicular foot calculation, that's why i'm just deleting geometry (i+1 and i+2) for smoothing
                    Vector2 P2Handler1vec = UndriedList.ElementAt(i + 2) - UndriedList.ElementAt(i + 3); //directionB * (perpendicularFootFactor(P2, UndriedQueue.ElementAt(1), directionB) * -(1.0f / 3.0f));
                    Vector2 Handlersvec = (P2 + P2Handler1vec) - (P1 + P1Handler2vec);
                    for (float time = 0.01f; time < 1.00f; time += 0.01f) //making a 3rd degree Bezier-curve between those two points and their handlers
                    {
                        Vector2 Bezierpoint1 = P1 + P1Handler2vec * time;
                        Vector2 Bezierpoint2 = P1 + P1Handler2vec + Handlersvec * time;
                        Vector2 Bezierpoint3 = P2 + P2Handler1vec * (1 - time);

                        Vector2 Bezierpoint1_2 = Vector2.Lerp(Bezierpoint1, Bezierpoint2, time);// Bezierpoint1 + (Bezierpoint2 - Bezierpoint1) * time;
                        Vector2 Bezierpoint2_3 = Vector2.Lerp(Bezierpoint2, Bezierpoint3, time);//Bezierpoint2 + (Bezierpoint3 - Bezierpoint2) * time;


                        Vector2 actualpoint = Vector2.Lerp(Bezierpoint1_2, Bezierpoint2_3, time);//Bezierpoint1_2 + (Bezierpoint2_3 - Bezierpoint1_2) * time;
                        layers[0].SetPixels32((int)actualpoint.x, (int)actualpoint.y, _penSize, _penSize, brushTexture);
                    }
                }
                UndriedList.Clear();
                //delete undried paint
                layers[0].Apply();
                layers[1].SetPixels32(resetColorArray);
                layers[1].Apply();

            }
        this.touchingLast = this.touching;

    }

    
    /// <summary>
    /// Checks if x and y is a valid position on whiteboard
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private bool ValidPosition(int x, int y)
    {
        return x >= _penSize / 2 && y >= _penSize / 2 && x+_penSize/2 <= width && y+_penSize/2 <= height;
    }

    public void SetTouching(bool touching)
    {
        this.touching = touching;
    }
    

    /// <summary>
    /// set the touchposition and color
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="color"></param>
    public void SetTouchPosition(float x, float y, Color color, int penSize)
    {
        // change color only if new color is different to current color
        if(this.color != color || _penSize != penSize)
        {
            SetColor(color,penSize);
        }
        this.pointsqueue.Enqueue(new Vector2(x, y));
        //this.positionX = x;
        //this.positionY = y;
    }

    public void SetColor(Color32 color, int penSize)
    {
        this._penSize = penSize;
        this.brushTexture = Enumerable.Repeat<Color32>(color, _penSize * _penSize).ToArray<Color32>();
        //this.brushTexture = getRoundBrush(color, penSize);
        this.color = color;
    }
    
}