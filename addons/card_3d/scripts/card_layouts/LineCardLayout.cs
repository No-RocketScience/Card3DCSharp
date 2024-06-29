using Godot;

// ReSharper disable once CheckNamespace
namespace addons.card_3d.scripts.card_layouts;

[GlobalClass]
public partial class LineCardLayout : CardLayout
{
	private float _maxWidth = 20;
	[Export]
	public float MaxWidth
	{
		get => _maxWidth;
		set
		{
			_maxWidth = value;
			var halfWidth = _maxWidth / 2.0f;
			_start = new Vector3(-halfWidth, 0, 0);
			_end = new Vector3(halfWidth, 0, 0.1f);
		}
	}

	private Vector3 _start = new (-7, 0, 0);
	private Vector3 _end = new (7, 0, 0.1f);
	private float _cardWidth = 2.5f;
	[Export]
	private float _padding = 0.5f;
	
	public override Vector3 CalculateCardPositionByIndex(int numCards, int index)
	{
		var cardOffset = GetCardOffset(numCards, _cardWidth);
		var handWidth = _cardWidth + ((numCards - 1) * cardOffset);
		var startPos = GetHandStartX(handWidth, _cardWidth);

		return new Vector3(startPos + (index * cardOffset), 0, 0.001f * index);
	}
	
	// where the first card will be on the x-axis
	private float  GetHandStartX(float handWidth,  float cardSize)
	{
		return (-handWidth / 2) + (cardSize / 2);
	}


	// how far apart to set each card
	private float  GetCardOffset(int numCards, float cardSize)
	{
		// Calculate required space for cards with padding
		var totalCardSpace = cardSize * numCards;
		var totalPaddingSpace = (numCards - 1) * _padding;

		if (totalCardSpace + totalPaddingSpace <= _maxWidth)
		{
			// Cards fit within the available space without overlapping
			return cardSize + _padding;
		}

		// Cards need to overlap
		return (_maxWidth - cardSize) / (numCards - 1);
	}
}