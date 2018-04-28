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

	// time the bar needs for a whole level
	public float xpChangeDurationPerLevel;

	public float levelPlopAnimationDuration;
	[Range(1,4)]
	public float levelUpPlopFontSizeMultiplier;


	TextAsset xpStepsJsonTextAsset;
	public XpSteps xpSteps;
	List<Sprite> characterImages = new List<Sprite>();
	
	int level;

	float xp;
	float xpTarget;

	// used for level up animation
	float levelUpTime = -3000;

	void Awake()
	{
		AddXpButton.onClick.AddListener(HandleOnXpButtonClicked);
		xpStepsJsonTextAsset = Resources.Load<TextAsset>("xpSteps");
		xpSteps = JsonUtility.FromJson<XpSteps>(xpStepsJsonTextAsset.text);
		
		string resourcesPath = Path.Combine(Application.dataPath, "Resources");
		foreach (LevelStep step in xpSteps.levelSteps)
			characterImages.Add(LoadNewSprite(Path.Combine(resourcesPath, step.imageName)));
	}

	void Start()
	{
		level = 0;
		xp = 0;
		RaiseXP(0);
		DisplayLevelUp();
	}

	void Update()
	{
		float xpDelta = xpTarget - xp;
		if (xpDelta > 0)
		{
			float levelStepXp = CurrentLevelStepXp();
			
			//xp_t += Time.deltaTime / xpChangeDuration;
			//float frameXpTarget = Mathf.Lerp(previousXp, xpTarget, xp_t);
			xpDelta = Mathf.Clamp(xpDelta, 0, xpChangeDurationPerLevel * Time.deltaTime * levelStepXp);
			RaiseXP(xpDelta);
		}

		float timeSinceLevelUp = Time.time - levelUpTime;
		if (timeSinceLevelUp <= levelPlopAnimationDuration)
		{
			float scale = Mathf.Clamp(1f + (levelUpPlopFontSizeMultiplier-1f) * PlopFunc(timeSinceLevelUp / levelPlopAnimationDuration), 1f, 4f);
			LevelText.transform.localScale = new Vector3(scale, scale, scale);
		}
	}

	void HandleOnXpButtonClicked()
	{
		if (XpInputField.text != "")
		{
			xpTarget += float.Parse(XpInputField.text);
			XpInputField.text = "";
		}
	}

	void RaiseXP(float xpDelta)
	{
		xp += xpDelta;

		float levelStepXp = CurrentLevelStepXp();

		while (levelStepXp > 0 && xp >= levelStepXp)
		{
			// Level Up
			xp -= levelStepXp;
			xpTarget -= levelStepXp;
			level++;
			DisplayLevelUp();
			levelStepXp = CurrentLevelStepXp();
			levelUpTime = Time.time;
		}

		if (levelStepXp > 0)
		{
			XpProgressBar.fillAmount = xp / levelStepXp;
			XpBarProgressLabel.text = (int) xp + " / " + (int) levelStepXp + " XP";
		}
		else
		{
			XpProgressBar.fillAmount = 1;
			XpBarProgressLabel.text = "MAX LEVEL";
		}
	}

	float CurrentLevelStepXp()
	{
		int nextLevel = level + 1;
		if (xpSteps.levelSteps.Count <= nextLevel)
			return 0;
		return xpSteps.levelSteps[nextLevel].xp - (nextLevel > 0 ? xpSteps.levelSteps[nextLevel - 1].xp : 0);
	}

	void DisplayLevelUp()
	{
		// TOOD add interpolated level up ui animation
		LevelText.text = "LEVEL " + (level + 1);
		if (characterImages.Count > level)
		{
			CharacterImage.sprite = characterImages[level];
		}
	}

	public float PlopFunc(float x)
	{
		return -Mathf.Pow((x * 2 - 1),2) + 1;
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
