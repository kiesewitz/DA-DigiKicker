using Godot;
using System;
using System.IO.Ports;

public partial class Arduino : Node3D
{

	SerialPort serialPort;
	RichTextLabel text;
	MeshInstance3D cube;
	bool hasHeardFromArduino = false;
	float x;
	float y;
	float z;
	string[] coords;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		text = GetNode<RichTextLabel>("RichTextLabel");
		cube = GetNode<MeshInstance3D>("Cube");
		text.Text = "Hallo";
		serialPort = new SerialPort("COM6", 9600, Parity.None, 8, StopBits.One);
		serialPort.DtrEnable = true;
		serialPort.RtsEnable = true;
		serialPort.Open();
	}

	public override void _Process(double delta)
	{
		if (!serialPort.IsOpen) return; //checks if serial port is open, if it's not do nothing.
		
		string serialMessage = serialPort.ReadLine().Replace('.', ',');
		
		coords = serialMessage.Split(' ');
		
		x = (float) (Convert.ToDouble(coords[2])*delta);
		y = (float) (Convert.ToDouble(coords[4])*delta);
		z = (float) (Convert.ToDouble(coords[6])*delta);
		
		//Vector3 newRotation = new Vector3(
		//	Mathf.DegToRad(x),
		//	Mathf.DegToRad(y),
		//	Mathf.DegToRad(z)
		//);
		
		//cube.Rotation = newRotation;
		cube.RotateX(x);
		cube.RotateY(y);
		cube.RotateZ(z);

		//GD.Print(serialMessage);

		text.Text = serialMessage;
	}
}
