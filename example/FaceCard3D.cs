using Godot;
using System;

// ReSharper disable once CheckNamespace
namespace addons.card_3d.scripts;
public partial class FaceCard3D : Card3D
{
	[Export]
	private CardData _data;
	
	public CardData Data
	{
		get => _data;
		set
		{
			_data = value;
			Rank = _data.Rank;
			Suit = _data.Suit;
			_frontMaterialPath = _data.FrontMaterialPath;
			_backMaterialPath = _data.BackMaterialPath;
		}
	}

	[Export]
	public FaceCards.Rank Rank = FaceCards.Rank.Two;
	[Export]
	public FaceCards.Suit Suit = FaceCards.Suit.Diamond;
	
	private string _frontMaterialPath;
	[Export]
	public string FrontMaterialPath
	{
		get => _frontMaterialPath;
		set
		{
			_frontMaterialPath = value;
			if (value.Length == 0)
			{
				return;
			}
			
			var material = GD.Load<Material>(_frontMaterialPath);
			if (material != null && _frontMesh != null)
			{
				_frontMesh.SetSurfaceOverrideMaterial(0, material);
			}
		}
	}

	private string _backMaterialPath;
	[Export]
	public string BackMaterialPath
	{
		get => _backMaterialPath;
		set
		{
			_backMaterialPath = value;
			if (value.Length == 0)
			{
				return;
			}

			var material = GD.Load<Material>(_backMaterialPath);
			if (material != null && _backMesh != null)
			{
				_backMesh.SetSurfaceOverrideMaterial(0, material);
			}
		}
	}

	private MeshInstance3D _backMesh;
	private MeshInstance3D _frontMesh;
		
	public override void _Ready()
	{
		base._Ready();
		_backMesh = GetNode<MeshInstance3D>("%CardBackMesh");
		_frontMesh = GetNode<MeshInstance3D>("%CardFrontMesh");
		
		if(_frontMaterialPath.Length > 0)
		{
			var material = GD.Load<Material>(_frontMaterialPath);
			if (material != null && _frontMesh != null)
			{
				_frontMesh.SetSurfaceOverrideMaterial(0, material);
			}
		}

		if (_backMaterialPath.Length > 0)
		{
			var material = GD.Load<Material>(_backMaterialPath);
			if (material != null && _backMesh != null)
			{
				_backMesh.SetSurfaceOverrideMaterial(0, material);
			}
		}
	}

	public override string ToString()
	{
		return $"{Rank} of {Suit}";
	}
}
