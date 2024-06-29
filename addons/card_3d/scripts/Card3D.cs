using System;
using Godot;

// ReSharper disable once CheckNamespace
namespace addons.card_3d.scripts;

public partial class Card3D : Node3D
{
    [Signal]
    public delegate void  Card3DMouseDownEventHandler(Card3D card);
    [Signal]
    public delegate void  Card3DMouseUpEventHandler(Card3D card);
    [Signal]
    public delegate void  Card3DMouseOverEventHandler(Card3D card);
    [Signal]
    public delegate void  Card3DMouseExitEventHandler(Card3D card);
    
    [Export] 
    public float HoverScaleFactor = 1.15f;
    [Export] 
    public Vector3 HoverPosMove = new Vector3(0, 0.7f, 0);
    [Export] 
    public float MoveTweenDuration = 0.08f;
    [Export] 
    public float RotateTweenDuration = 0.15f;
    [Export] 
    public bool FaceDown
    {
        get => _isFaceDown;
        set
        {
            _isFaceDown = value;
            GetNode<Node3D>("CardMesh").Rotation = _isFaceDown ? new Vector3(0, (float)Math.PI, 0) : Vector3.Zero;
        }
    }

    private Node3D _cardMesh;
    private CollisionShape3D _collisionShape;
    private StaticBody3D _staticBody;
    private Tween _positionTween;
    private Tween _rotateTween;
    private Tween _hoverTween;
    private bool _isFaceDown;

    public override void _Ready()
    {
        _cardMesh = GetNode<Node3D>("%CardMesh");
        _staticBody = GetNode<StaticBody3D>("%StaticBody3D");
        _collisionShape = GetNode<CollisionShape3D>("%CollisionShape3D");
        
        _staticBody.MouseEntered += StaticBodyOnMouseEntered;
        _staticBody.MouseExited += StaticBodyOnMouseExited;
        _staticBody.InputEvent += StaticBodyOnInputEvent;
    }
    public void DisableCollision()
    {
        _collisionShape.Disabled = true;
    }

    public void EnableCollision()
    {
        _collisionShape.Disabled = false;
    }

    public void SetHovered()
    {
        if (_hoverTween != null && _hoverTween.IsRunning())
        {
            _hoverTween.Kill();
        }

        _hoverTween = CreateTween();
        _hoverTween.SetParallel();
        _hoverTween.SetEase(Tween.EaseType.In);
        TweenCardScale(HoverScaleFactor);
        TweenMeshPosition(HoverPosMove);
    }

    public void RemoveHovered()
    {
        if (_hoverTween != null && _hoverTween.IsRunning())
        {
            _hoverTween.Kill();
        }

        _hoverTween = CreateTween();
        _hoverTween.SetParallel();
        _hoverTween.SetEase(Tween.EaseType.In);
        TweenCardScale(1);
        TweenMeshPosition(Vector3.Zero);
    }

    public void DraggingRotation(Vector3 dragRotation)
    {
        if (_rotateTween != null && _rotateTween.IsRunning())
        {
            _rotateTween.Kill();
        }

        _rotateTween = CreateTween();
        TweenCardRotation(dragRotation);
    }
    
    public Tween  AnimateToPosition(Vector3 newPosition, float duration = 0)
    {
        if (duration == 0)
        {
            duration = MoveTweenDuration;
        }

        if (_positionTween != null && _positionTween.IsRunning())
        {
            _positionTween.Kill();
        }

        var position = Position;
        position.Z = newPosition.Z;// set z to prevent transition spring from making card go below another card
        Position = position;
        _positionTween = CreateTween();
        _positionTween.SetEase(Tween.EaseType.Out);
        _positionTween.SetTrans(Tween.TransitionType.Spring);
        TweenCardPosition(newPosition, duration);
        return _positionTween;
    }

    private void TweenCardScale(float scale)
    {
        var targetScale = new Vector3(scale, scale, 1);
        _hoverTween.TweenProperty(_cardMesh, "scale", targetScale, MoveTweenDuration);
    }

    private void TweenMeshPosition(Vector3 position)
    {
        _hoverTween.TweenProperty(_cardMesh, "position", position, MoveTweenDuration);
    }

    private void TweenCardPosition(Vector3 position, float duration)
    {
        _positionTween.TweenProperty(this, "position", position, duration);
    }


    private void TweenCardRotation(Vector3 targetRotation)
    {
        _rotateTween.SetEase(Tween.EaseType.In);
        _rotateTween.TweenProperty(this, "rotation", targetRotation, RotateTweenDuration);
    }

    private void StaticBodyOnInputEvent(Node camera, InputEvent @event, Vector3 position, Vector3 normal, long shapeidx)
    {
        if (@event is not InputEventMouseButton mouseEvent)
        {
            return;
        }

        if (mouseEvent is { ButtonIndex: MouseButton.Left, Pressed: true })
        {
            EmitSignal(SignalName.Card3DMouseDown, this);
        }
        else if (mouseEvent is { ButtonIndex: MouseButton.Left, Pressed: false })
        {
            EmitSignal(SignalName.Card3DMouseUp, this);
        }
    }


    private void StaticBodyOnMouseExited()
    {
        EmitSignal(SignalName.Card3DMouseExit, this);
    }

    private void StaticBodyOnMouseEntered()
    {
        EmitSignal(SignalName.Card3DMouseOver, this);
    }

}