using System.Collections.Generic;
using Godot;
using Godot.Collections;

// ReSharper disable once CheckNamespace
namespace addons.card_3d.scripts.card_layouts;

[GlobalClass]
public partial class CardLayout : Resource
{
	public void UpdateCardPositions(List<Card3D> cards, float duration)
	{
		var positions = CalculateCardPositions(cards.Count);
		var rotations = CalculateCardRotations(cards.Count);

		for (int i = 0; i < cards.Count; i++)
		{
			var card = cards[i];
			if (card == null)
			{
				continue;
			}

			card.AnimateToPosition(positions[i], duration);
			card.DraggingRotation(rotations[i]);
		}
	}

	public void UpdateCardPosition(Card3D card, int numCards, int index, float duration)
	{
		var position = CalculateCardPositionByIndex(numCards, index);
		var rotation = CalculateCardRotationByIndex(numCards, index);
		card.AnimateToPosition(position, duration);
		card.DraggingRotation(rotation);
	}

	public virtual List<Vector3> CalculateCardPositions(int numCards)
	{
		List<Vector3> positions = [];
		for (int i = 0; i < numCards; i++)
		{
			positions.Add(CalculateCardPositionByIndex(numCards, i));
		}

		return positions;
	}

	public virtual Vector3 CalculateCardPositionByIndex(int numCards, int index)
	{
		return Vector3.Zero;
	}

	public virtual List<Vector3> CalculateCardRotations(int numCards)
	{
		List<Vector3> rotations = [];
		for (int i = 0; i < numCards; i++)
		{
			rotations.Add(CalculateCardRotationByIndex(numCards, i));
		}

		return rotations;
	}

	public virtual Vector3 CalculateCardRotationByIndex(int numCards, int index)
	{
		return Vector3.Zero;
	}
}