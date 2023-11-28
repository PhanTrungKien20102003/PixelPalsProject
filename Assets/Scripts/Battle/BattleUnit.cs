using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; //import
using DG.Tweening; //installing DoTween

public class BattleUnit : MonoBehaviour
{
    
    [SerializeField] bool isPlayerUnit; //identify if its a player's Pokemon or opponent's Pokemon

    public PokemonLevel Pokemon { get; set; } 

    Image image;
    Vector3 originalPos; //variable to store original position of the image 
    Color originalColor; //change the color when being hit

    private void Awake()
    {
        image = GetComponent<Image>();
        originalPos = image.transform.localPosition; /* using localPosition is because I want its position relative to the
                                                        canvas, not its actual position*/
        originalColor = image.color;
    }

    public void Setup(PokemonLevel pokemon) //public functions that will create a Pokemon from the base and it level
                                            //set the Pokemon dynamically
    {
        Pokemon = pokemon;
        if (isPlayerUnit)
           image.sprite = Pokemon.Base.BackSprite;
        else
           image.sprite = Pokemon.Base.FrontSprite;

        image.color = originalColor; //reset the color image after the alpha was faded to 0 which is fainted
        PlayEnterAnimation();
    }
    public void PlayEnterAnimation() //when encouting a battle, the Pokemon will appear from the outside into the battle
    {
        if (isPlayerUnit)
            image.transform.localPosition = new Vector3(-500f, originalPos.y); //place the player unit outside canvas
        else
            image.transform.localPosition = new Vector3(500f, originalPos.y); //place the enemy unit outside canvas

        image.transform.DOLocalMoveX(originalPos.x, 1f); //update the local position
    }
    public void PlayAttackAnimation() //function for the attack animation
    {
        var sequence = DOTween.Sequence(); //use sequence to play multiple animations one-by-one
        if (isPlayerUnit)
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x + 50f, 0.25f));/*when the player unit attack, 
                                                                                       the Pokemon will move to the right*/
        else
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x - 50f, 0.25f));/*when the enemy unit attack, 
                                                                                       the Pokemon will move to the left*/

        sequence.Append(image.transform.DOLocalMoveX(originalPos.x, 0.25f)); //bring back to original position
    }

    /*public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.gray, 0.1f));
        sequence.Append(image.DOColor(originalColor, 0.1f));
    }*/

    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();
        if (!isPlayerUnit)
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x + 50f, 0.25f)); /*when Player unit hit enemy's Pokemon
                                                                                        it will move a little bit to the right*/
        else
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x - 50f, 0.25f)); /*when Enemy unit hit player's Pokemon
                                                                                        it will move a little bit to the left*/

        sequence.Append(image.transform.DOLocalMoveX(originalPos.x, 0.25f));
        sequence.Append(image.DOColor(Color.gray, 0.1f)); //changing the color of the image if being hit
        sequence.Append(image.DOColor(originalColor, 0.1f)); //bringing back to original color

    }

    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.transform.DOLocalMoveY(originalPos.y - 150f, 0.5f)); /* move the unit down and fade its alpha
                                                                                    to 0 */
        
        sequence.Join(image.DOFade(0f, 0.5f)); /*I'm using Join() because it will not start playing the fade animation
                                                 if the first animation isn't complete. I want both of them to play together*/
    }
}
