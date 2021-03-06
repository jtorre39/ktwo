﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// The lists are populated in the inspector
// and *should* be in the character order
// as defined in the CharacterEnum.

// This manager depends on having the 
// CSS prefab in the scene under
// the canvas.
public class CSSManager : MonoBehaviour
{
    public static CSSManager instance;

    public List<GameObject> previewModels;
    public List<GameObject> abilityIcons;
    public List<CSSCharacterInfoStruct> rosterData;

    public GameObject currentModel = null;
    public GameObject CSS;
    public GameObject VThirdPersonController;
    public GameObject playerUI;

    public Text textDescription;
    public Text name;
    public Canvas canvas;
    public Animator animator;
    public PlayerConnectionObject localPlayer;

    public bool animationPlayed = false;
    public int connectionNumber;

    string currentLoadedText;
    CharacterEnum? chosenCharacter = null;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    public void UpdateWelcomeMessage()
    {
        name.text = string.Format("Welcome Player {0}", connectionNumber + 1);
    }

    public void PreviewCharacter(CharacterEnum? characterToPreview)
    {
        if (characterToPreview == null || !animationPlayed) return;
        var characterIndex = (int)characterToPreview;
        UpdateAbilityIcons(characterIndex);
        UpdateCharacterDescription(characterIndex);
        UpdateCharacterTitle(characterIndex);
        UpdatePreviewModel(characterIndex);
    }

    public void RestoreChosenCharacter()
    {
        // if currentModel is null, we don't have a character to restore.
        if (!currentModel) return;
        PreviewCharacter(chosenCharacter);
    }

    public void ChooseCharacter(CharacterEnum character)
    {
        chosenCharacter = character;

        if (!animationPlayed) // begin animation, load text boxes
        {
            animationPlayed = true;
            animator.SetBool("characterChosen", true);
            PreviewCharacter(character);
        }

        localPlayer.chosenCharacter = (int)this.chosenCharacter;
        localPlayer.CmdUpdateChosenCharacter((int)chosenCharacter);

    }

    void UpdateAbilityIcons(int characterId)
    {
        for (int i = 0; i < abilityIcons.Count; i++)
        {
            var iconToChange = abilityIcons[i].GetComponent<Image>();
            iconToChange.sprite = rosterData[characterId].abilityIcons[i];
        }    
    }

    void UpdateCharacterTitle(int characterId)
    {
        name.text = string.Format("\"The {0}\"", rosterData[characterId].character.ToString());
    }

    void UpdateCharacterDescription(int characterId)
    {
        textDescription.text = rosterData[characterId].characterDescription;
    }

    void UpdatePreviewModel(int characterId)
    {
        if (currentModel == rosterData[characterId].previewModel) return;

        if (currentModel)
        {
            currentModel.SetActive(false);
        }

        currentModel = rosterData[characterId].previewModel;
        currentModel.SetActive(true);
    }

    public void ShowAbilityDescription(int abilityIndex)
    {
        textDescription.text = rosterData[(int)chosenCharacter].abilityDescriptions[abilityIndex];
    }

    public void RestoreCharacterDescription()
    {
        UpdateCharacterDescription((int)chosenCharacter);
    }

    public void ShowCSSScreen()
    {
        UpdateWelcomeMessage();
        CSS.SetActive(true);
    }

    public void HideCSSScreen()
    {
        CSS.SetActive(false);
        VThirdPersonController.SetActive(true);
        playerUI.SetActive(true);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    }
}
