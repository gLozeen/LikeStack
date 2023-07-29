using Godot;
using Godot.NativeInterop;
using System;

public class Range<T> : IComparable<T>, IComparable where T : IComparable<T>
{
	#region Variables
	private readonly T _lower;
	private readonly T _upper;
	public T LowerBound
	{
		get { return _lower; }
	}
	public T UpperBound
	{
		get { return _upper; }
	}
	#endregion
	internal Range(T lower, T upper)
	{
		_lower = lower;
		_upper = upper;
	}
	public bool Contains(T value)
	{
		return (LowerBound.CompareTo(value) <= 0) && (UpperBound.CompareTo(value) >= 0);
	}
	public bool Overlaps(Range<T> value)
	{
		return (Contains(value.LowerBound)||Contains(value.UpperBound)||value.Contains(LowerBound)||value.Contains(UpperBound));
	}
	public Range<T> Intersect(Range<T> value)
	{
		if (Overlaps(value))
		{
			T StartOfNewRange;
			if (LowerBound.CompareTo(value.LowerBound) < 0)
			{
				StartOfNewRange = value.LowerBound;
			} else
			{
				StartOfNewRange = LowerBound;
			}
			if(UpperBound.CompareTo(value.UpperBound) > 0)
			{
				return new Range<T>(StartOfNewRange, value.UpperBound);
			} else
			{
				return new Range<T>(StartOfNewRange, UpperBound);
			}
		}else
		{
			throw new InvalidOperationException(string.Format("Cannot intersect with {0}", value));
		}
	}
	public int CompareTo(object obj)
	{
		if (obj is Range<T>)
		{
			Range<T> other = (Range<T>)obj;
			return CompareTo(other);
		}
		else if (obj is T)
		{
			T other = (T)obj;
			return CompareTo(other);
		}

		throw new InvalidOperationException(string.Format("Cannot compare to {0}", obj));
	}
	public int CompareTo(T other)
	{
		return LowerBound.CompareTo(other);
	}
}
public partial class main_script : Node3D
{
	#region Variables
	private Vector3 BoxSize = new Vector3(5f,1f,5f);
	private string GameState = "game_start";
	private float SpawnHeight = 1f;
	private Vector3 CameraPosition;
	private int CurrentLayer = 0;
	private float LastSpawnedHeight = 0;
	private float NewSpawnCord = 0;
	private string NewSpawnSide = "X";
	private bool IsAlreadySpawned = false;
	private PackedScene Scene = GD.Load<PackedScene>("res://box.tscn");
	private Tween BoxTween = null;
	private float TweenSpeed = 10;
	#endregion

	public float MiddleOfRange(Range<float> value)
	{
		return (value.LowerBound+value.UpperBound)/2;
	}

	public Tween SetTween(Node3D obj)
	{
		var CubeTween = GetTree().CreateTween();
		if (NewSpawnSide == "X")
		{
			CubeTween.TweenProperty(obj, "position:x", -30, TweenSpeed);
		}
		else if (NewSpawnSide == "Z")
		{
			CubeTween.TweenProperty(obj, "position:z", -30, TweenSpeed);
		}
		CubeTween.SetEase(Tween.EaseType.InOut);
		return CubeTween;
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var Box = (Node3D)Scene.Instantiate();
		var NewPosition = Box.Position;
		NewPosition.X = 0;
		NewPosition.Z = 0;
		NewPosition.Y = SpawnHeight;
		LastSpawnedHeight = NewPosition.Y;
		CurrentLayer = (Int32)LastSpawnedHeight;
		Box.Position = NewPosition;
		this.AddChild(Box);

		Box.Position = NewPosition;
		GameState = "new_layer";
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var Camera = (Camera3D)this.GetNode("Camera3D");
		CameraPosition = Camera.Position;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		var global = GetNode<globals>("/root/Globals");
		var Box = (CsgBox3D)Scene.Instantiate();
		var AnimationContainer = (AnimationPlayer)this.GetNode("AnimationPlayer");

		if (@event.IsActionPressed("AddBox"))
		{
			if(GameState != "game_start") { 
				var NewPosition = Box.Position;
				Box.Size = BoxSize;

				if (NewSpawnSide == "X")
				{
					NewPosition.X = 7;
					NewPosition.Z = NewSpawnCord;
					BoxTween = SetTween(Box);
					NewSpawnSide = "Z";
				}
				else
				{
					NewPosition.Z = 7;
					NewPosition.X = NewSpawnCord;
					BoxTween = SetTween(Box);
					NewSpawnSide = "X";
				}

				if (GameState == "new_layer" && IsAlreadySpawned == false)
				{

					NewPosition.Y = SpawnHeight + LastSpawnedHeight;
					LastSpawnedHeight = NewPosition.Y;
					CurrentLayer = (Int32)LastSpawnedHeight;
					Box.Position = NewPosition;
					this.AddChild(Box);
					BoxTween.Play();
					TweenSpeed *= 0.9f;
					IsAlreadySpawned = true;

					AnimationContainer.Play("CameraMovement");

					var NewCameraPosition = CameraPosition;
					var CameraMovement = AnimationContainer.GetAnimation("CameraMovement");
					var StartKeyId = CameraMovement.TrackFindKey(0, 0);
					var EndKeyId = CameraMovement.TrackFindKey(0, 0.35);

					CameraMovement.TrackSetKeyValue(0, StartKeyId, NewCameraPosition);
					NewCameraPosition.Y = NewPosition.Y + 6;
					CameraMovement.TrackSetKeyValue(0, EndKeyId, NewCameraPosition);

				}
			}
		}

		if (@event.IsActionPressed("Action"))
		{
			var lbl = GetNode<Label>("/root/Scene/Score");
			var CBox = this.GetChild<CsgBox3D>(-1);//Current box
			var LBox = this.GetChild<CsgBox3D>(-2);//Last box

			Vector3 CBPosition = CBox.Position;//Current Box Position
			Vector3 LBPosition = LBox.Position;//Last Box Position

			if (NewSpawnSide == "Z")// when box comes from the X side, NewSpawnSide = "Z"
			{

				Range<float> CurrentBoxRange = new Range<float>(CBPosition.X - CBox.Size.X / 2, CBPosition.X + CBox.Size.X / 2);

				Range<float> LastBoxRange = new Range<float>(LBPosition.X - CBox.Size.X / 2, LBPosition.X + CBox.Size.X / 2);
				
				if (CurrentBoxRange.Overlaps(LastBoxRange))
				{
					BoxTween.Stop();
					Range<float> Intersect = CurrentBoxRange.Intersect(LastBoxRange);


					Vector3 NewPosition = CBPosition;
					NewPosition.X = MiddleOfRange(Intersect);
					CBox.Position = NewPosition;

					Vector3 NewSize = LBox.Size;
					NewSize.X = Intersect.UpperBound - Intersect.LowerBound;
					CBox.Size = NewSize;

					BoxSize = NewSize;
					NewSpawnCord = MiddleOfRange(Intersect);
					GameState = "new_layer";
					IsAlreadySpawned = false;
					global.Score += 1;
					lbl.Text = "Score: "+(global.Score).ToString();
				}
				else if(CurrentLayer == LastSpawnedHeight)
				{
					GetTree().ChangeSceneToFile("res://you_failed.tscn");
				}

			} else if(NewSpawnSide == "X")// when box comes from the Z side, NewSpawnSide = "X"
			{
				Range<float> CurrentBoxRange = new Range<float>(CBPosition.Z - CBox.Size.Z / 2, CBPosition.Z + CBox.Size.Z / 2);

				Range<float> LastBoxRange = new Range<float>(LBPosition.Z - CBox.Size.Z / 2, LBPosition.Z + CBox.Size.Z / 2);  

				

				if (CurrentBoxRange.Overlaps(LastBoxRange))
				{
					BoxTween.Stop();
					Range<float> Intersect = CurrentBoxRange.Intersect(LastBoxRange);

					Vector3 NewPosition = CBPosition;
					NewPosition.Z = MiddleOfRange(Intersect);
					CBox.Position = NewPosition;

					Vector3 NewSize = LBox.Size;
					NewSize.Z = Intersect.UpperBound - Intersect.LowerBound;
					CBox.Size = NewSize;

					BoxSize = NewSize;
					NewSpawnCord = MiddleOfRange(Intersect);
					GameState = "new_layer";
					IsAlreadySpawned = false;
					global.Score += 1;
					lbl.Text = "Score: " + (global.Score).ToString();

				} else if(CurrentLayer==LastSpawnedHeight)
				{
					GetTree().ChangeSceneToFile("res://you_failed.tscn");
				}
			}
		}
		
		if (@event.IsActionPressed("Exit"))
		{
			GetTree().Quit();
		}
	}

}
