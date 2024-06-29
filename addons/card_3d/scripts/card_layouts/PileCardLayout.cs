using Godot;
using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace addons.card_3d.scripts.card_layouts;

[GlobalClass]
public partial class PileCardLayout : CardLayout
{
	[Export]
	private float _pileYOffset;

	public override Vector3  CalculateCardPositionByIndex(int numCards, int index)
	{
		return new Vector3(0, (numCards - index) * (-_pileYOffset), 0.01f * index);
	}

	public override List<Vector3> CalculateCardRotations(int numCards)
	{
		return base.CalculateCardRotations(numCards);
	}
}
