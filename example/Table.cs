using Godot;
using System;
using addons.card_3d.scripts.card_layouts;

// ReSharper disable once CheckNamespace
namespace addons.card_3d.scripts;
public partial class Table : Node3D
{
	private FaceCards _cardDatabase = new ();
	private CardCollection3D _hand;
	private CardCollection3D _pile;
	private static readonly PackedScene _faceCardScene = GD.Load<PackedScene>("res://example/face_card_3d.tscn");
	[Export]
	private FaceCard3D _deck;
	
	public override void _Ready()
	{
        _hand = GetNode<CardCollection3D>("DragController/Hand");
        _pile = GetNode<CardCollection3D>("DragController/TableCards");
        _deck.Card3DMouseUp += _ => AddCard();
	}
	

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_down"))
		{
			AddCard();
		}
		else if (@event.IsActionPressed("ui_up"))
		{
			RemoveCard();
		}
		else if (@event.IsActionPressed("ui_left"))
		{
			ClearCards();
		}else if (@event.IsActionPressed("ui_right"))
		{
			if (_pile.CardLayoutStrategy is PileCardLayout && _hand.CardLayoutStrategy is LineCardLayout)
			{
				
			}else if (_hand.CardLayoutStrategy is LineCardLayout)
			{
				_hand.CardLayoutStrategy = new FanCardLayout();
			}
			else if (_pile.CardLayoutStrategy is LineCardLayout)
			{
				_pile.CardLayoutStrategy = new PileCardLayout();
			}
			else if(_hand.CardLayoutStrategy is FanCardLayout)
			{
				_hand.CardLayoutStrategy = new LineCardLayout();
			}
		}
	}
	
	private FaceCard3D InstantiateFaceCard(FaceCards.Rank rank,FaceCards.Suit suit)
	{
		var faceCard3D = _faceCardScene.Instantiate<FaceCard3D>();
		var cardData = _cardDatabase.GetCardData(rank, suit);
		faceCard3D.Rank = cardData.Rank;
		faceCard3D.Suit = cardData.Suit;
		faceCard3D.FrontMaterialPath = cardData.FrontMaterialPath;
		faceCard3D.BackMaterialPath = cardData.BackMaterialPath;

		return faceCard3D;
	}
	
	private void AddCard()
	{
		var data = NextCard();
		var card = InstantiateFaceCard(data.Rank, data.Suit);
		if(!_hand.CanInsertCard(card, null))
		{
			return;
		}
		
		_hand.AppendCard(card);
		card.GlobalPosition = _deck.GlobalPosition;
	}

	private CardData NextCard()
	{
		var suit = Enum.GetValues<FaceCards.Suit>()[Random.Shared.Next(0, Enum.GetValues<FaceCards.Suit>().Length - 1)];
		var rank = Enum.GetValues<FaceCards.Rank>()[Random.Shared.Next(0, Enum.GetValues<FaceCards.Rank>().Length - 1)];
		return new CardData
		{
			Rank = rank,
			Suit = suit,
		};
	}
	
	private void RemoveCard()
	{
		if (_hand.Cards.Count == 0)
		{
			return;
		}

		var randomCardIndex = Random.Shared.Next(0,_hand.Cards.Count -1);
		var cardToRemove = _hand.Cards[randomCardIndex];
		PlayCard(cardToRemove);
	}
	
	private void PlayCard(Card3D card)
	{
		var cardIndex = _hand.CardIndices[card];
		var cardGlobalPosition = _hand.Cards[cardIndex].GlobalPosition;
		var c = _hand.RemoveCard(cardIndex);

		_pile.AppendCard(c);
		c.RemoveHovered();
		c.GlobalPosition = cardGlobalPosition;
	}
	


	private void ClearCards()
	{
		var handCards = _hand.RemoveAll();
		var pileCards = _pile.RemoveAll();

		foreach (var c in handCards)
		{
			c.QueueFree();
		}
		foreach (var c in pileCards)
		{
			c.QueueFree();
		}
	}
}
