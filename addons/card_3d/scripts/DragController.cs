using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace addons.card_3d.scripts;
public partial class DragController : Node3D
{
	[Signal]
	public delegate void DragStartedEventHandler(Card3D card);
	[Signal]
	public delegate void CardMovedEventHandler(Card3D card, CardCollection3D fromCollection,CardCollection3D toCollection, int fromIndex, int toIndex);


	[Export]
	private int _maxDragYRotationDeg = 65;
	[Export]
	private int _maxDragXRotationDeg = 65;


	[Export]
	private Plane _cardDragPlane = new (new Vector3(0, 0, 1), 1.5f);

	
	private Camera3D _camera; // camera used for determining where mouse is on drag plane
	private Card3D _draggingCard; // card that is being dragged
	private CardCollection3D _dragFromCollection; // collection card being dragged from
	private bool _dragging;
	private CardCollection3D _hoveredCollection; // collection about to drop card into
	private readonly List<CardCollection3D> _cardCollections = [];

	public override void _Ready()
	{
		var window = GetWindow();
		_camera = window.GetCamera3D();

		_cardCollections.AddRange(GetChildren().OfType<CardCollection3D>()); 
		foreach (var cardCollection in _cardCollections)
		{
			
			cardCollection.CardSelected += OnCollectionCardSelected;
			cardCollection.MouseEnterDropZone += OnCollectionMouseEnterDropZone;
			cardCollection.MouseExitDropZone += OnCollectionMouseExitDropZone;
		}
	}


	public override void _Input(InputEvent @event)
	{
		switch (@event)
		{
			case InputEventMouseButton eventMouseButton:
			{
				if (_dragging && eventMouseButton.ButtonIndex == MouseButton.Left && !eventMouseButton.Pressed)
				{
					var m = GetViewport().GetMousePosition();
					StopDrag(m);
				}

				break;
			}
			case InputEventMouseMotion eventMouseMotion:
			{
				if (_dragging)
				{
					HandleDragEvent(eventMouseMotion);
				}

				break;
			}
		}
	}
	
	private void OnCollectionCardSelected(Card3D card, CardCollection3D collection)
	{
		DragCardStart(card, collection);
	}


	private void OnCollectionMouseEnterDropZone(CardCollection3D collection)
	{
		_hoveredCollection = collection;
	}


	private void OnCollectionMouseExitDropZone(CardCollection3D collection)
	{
		if (_hoveredCollection != _dragFromCollection)
		{
			_hoveredCollection.ApplyCardLayout();
		}
		else
		{
			_hoveredCollection.PreviewCardRemove(_draggingCard);
			_hoveredCollection = null;
		}
	}
	
	private void ReturnCardToCollection(Vector2 mousePosition)
	{
		if (_dragFromCollection.CanReorderCard(_draggingCard))
		{
			var currentIndex = _dragFromCollection.CardIndices[_draggingCard];
			var newIndex = _dragFromCollection.GetCardIndexAtPoint(mousePosition);
			newIndex = Math.Clamp(newIndex, 0, _dragFromCollection.Cards.Count - 1);

			if (currentIndex != newIndex)
			{
				_dragFromCollection.RemoveCard(currentIndex);
				_dragFromCollection.InsertCard(_draggingCard, newIndex);
				EmitSignal(SignalName.CardMoved,_draggingCard, _dragFromCollection, _dragFromCollection, currentIndex, newIndex);
			}
		}

		_dragFromCollection.ApplyCardLayout();
	}
	
	private void DropCardToAnotherCollection(Vector2 mousePosition)
	{
		if (!_hoveredCollection.CanInsertCard(_draggingCard, _dragFromCollection))
		{
			return;
		}

		var cardIndex = _dragFromCollection.CardIndices[_draggingCard];
		var cardGlobalPosition = _dragFromCollection.Cards[cardIndex].GlobalPosition;
		var card = _dragFromCollection.RemoveCard(cardIndex);

		if (_hoveredCollection.CanReorderCard(card))
		{
			var index = _hoveredCollection.GetCardIndexAtPoint(mousePosition);
			_hoveredCollection.InsertCard(card, index);
			EmitSignal(SignalName.CardMoved,_draggingCard, _dragFromCollection, _dragFromCollection, cardIndex, index);
		}
		else
		{
			_hoveredCollection.AppendCard(card);
			EmitSignal(SignalName.CardMoved,_draggingCard, _dragFromCollection, _dragFromCollection, cardIndex,  _hoveredCollection.Cards.Count - 1);
		}

		card.RemoveHovered();
		card.GlobalPosition = cardGlobalPosition;
	}
	
	private void DragCardStart(Card3D card, CardCollection3D dragFromCollection)
	{
		_dragging = true;
		_dragFromCollection = dragFromCollection;
		_draggingCard = card;
		_draggingCard.DisableCollision();
		_draggingCard.RemoveHovered();

		_dragFromCollection.EnableDropZone();

		foreach (var collection in _cardCollections)
		{
			if (collection.CanInsertCard(_draggingCard, _dragFromCollection))
			{
				collection.EnableDropZone();
			}

			collection.HoverDisabled = true;
		}

		EmitSignal(SignalName.DragStarted);
	}
	
	private void StopDrag(Vector2 mousePosition)
	{
		var canInsert = true;
		if (_hoveredCollection != null)
		{
			canInsert = _hoveredCollection.CanInsertCard(_draggingCard, _dragFromCollection);
		}

		if (!canInsert)
		{
			ReturnCardToCollection(mousePosition);
		}
		
		if (_hoveredCollection == null || _hoveredCollection == _dragFromCollection)
		{
			ReturnCardToCollection(mousePosition);
		}
		else if (_hoveredCollection != null && _hoveredCollection != _dragFromCollection)
		{
			DropCardToAnotherCollection(mousePosition);
		}

		_dragFromCollection.DisableDropZone();
		_draggingCard.EnableCollision();

		_dragging = false;
		_draggingCard = null;
		_dragFromCollection = null;

		foreach (var collection in _cardCollections)
		{
			collection.DisableDropZone();
			collection.HoverDisabled = false;
		}
	}
	
	private void HandleDragEvent(InputEventMouseMotion @event)
	{
		var m = GetViewport().GetMousePosition();
		var position3D = _cardDragPlane.IntersectsRay(_camera.ProjectRayOrigin(m),
			_camera.ProjectRayNormal(m));
		if (!position3D.HasValue)
		{
			return;
		}
		
		var cardPosition = _draggingCard.GlobalPosition;

		var xDistance = position3D.Value.X - cardPosition.X;
		var yDistance = position3D.Value.Y - cardPosition.Y;

		// add rotation to make dragging cards pretty
		// rotate around y-axis for horizontal rotation
		var yDegrees = xDistance * 25f;
		yDegrees = Math.Clamp(yDegrees, -_maxDragYRotationDeg, _maxDragYRotationDeg);

		// rotate around x-axis for vertical rotation
		var xDegrees = -yDistance * 25f;
		xDegrees = Math.Clamp(xDegrees, -_maxDragXRotationDeg, _maxDragXRotationDeg);
		float zDegrees = 0;

		// put degrees in Vector3
		var targetRotation = new Vector3(
			Mathf.DegToRad(xDegrees),
			Mathf.DegToRad(yDegrees),
			Mathf.DegToRad(zDegrees)
		);
		
		// set rotation
		_draggingCard.DraggingRotation(targetRotation);

		// set card position to under mouse
		_draggingCard.GlobalPosition  = position3D.Value;

		if (_hoveredCollection != null && _hoveredCollection.CanReorderCard(_draggingCard))
		{
			var dragScreenPoint = GetDragScreenPoint(position3D)!.Value;
			_hoveredCollection.OnDragHover(_draggingCard, dragScreenPoint);
		}
	}
	
	private Vector2? GetDragScreenPoint(Vector3? worldPosition)
	{
		if (worldPosition != null)
		{
			return _camera.UnprojectPosition(worldPosition.Value);
		}

		return null;
	}
}
