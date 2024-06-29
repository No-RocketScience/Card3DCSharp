using Godot;
using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace addons.card_3d.scripts;

public partial class FaceCards : Resource
{
	public enum Rank {
		Two = 2,
		Three = 3,
		Four = 4,
		Five = 5,
		Six = 6,
		Seven = 7,
		Eight = 8,
		Nine = 9,
		Ten = 10,
		Jack = 11,
		Queen = 12,
		King = 13,
		Ace = 14,
	}

	public enum Suit {
		Heart,
		Diamond,
		Club,
		Spade,
	}

	public readonly Dictionary<string, CardData> Data = GenerateAllFaceCards();
	
	
	public CardData GetCardData(Rank rank, Suit suit)
	{
		return Data.GetValueOrDefault( GetCardId(rank, suit));
	}

	public static string GetCardId(Rank rank, Suit suit)
	{
		return $"{rank} of {suit}";
	}
	
	public static Dictionary<string, CardData> GenerateAllFaceCards()
	{
		var data = new Dictionary<string, CardData>();

		foreach (var suit in Enum.GetValues<Suit>())
		{
			foreach (var rank in Enum.GetValues<Rank>())
			{
				var frontMaterial = $"res://example/materials/{suit.ToString().ToLower()}-{(int)rank}.tres";
				var backMaterial = "res://example/materials/card-back.tres";
				var cardData = new CardData
				{
					Rank = rank,
					Suit = suit,
					FrontMaterialPath = frontMaterial,
					BackMaterialPath = backMaterial,
				};
				var cardId = GetCardId(rank, suit);
				data[cardId] = cardData;
			}
		}

		return data;
	}
}

public partial class CardData: Resource
{
	public FaceCards.Rank Rank;
	public FaceCards.Suit Suit;
	public string FrontMaterialPath;
	public string BackMaterialPath;
}
