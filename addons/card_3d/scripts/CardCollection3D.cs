using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using addons.card_3d.scripts.card_layouts;

// ReSharper disable once CheckNamespace
namespace addons.card_3d.scripts;

public partial class CardCollection3D : Node3D
{
	[Signal]
	public delegate void MouseEnterDropZoneEventHandler(CardCollection3D cardCollection3D);
	[Signal]
	public delegate void MouseExitDropZoneEventHandler(CardCollection3D cardCollection3D);
	[Signal]
	public delegate void CardSelectedEventHandler(Card3D card, CardCollection3D cardCollection3D);
	[Signal]
	public delegate void CardClickedEventHandler(Card3D card, CardCollection3D cardCollection3D);
	[Signal]
	public delegate void CardAddedEventHandler(Card3D card, CardCollection3D cardCollection3D);
    
    
	private CollisionShape3D _dropzoneCollision;
	private StaticBody3D _dropZone;

	[Export]
	private bool _highlightOnHover = true;
	[Export]
	private float _cardMoveTweenDuration = .25f;
	[Export]
	public float CardSwapTweenDuration = .25f;
	
	private CardLayout _cardLayoutStrategy = new LineCardLayout();
	[Export]
	public CardLayout CardLayoutStrategy
	{
		get => _cardLayoutStrategy;
		set
		{
			_cardLayoutStrategy = value;
			ApplyCardLayout();
		}
	}
	
	private Shape3D _dropzoneCollisionShape = DefaultCollisionShape;
	[Export]
	public Shape3D DropzoneCollisionShape
	{
		get => _dropzoneCollisionShape;
		set
		{
			_dropzoneCollisionShape = value;
			if (value != null && _dropzoneCollision != null)
			{
				_dropzoneCollision.Shape = _dropzoneCollisionShape;
			}
		}
	}
	
	private float _dropzoneZOffset = 1.6f;
	[Export]
	public float DropzoneZOffset
	{
		get => _dropzoneZOffset;
		set
		{
			_dropzoneZOffset = value;
			if(_dropZone != null)
			{
				var dropZonePosition = _dropZone.Position;
				dropZonePosition.Z = _dropzoneZOffset;
				_dropZone.Position = dropZonePosition;
			}
		}
	}

	public List<Card3D> Cards = [];
	public Dictionary<Card3D, int> CardIndices = new();

	public bool HoverDisabled; // disable card hover animation (useful when dragging other cards around)
	private Card3D _hoveredCard; // card currently hovered
	private int _previewDropIndex = -1;

	public override void _Ready()
	{
		_dropZone = GetNode<StaticBody3D>("DropZone");
		_dropzoneCollision = _dropZone.GetNode<CollisionShape3D>("CollisionShape3D");
		_dropzoneCollision.Shape = _dropzoneCollisionShape;
		var dropZonePosition = _dropZone.Position;
		dropZonePosition.Z = _dropzoneZOffset;
		_dropZone.Position = dropZonePosition;
		_dropZone.MouseEntered += OnDropZoneMouseEntered;
		_dropZone.MouseExited += OnDropZoneMouseExited;
	}
	
// add a card to the hand and animate it to the correct position
// this will add card as child of this node
	public void AppendCard(Card3D card)
	{
		InsertCard(card, Cards.Count);
	}
	
	public void PrependCard(Card3D card)
	{
		InsertCard(card, 0);
	}

	public void InsertCard(Card3D card,int index)
	{
		card.Card3DMouseDown += OnCardPressed;
		card.Card3DMouseUp += OnCardClicked;
		card.Card3DMouseOver += OnCardHover;
		card.Card3DMouseExit += OnCardExit;

		Cards.Insert(index, card);
		AddChild(card);

		for(var i = 0; i < Cards.Count; i++)
		{
			CardIndices[Cards[i]] = i;
		}

		ApplyCardLayout();
		EmitSignal(SignalName.CardAdded, card, this);
	}


	// remove and return card from the end of the list
	public Card3D PopCard()
	{
		return RemoveCard(Cards.Count - 1);
	}


	// remove and return card from the beggining of the list
	public Card3D ShiftCard()
	{
		return RemoveCard(0);
	}
	
	// remove card from this hand and return it.
	// the caller is responsible for adding card elsewhere
	// and/or calling queue_free on it
	public Card3D RemoveCard(int index)
	{
		var removedCard = Cards[index];
		Cards.RemoveAt(index);
		CardIndices.Remove(removedCard);

		
		for(var i = 0; i < Cards.Count; i++)
		{
			CardIndices[Cards[i]] = i;
		}

		RemoveChild(removedCard);
		ApplyCardLayout();
			
		removedCard.Card3DMouseDown -= OnCardPressed;
		removedCard.Card3DMouseUp -= OnCardClicked;
		removedCard.Card3DMouseOver -= OnCardHover;
		removedCard.Card3DMouseExit -= OnCardExit;

		return removedCard;
	}
	
	// remove and return all cards
	public List<Card3D> RemoveAll()
	{
		var cardsToReturn = Cards;
		Cards = [];
		CardIndices = new Dictionary<Card3D, int>();

		foreach(var c in cardsToReturn)
		{
			RemoveChild(c);
		}

		return cardsToReturn;
	}

	public void ApplyCardLayout()
	{
		CardLayoutStrategy.UpdateCardPositions(Cards, _cardMoveTweenDuration);
	}
	
	public void PreviewCardRemove(Card3D draggingCard)
	{
		if (!CardIndices.TryGetValue(draggingCard, out var cardIndex))
		{
			return;
		}

		List<Card3D> previewCards = [];
		previewCards.AddRange(Cards[..cardIndex]);
		var end = Cards.Count - (cardIndex + 2);
		previewCards.AddRange(Cards.Slice(cardIndex + 1, end < 0 ? 0 : end));

		CardLayoutStrategy.UpdateCardPositions(previewCards, CardSwapTweenDuration);
	}


	public void PreviewCardDrop(Card3D draggingCard, int index)
	{
		if(index == _previewDropIndex)
		{
			return;
		}

		_previewDropIndex = index;
		List<Card3D> previewCards = [];

		if (CardIndices.TryGetValue(draggingCard, out var currentIndex))
		{
			// dragging card in the current collection
			index = Math.Clamp(index, 0, Cards.Count - 1);
			previewCards.AddRange(Cards[..currentIndex]);
			previewCards.AddRange(Cards[(currentIndex + 1)..]);
			previewCards.Insert(index, null);
		}
		else
		{
			// dragging new card in from another collection
			previewCards.AddRange(Cards[..index]);
			previewCards.Add(null);
			previewCards.AddRange(Cards[index..]);
		}
		
		CardLayoutStrategy.UpdateCardPositions(previewCards, CardSwapTweenDuration);
	}
	
	public void EnableDropZone()
	{
		_previewDropIndex = -1;
		_dropzoneCollision.Disabled = false;
	}


	public void DisableDropZone()
	{
		_previewDropIndex = -1;
		_dropzoneCollision.Disabled = true;
	}


	public void OnDragHover(Card3D draggingCard, Vector2 mousePosition)
	{
		var indexToDrop = GetCardIndexAtPoint(mousePosition);
		PreviewCardDrop(draggingCard, Math.Max(indexToDrop, 0));
	}
	
	public int GetCardIndexAtPoint(Vector2 mousePosition)
	{
		var camera = GetWindow().GetCamera3D();
		var index = Cards.Count;
		// iterate cards until finding screen position after mouse position
		// this is the index where we will add card
		foreach (var card in Cards)
		{
			var cardIndex = CardIndices[card];
			var cardPosition = CardLayoutStrategy.CalculateCardPositionByIndex(Cards.Count, cardIndex);
			var cardScreenPosition = camera.UnprojectPosition(cardPosition);
			if (mousePosition.X < cardScreenPosition.X)
			{
				index = CardIndices[card];
				break;
			}
		}

		return index;
	}
	
	
	// when a mouse enters card collision
	// set hover state, if applicable
	private void OnCardHover(Card3D card)
	{
		if (HoverDisabled || !CanSelectCard(card))
		{
			return;
		}

		_hoveredCard = card;
		if (_highlightOnHover)
		{
			card.SetHovered();
		}
	}
	
	private void OnCardExit(Card3D card)
	{
		if (HoverDisabled || _hoveredCard != card)
		{
			return;
		}

		card.RemoveHovered();
		_hoveredCard = null;
	}


	private void OnCardPressed(Card3D card)
	{
		if(CanSelectCard(card))
		{
			EmitSignal(SignalName.CardSelected, card, this);
		}
	}
		
	private void OnCardClicked(Card3D card)
	{
		EmitSignal(SignalName.CardClicked, card, this);
	}


	private void OnDropZoneMouseEntered()
	{
		EmitSignal(SignalName.MouseEnterDropZone, this);
	}


	private void OnDropZoneMouseExited()
	{
		_previewDropIndex = -1;
		EmitSignal(SignalName.MouseExitDropZone, this);
	}


	private static Shape3D DefaultCollisionShape => new ConvexPolygonShape3D{Points = [
		new Vector3(-7, 2, 0),
		new Vector3(-7, -2, 0),
		new Vector3(7, -2, 0),
		new Vector3(7, 2, 0),
	]};

		
	// whether or not a card can be selected
	public virtual bool CanSelectCard(Card3D card) => true;

	public virtual bool CanRemoveCard(Card3D card) => true;

	public virtual bool CanReorderCard(Card3D card) => true;

	// if the card can be inserted to the collection
	public virtual bool CanInsertCard(Card3D card, CardCollection3D fromCollection) => true;
}
