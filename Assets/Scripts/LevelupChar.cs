using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

[Serializable]
public class XpSteps
{
	public List<LevelStep> levelSteps;
}

[Serializable]
public class LevelStep
{
	public float xp;
	public string imageName;
}

public class LevelupChar : MonoBehaviour
{
	public Image CharacterImage;
	public InputField XpInputField;
	public Button AddXpButton;
	public Image XpProgressBar;
	public Text XpBarProgressLabel;
	public Text LevelText;

	TextAsset xpStepsJsonTextAsset;
	public XpSteps xpSteps;
	List<Sprite> characterImages = new List<Sprite>();


	int level;
	float xp;

	void Awake()
	{
		AddXpButton.onClick.AddListener(HandleOnXpButtonClicked);
		xpStepsJsonTextAsset = Resources.Load<TextAsset>("xpSteps");
		xpSteps = JsonUtility.FromJson<XpSteps>(xpStepsJsonTextAsset.text);
		
		string resourcesPath = System.IO.Path.Combine(Application.dataPath, "Resources");
		foreach (LevelStep step in xpSteps.levelSteps)
			characterImages.Add(LoadNewSprite(Path.Combine(resourcesPath, step.imageName)));
		//characterImages.Add(Resources.Load<Sprite>(step.imageName));
	}

	void Start()
	{
		level = 0;
		xp = 0;
		RaiseXP(0);
		DisplayLevelUp();
	}

	void HandleOnXpButtonClicked()
	{
		RaiseXP(float.Parse(XpInputField.text));
		XpInputField.text = "";
	}

	void RaiseXP(float xpDelta)
	{
		// TOOD add interpolated raise xp ui animation
		xp += xpDelta;

		float requiredXp = GetRequiredXp();

		while (requiredXp > 0 && xp >= requiredXp)
		{
			// Level Up
			xp -= requiredXp;
			level++;
			DisplayLevelUp();
			requiredXp = GetRequiredXp();
		}

		XpProgressBar.fillAmount = requiredXp > 0 ? xp / requiredXp : 1f;
		XpBarProgressLabel.text = xp + " / " + requiredXp + " XP";
	}

	float GetRequiredXp()
	{
		int nextLevel = level + 1;
		if (xpSteps.levelSteps.Count <= nextLevel)
			return 0;
		return xpSteps.levelSteps[nextLevel].xp - (nextLevel > 0 ? xpSteps.levelSteps[nextLevel - 1].xp : 0);
	}

	void DisplayLevelUp()
	{
		// TOOD add interpolated level up ui animation
		LevelText.text = "LEVEL " + level;
		if (characterImages.Count > level)
		{
			CharacterImage.sprite = characterImages[level];
		}
	}




	public static Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f, SpriteMeshType spriteType = SpriteMeshType.Tight)
	{

		// Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference

		Sprite NewSprite = new Sprite();
		Texture2D SpriteTexture = LoadTexture(FilePath);
		NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), PixelsPerUnit, 0, spriteType);

		return NewSprite;
	}

	public static Sprite ConvertTextureToSprite(Texture2D texture, float PixelsPerUnit = 100.0f, SpriteMeshType spriteType = SpriteMeshType.Tight)
	{
		// Converts a Texture2D to a sprite, assign this texture to a new sprite and return its reference

		Sprite NewSprite = new Sprite();
		NewSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), PixelsPerUnit, 0, spriteType);

		return NewSprite;
	}

	public static Texture2D LoadTexture(string FilePath)
	{

		// Load a PNG or JPG file from disk to a Texture2D
		// Returns null if load fails

		Texture2D Tex2D;
		byte[] FileData;

		if (File.Exists(FilePath))
		{
			FileData = File.ReadAllBytes(FilePath);
			Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
			if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
				return Tex2D;                 // If data = readable -> return texture
		}
		return null;                     // Return null if load failed
	}
}
