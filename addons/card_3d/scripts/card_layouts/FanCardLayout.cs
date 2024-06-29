using System;
using Godot;

// ReSharper disable once CheckNamespace
namespace addons.card_3d.scripts.card_layouts;

[GlobalClass]
public partial class FanCardLayout : CardLayout
{

	private double _arcAngle = double.Pi/2;
	[Export(PropertyHint.Range, "5,180,radians_as_degrees")]
	public double ArcAngle
	{
		get => _arcAngle;
		set
		{
			_arcAngle = value;
			_startAngle = double.Pi / 2 + ArcAngle / 2;
		}
	}

	private double _arcRadius = 7;
	private double _startAngle;

	public FanCardLayout()
	{
		_startAngle = double.Pi/2 + ArcAngle / 2;
	}
	
	public override Vector3 CalculateCardPositionByIndex(int numCards, int index)
	{
		var angleStep = ArcAngle / (numCards + 1);

		var angle = _startAngle - ((index + 1) * angleStep);
		var x = (float)(_arcRadius * Math.Cos(angle));
		var y = (float)((_arcRadius * Math.Sin(angle)) - _arcRadius);
		var position = new Vector3(x, y, 0.001f * (index + 1));

		return position;
	}

	public override Vector3 CalculateCardRotationByIndex(int numCards, int index)
	{
		var angleStep = ArcAngle / (numCards + 1);
		var angle = _startAngle - ((index + 1) * angleStep);
		var rotationQuaternion = new Quaternion(new Vector3(0, 0, 1), (float)(angle - float.Pi / 2));
		return rotationQuaternion.GetEuler();
	}
}