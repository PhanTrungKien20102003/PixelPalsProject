using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator
{
    //references
    SpriteRenderer spriteRenderer;
    List<Sprite> frames; //list of sprites should animate
                         //name "frames" since its going to be frames of the animation
    

    /*change the sprite of SpriteRenderer to the next frame after a period of time
      once it reaches the last frame, change it back to the first frame*/
    float frameRate; //this going to be the interval at which we need to change the sprites


    int currentFrame; //keep track of current frame
    float timer; //keep track of the time


    //constructor to initialize
    public SpriteAnimator(List<Sprite> frames, SpriteRenderer sprite, float frameRate = 0.16f)
                                                                      //default 60 fps
    {
        this.frames = frames;
        this.spriteRenderer = sprite;
        this.frameRate = frameRate;
    }
    public void Start() //start the animation
    {
        currentFrame = 0;
        timer = 0;
        spriteRenderer.sprite = frames[0]; //get the first frame
    }
    public void HandleUpdate() //animate any GameObject (Player, NPC, Pokemon,...)
    {
        //increment the timer
        timer += Time.deltaTime;
        if (timer > frameRate)
        {
            currentFrame = (currentFrame + 1) % frames.Count; //change the current frame to the next frame and loop back to the first frame
            spriteRenderer.sprite = frames[currentFrame];
            timer -= frameRate; //reset timer
        }
    }

    public List<Sprite> Frames //expose the frame of animation
    {
        get { return frames; }
    }
}
