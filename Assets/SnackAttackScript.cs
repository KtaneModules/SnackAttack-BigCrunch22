using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class SnackAttackScript : MonoBehaviour
{
	public KMAudio Audio;
    public KMBombInfo Bomb;
    public KMBombModule Module;
	
	public AudioClip[] SFX;
	public AudioSource MusicManagement;
	public KMSelectable MainSelect;
	public GameObject MainSelectAppear;
	public TextMesh StageBro;
	public SpriteRenderer[] ImageRenderers;
	public GameObject CenterDisplay;
	public GameObject[] Appear;
	public KMSelectable[] Arrows;
	public Sprite[] Food;
	public Sprite[] Warnings;
	public Sprite[] Pacman;
	public Sprite Pellets;
	public TextMesh MoveCount;
	public Sprite[] WhiteArrows;
	public KMSelectable[] VarietyOfOptions;
	public MeshRenderer[] TheseMesh;
	public Material[] OtherColors;
	public TextMesh RGB;
	public GameObject SuddenlyNext;
	public KMSelectable PlayButton;
	public KMSelectable ConfirmPlaying;
	public TextMesh StateText;

	bool Playable = false;
	
	List<int> TheColorNumber = new List<int>();
	List<int> Position = new List<int>();
	List<int> FoodNumber = new List<int>();
	List<string> MovementRecord = new List<string>();
	List<string> RGBRecord = new List<string>();
	List<int> PressRecord = new List<int>();
	Coroutine Scan, Play, PacmanLoop, MusicLoop;
	
	int PacmanSprite = 0, PacmanPosition = 1, RGBNumber = 0, GuideNumber = 1, PacmanColor = 0;
	string[] ColorOptions = {"R", "G", "B"};
	bool PlayPrompt = false;
	string[] FoodNames = {"Waffles", "Fish Steak", "Olive", "Cookie", "Apple", "Lemon Pie", "Jam", "Tomato", "Chicken Leg", "Tart", "Honey", "Peach", "Fish", "Roll", "Honeydew", "Stein", "Pepperoni", "Cantaloupe", "Strawberry", "Turnip", "Marmalade", "Whiskey", "Eggs", "Bacon", "Shrimp", "Pineapple", "Apple Pie", "Jerky", "Sushi", "Sake", "Ribs", "Pretzel", "Onion", "Honeycomb", "Steak", "Kiwi", "Avocado", "Sashimi", "Green Pepper", "Brownie", "Fish Fillet", "Sausages", "Pickle", "Watermelon", "Cheese", "Red Pepper", "Beer", "Bread", "Dragon Fruit", "Sardines", "Wine", "Moonshine", "Cherry", "Chicken", "Potato", "Eggplant", "Pumpkin Pie"};
	string[] FoodDirection = {"Left", "Middle", "Right"};
	
	//Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool ModuleSolved;
	
	void Awake()
	{
		moduleId = moduleIdCounter++;
		MainSelect.OnInteract += delegate() { PressCenter(); return false; };
		PlayButton.OnInteract += delegate() { PressPlay(); return false; };
		ConfirmPlaying.OnInteract += delegate() { ConfirmThePlay(); return false; };
		for (int k = 0; k < Arrows.Length; k++)
        {
            int Movement = k;
            Arrows[Movement].OnInteract += delegate
            {
                ArrowPress(Movement);
                return false;
            };
		}
		
		for (int a = 0; a < VarietyOfOptions.Length; a++)
        {
            int Highlight = a;
            VarietyOfOptions[Highlight].OnInteract += delegate
            {
                PressBorder(Highlight);
                return false;
            };
		}
	}
	
	void Start()
	{
		CenterDisplay.SetActive(false);
		MainSelectAppear.SetActive(false);
		foreach (GameObject a in Appear)
		{
			a.SetActive(false);
		}
		StartUp();
	}
	
	void PressBorder(int Highlight)
	{
		PlayPrompt = false;
		VarietyOfOptions[Highlight].AddInteractionPunch(.2f);
		Audio.PlaySoundAtTransform(SFX[3].name, transform);
		for (int x = 0; x < 4; x++)
		{
			TheseMesh[x].material = OtherColors[0];
		}
		PressRecord[GuideNumber-1] = Highlight;
		switch (Highlight)
		{
			case 0:
				MovementRecord[GuideNumber-1] = "left";
				break;
			case 1:
				MovementRecord[GuideNumber-1] = "none";
				break;
			case 2:
				MovementRecord[GuideNumber-1] = "right";
				break;
			case 3:
				if (MovementRecord[GuideNumber-1].EqualsAny(ColorOptions))
				{
					RGBNumber = (RGBNumber + 1)  % 3;
				}
				MovementRecord[GuideNumber-1] = ColorOptions[RGBNumber];
				RGBRecord[GuideNumber-1] = ColorOptions[RGBNumber];
				RGB.text = ColorOptions[RGBNumber];
				break;
			default:
				break;
		}
		TheseMesh[Highlight].material = OtherColors[1];
	}
	
	void ConfirmThePlay()
	{
		ConfirmPlaying.AddInteractionPunch(0.2f);
		if (PlayPrompt == true)
		{
			PlayPrompt = false;
			foreach (GameObject a in Appear)
			{
				a.SetActive(false);
			}	
			for (int x = 0; x < 9; x++)
			{
				ImageRenderers[x].sprite = null;
			}
			Debug.LogFormat("[Snack Attack #{0}] You started the game", moduleId);
			string InputProvided = "Input Provided: ";
			for (int x = 0; x < 45; x++)
			{
				switch (MovementRecord[x])
				{
					case "left":
						InputProvided += "Left";
						break;
					case "none":
						InputProvided += "None";
						break;
					case "right":
						InputProvided += "Right";
						break;
					case "R":
						InputProvided += "Red";
						break;
					case "G":
						InputProvided += "Green";
						break;
					case "B":
						InputProvided += "Blue";
						break;
					default:
						break;
				}
				if (x != 44)
				{
					InputProvided += ", ";
				}
			}
			Debug.LogFormat("[Snack Attack #{0}] {1}", moduleId, InputProvided);
			AbleToInput = false;
			Play = StartCoroutine(PlayIsNow());
		}
		
		else
		{
			PlayPrompt = true;
			Audio.PlaySoundAtTransform(SFX[3].name, transform);
			for (int x = 0; x < 4; x++)
			{
				TheseMesh[x].material = OtherColors[2];
			}
		}
	}
	
	void PressPlay()
	{
		for (int x = 0; x < 4; x++)
		{
			TheseMesh[x].material = OtherColors[0];
		}
		SuddenlyNext.SetActive(false);
		foreach (GameObject a in Appear)
		{
			a.SetActive(true);
		}
		StageBro.text = "";
		Audio.PlaySoundAtTransform(SFX[2].name, transform);
		MovementRecord.Add("");
		for (int x = 0; x < 3; x++)
		{
			ImageRenderers[(x * 3) + 2].sprite = WhiteArrows[x];
		}
		MainSelectAppear.SetActive(false);
		AbleToPressNext = false;
		AbleToCycle = false;
		AbleToInput = true;
	}
	
	void PressCenter()
	{
		MainSelect.AddInteractionPunch(.2f);
		MainSelectAppear.SetActive(false);
		StageBro.text = "";
		SuddenlyNext.SetActive(false);
		Scan = StartCoroutine(ToCycle());
	}
	
	void ArrowPress(int Movement)
	{
		PlayPrompt = false;
		Arrows[Movement].AddInteractionPunch(.2f);
		Audio.PlaySoundAtTransform(SFX[3].name, transform);
		if (Movement == 0)
		{
			if (GuideNumber > 1)
			{
				GuideNumber--;
				MoveCount.text = GuideNumber.ToString();
				for (int x = 0; x < 4; x++)
				{
					TheseMesh[x].material = OtherColors[0];
				}
				if (PressRecord[GuideNumber-1] != -1)
				{
					TheseMesh[PressRecord[GuideNumber-1]].material = OtherColors[1];
				}
				RGBNumber = Array.IndexOf(ColorOptions, RGBRecord[GuideNumber-1]);
				RGB.text = ColorOptions[RGBNumber];
			}
		}
		
		else
		{
			if (MovementRecord[GuideNumber-1] != "" && GuideNumber < 45)
			{
				GuideNumber++;
				MoveCount.text = GuideNumber.ToString();
				for (int x = 0; x < 4; x++)
				{
					TheseMesh[x].material = OtherColors[0];
				}
				if (PressRecord[GuideNumber-1] != -1)
				{
					TheseMesh[PressRecord[GuideNumber-1]].material = OtherColors[1];
				}
				RGBNumber = Array.IndexOf(ColorOptions, RGBRecord[GuideNumber-1]);
				RGB.text = ColorOptions[RGBNumber];
			}
		}
	}
	
	IEnumerator ToCycle()
	{
		Cycling = true;
		StageBro.text = "";
		yield return new WaitForSecondsRealtime(0.5f);
		for (int x = 0; x < 15; x++)
		{
			CenterDisplay.SetActive(true);
			Audio.PlaySoundAtTransform(SFX[0].name, transform);
			ImageRenderers[4].sprite = Food[TheColorNumber[x] * 19 + FoodNumber[x]];
			ImageRenderers[Position[x]*3].sprite = Warnings[0];
			yield return new WaitForSecondsRealtime(0.5f);
			ImageRenderers[4].sprite = null;
			CenterDisplay.SetActive(false);
			ImageRenderers[Position[x]*3].sprite = null;
			yield return new WaitForSecondsRealtime(0.5f);
		}
		CenterDisplay.SetActive(false);
		MainSelectAppear.SetActive(true);
		SuddenlyNext.SetActive(true);
		Cycling = false;
		AbleToPressNext = true;
	}
	
	IEnumerator PlayIsNow()
	{
		MusicManagement.clip = SFX[5];
		MusicManagement.Play();
		for (int x = 0; x < 9; x++)
		{
			ImageRenderers[x].sprite = Pellets;
		}
		ImageRenderers[(PacmanPosition * 3) + 2].sprite = Pacman[PacmanSprite];
		ImageRenderers[(PacmanPosition * 3) + 2].color = new Color (255, 255, 255);
		while (MusicManagement.isPlaying)
		{
			yield return new WaitForSecondsRealtime(0.1f);
		}
		MusicLoop = StartCoroutine(DoTheBackgroundMusic());
		PacmanLoop = StartCoroutine(PacmanAnimation());
		for (int x = 0; x < 45; x++)
		{
			for (int b = 0; b < 9; b++)
			{
				ImageRenderers[b].sprite = Pellets;
			}
			ImageRenderers[(Position[x/3] * 3) + (x % 3)].sprite = Food[(TheColorNumber[x/3] * 19) + FoodNumber[x/3]];
			
			if (MovementRecord[x] == "left")
			{
				if (PacmanPosition != 0)
				{
					ImageRenderers[(PacmanPosition * 3) + 2].sprite = Pellets;
					PacmanPosition = PacmanPosition - 1;
				}
			}
			
			else if (MovementRecord[x] == "right")
			{
				if (PacmanPosition != 2)
				{
					ImageRenderers[(PacmanPosition * 3) + 2].sprite = Pellets;
					PacmanPosition = PacmanPosition + 1;
				}
			}
			
			else if (MovementRecord[x] == "none")
			{
				ImageRenderers[(PacmanPosition * 3) + 2].sprite = Pellets;
				PacmanPosition = PacmanPosition;
			}
			
			else if (MovementRecord[x] == "R")
			{
				PacmanColor = 0;
			}
			
			else if (MovementRecord[x] == "G")
			{
				PacmanColor = 1;
			}
			
			else if (MovementRecord[x] == "B")
			{
				PacmanColor = 2;
			}
			
			if (x % 3 == 2)
			{
				if (PacmanPosition != Position[x/3])
				{
					Debug.LogFormat("[Snack Attack #{0}] Food Number {1} ({2}) was not captured.", moduleId, ((x/3)+1).ToString(), FoodNames[(TheColorNumber[x/3] * 19) + FoodNumber[x/3]]);
					StopCoroutine(PacmanLoop);
					StopCoroutine(MusicLoop);
					MusicManagement.Stop();
					for (int a = 0; a < 3; a++)
					{
						ImageRenderers[(a * 3) + 2].color = new Color (255, 255, 255);
					}
					ImageRenderers[(Position[x/3] * 3) + 2].sprite = Food[(TheColorNumber[x/3] * 19) + FoodNumber[x/3]];
					yield return new WaitForSecondsRealtime(1f);
					for (int c = 0; c < 5; c++)
					{
						ImageRenderers[(Position[x/3] * 3) + 2].sprite = Warnings[1];
						Audio.PlaySoundAtTransform(SFX[0].name, transform);
						yield return new WaitForSecondsRealtime(0.5f);
						ImageRenderers[(Position[x/3] * 3) + 2].sprite = Food[(TheColorNumber[x/3] * 19) + FoodNumber[x/3]];
						yield return new WaitForSecondsRealtime(0.5f);
					}
					StartCoroutine(LosingSound());
					break;
				}
				
				else
				{
					string[] PlacementColor = {"Red", "Green", "Blue"};
					if (PacmanColor != TheColorNumber[x/3])
					{
						Debug.LogFormat("[Snack Attack #{0}] Food Number {1} ({2}) was eaten while the player's color was {3}.", moduleId, ((x/3)+1).ToString(), FoodNames[(TheColorNumber[x/3] * 19) + FoodNumber[x/3]], PlacementColor[PacmanColor]);
						StopCoroutine(PacmanLoop);
						StopCoroutine(MusicLoop);
						MusicManagement.Stop();
						ImageRenderers[(Position[x/3] * 3) + 2].sprite = Pacman[PacmanPosition];
						yield return new WaitForSecondsRealtime(1f);
						for (int c = 0; c < 5; c++)
						{
							ImageRenderers[(Position[x/3] * 3) + 2].sprite = Warnings[1];
							Audio.PlaySoundAtTransform(SFX[0].name, transform);
							yield return new WaitForSecondsRealtime(0.5f);
							ImageRenderers[(Position[x/3] * 3) + 2].sprite = Pacman[PacmanPosition];
							yield return new WaitForSecondsRealtime(0.5f);
						}
						for (int a = 0; a < 3; a++)
						{
							ImageRenderers[(a * 3) + 2].color = new Color (255, 255, 255);
						}
						StartCoroutine(LosingSound());
						break;
					}
				}
			}
			
			if(x != 44)
			{
				yield return new WaitForSecondsRealtime(0.33f);
			}
			
			else
			{
				StopCoroutine(PacmanLoop);
				StopCoroutine(MusicLoop);
				MusicManagement.Stop();
				for (int y = 0; y < 9; y++)
				{
					ImageRenderers[y].color = new Color (255, 255, 255);
				}
				ImageRenderers[(Position[x/3] * 3) + 2].sprite = Pacman[PacmanPosition];
				yield return new WaitForSecondsRealtime(1f);
				MusicManagement.clip = SFX[10];
				MusicManagement.Play();
				for (int y = 0; y < 9; y++)
				{
					ImageRenderers[y].sprite = Pellets;
				}
				while (MusicManagement.isPlaying)
				{
					for (int j = 0; j < 2; j++)
					{
						
						for (int z = 0; z < 9; z++)
						{
							ImageRenderers[z].color = new Color (255, 255, 255);
						}
						yield return new WaitForSecondsRealtime(0.05f);
						for (int z = 0; z < 9; z++)
						{
							ImageRenderers[z].color = new Color (0, 255, 0);
						}
						yield return new WaitForSecondsRealtime(0.05f);
					}
				}
				for (int z = 0; z < 9; z++)
				{
					ImageRenderers[z].sprite = null;
				}
				yield return new WaitForSecondsRealtime(0.6f);
				StateText.text = "YOU\nWIN";
				Audio.PlaySoundAtTransform(SFX[8].name, transform);
				yield return new WaitForSecondsRealtime(0.16f);
				StateText.color = Color.green;
				Audio.PlaySoundAtTransform(SFX[9].name, transform);
				Module.HandlePass();
				Debug.LogFormat("[Snack Attack #{0}] You successfully captured every food. Module solved.", moduleId);
			}
		}
	}
	
	IEnumerator LosingSound()
	{
		Debug.LogFormat("[Snack Attack #{0}] You did not win the game. Module is giving a strike and a reset", moduleId);
		MusicManagement.clip = SFX[6];
		MusicManagement.Play();
		StateText.text = "GAME\nOVER";
		for (int x = 0; x < 9; x++)
		{
			ImageRenderers[x].sprite = null;
		}
		while (MusicManagement.isPlaying)
		{
			yield return new WaitForSecondsRealtime(0.05f);
		}
		Module.HandleStrike();
		StateText.text = "";
		TheColorNumber = new List<int>();
		Position = new List<int>();
		FoodNumber = new List<int>();
		MovementRecord = new List<string>();
		RGBRecord = new List<string>();
		PressRecord = new List<int>();
		PacmanSprite = 0;
		PacmanPosition = 1;
		RGBNumber = 0;
		GuideNumber = 1;
		PacmanColor = 0;
		AbleToCycle = true;
		StartUp();
	}
	
	IEnumerator DoTheBackgroundMusic()
	{
		while (true)
		{
			MusicManagement.clip = SFX[4];
			MusicManagement.Play();
			while (MusicManagement.isPlaying)
			{
				yield return new WaitForSecondsRealtime(0.1f);
			}
		}
	}
	
	IEnumerator PacmanAnimation()
	{
		int x = 0;
		while (true)
		{
			if (PacmanSprite > 1)
			{
				x = -1;
			}
			
			else if (PacmanSprite < 1)
			{
				x = 1;
			}
			
			PacmanSprite = PacmanSprite + x;
			for (int a = 0; a < 3; a++)
			{
				ImageRenderers[(a * 3) + 2].color = new Color (255, 255, 255);
			}
			switch (PacmanColor)
			{
				case 0:
					ImageRenderers[(PacmanPosition * 3) + 2].color = new Color (255, 0, 0);
					break;
				case 1:
					ImageRenderers[(PacmanPosition * 3) + 2].color = new Color (0, 255, 0);
					break;
				case 2:
					ImageRenderers[(PacmanPosition * 3) + 2].color = new Color (0, 0, 255);
					break;
				default:
					break;
			}
			ImageRenderers[(PacmanPosition * 3) + 2].sprite = Pacman[PacmanSprite];
			yield return new WaitForSecondsRealtime(0.05f);
		}
	}
	
	void GenerateAbsolutelyEverything()
	{
		string FoodSequence = "Food Sequence: ", FoodArrival = "Arriving Sequence: ";
		for (int y = 0; y < 15; y++)
		{
			for (int x = 0; x < 3; x++)
			{
				RGBRecord.Add("R");
				MovementRecord.Add("none");
				PressRecord.Add(-1);
			}
			Position.Add(UnityEngine.Random.Range(0,3));
			TheColorNumber.Add(UnityEngine.Random.Range(0,3));
			FoodNumber.Add(UnityEngine.Random.Range(0,19));
			FoodSequence += y != 14 ? FoodNames[(TheColorNumber[y]*19) + FoodNumber[y]] + ", " : FoodNames[(TheColorNumber[y]*19) + FoodNumber[y]];
			FoodArrival += y != 14 ? FoodDirection[Position[y]] + ", " : FoodDirection[Position[y]];
		}
		Debug.LogFormat("[Snack Attack #{0}] {1}", moduleId, FoodSequence);
		Debug.LogFormat("[Snack Attack #{0}] {1}", moduleId, FoodArrival);
	}
	
	void StartUp()
	{
		GenerateAbsolutelyEverything();
		MainSelectAppear.SetActive(true);
		StageBro.text = "Snack\nAttack";
		SuddenlyNext.SetActive(false);
	}
	
	//twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"To perform a cycle on the module, use !{0} cycle/cyclefocus | To proceed to the input section, use !{0} next | To input the commands that are going to be pressed in the game, use !{0} input <all inputs on one string> [l - left, r - right, n - none, R - red, G - green, B - blue] (THE INPUT WILL ALWAYS START ON COMMAND 1!) (Tip: If you want to erase an input on a certain command number, use the letter n on said number)  | To play the game, use !{0} proceed";
    #pragma warning restore 414
	
	bool AbleToCycle = true, AbleToPressNext = false, AbleToInput = false, Cycling = false;
	
    IEnumerator ProcessTwitchCommand(string command)
	{
		string[] parameters = command.Split(' ');
		if (Regex.IsMatch(command, @"^\s*cycle\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
			if (!AbleToCycle)
			{
				yield return "sendtochaterror You are not able to do this currently. The command was not processed.";
				yield break;
			}
			
			if (Cycling)
			{
				yield return "sendtochaterror The module is cycling. The command was not processed.";
				yield break;
			}
            MainSelect.OnInteract();
        }
		
		if (Regex.IsMatch(command, @"^\s*cyclefocus\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
			if (!AbleToCycle)
			{
				yield return "sendtochaterror You are not able to do this currently. The command was not processed.";
				yield break;
			}
			
			if (Cycling)
			{
				yield return "sendtochaterror The module is cycling. The command was not processed.";
				yield break;
			}
            MainSelect.OnInteract();
			while (Cycling)
			{
				yield return null;
			}
        }
		
		if (Regex.IsMatch(command, @"^\s*next\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
			if (!AbleToPressNext)
			{
				yield return "sendtochaterror You are not able to do this currently. The command was not processed.";
				yield break;
			}
			
			if (Cycling)
			{
				yield return "sendtochaterror The module is cycling. The command was not processed.";
				yield break;
			}
            PlayButton.OnInteract();
        }
		
		if (Regex.IsMatch(parameters[0], @"^\s*input\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
			if (!AbleToInput)
			{
				yield return "sendtochaterror You are not able to do this currently. The command was not processed.";
				yield break;
			}
			
			if (parameters.Length != 2)
			{
				yield return "sendtochaterror Parameter length invalid. The command was not processed.";
				yield break;
			}
			
			while (MoveCount.text != "1")
			{
				Arrows[0].OnInteract();
				yield return new WaitForSecondsRealtime(0.01f);
			}
			
			for (int x = 0; x < parameters[1].Length; x++)
			{
				if (x == 45)
				{
					yield return "sendtochaterror Input was longer the 45. Further commands were not processed.";
					yield break;
				}
				
				switch (parameters[1][x].ToString())
				{
					case "l":
						VarietyOfOptions[0].OnInteract();
						yield return new WaitForSecondsRealtime(0.05f);
						break;	
					case "n":
						VarietyOfOptions[1].OnInteract();
						yield return new WaitForSecondsRealtime(0.05f);
						break;
					case "r":
						VarietyOfOptions[2].OnInteract();
						yield return new WaitForSecondsRealtime(0.05f);
						break;
					case "R":
					case "B":
					case "G":
						if (PressRecord[x] != 3)
						{
							VarietyOfOptions[3].OnInteract();
							yield return new WaitForSecondsRealtime(0.05f);
						}
						while (RGB.text != parameters[1][x].ToString())
						{
							VarietyOfOptions[3].OnInteract();
							yield return new WaitForSecondsRealtime(0.05f);
						}
						break;
					default:
						yield return "sendtochaterror Current input was not able to be processed. Further commands were not processed.";
						yield break;
				}
				Arrows[1].OnInteract();
				yield return new WaitForSecondsRealtime(0.05f);
			}
        }
		
		if (Regex.IsMatch(command, @"^\s*proceed\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
			yield return null;
			if (!AbleToInput)
			{
				yield return "sendtochaterror You are not able to do this currently. The command was not processed.";
				yield break;
			}
			
			yield return "strike";
			yield return "solve";
			
			for (int x = 0; x < 2; x++)
			{
				ConfirmPlaying.OnInteract();
				yield return new WaitForSecondsRealtime(0.05f);
			}
		}
	}
}