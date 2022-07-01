using Godot;
using System;

public class perlinnoise : Sprite
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	Vector2[,] grid;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		initializeNodes(255);

		Image i = new Image();
		i.Create(1920, 1080, false, Image.Format.Rgb8);
		i.Lock();
//Random r = new Random();
		ImageTexture t = new ImageTexture();

		float scale = 0.005f;

		for (int y = 0; y < i.GetHeight(); y++)
		{
			for (int x = 0; x < i.GetWidth(); x++)
			{
				float f = noise2D(x * scale, y * scale);
				i.SetPixel(x, y, new Color(f, f, f));
			}
		}

		i.Unlock();
		t.CreateFromImage(i);

		this.Texture = t;

	}

	private float dotProduct(float x, float y, int vx, int vy){
		// Auswählen des Gradienten
		Vector2 g_vect = grid[vx,vy];
		// Distanz berechnen:
		Vector2 d_vect = new Vector2(x - vx, y - vy);

		return d_vect.x * g_vect.x + d_vect.y * g_vect.y;
	
	}

	private void initializeNodes(int nodeSize)
	{
		grid = new Vector2[nodeSize, nodeSize];
		Random r = new Random();
		double theta = 0;
		for (int x = 0; x < nodeSize; x++)
		{
			for (int y = 0; y < nodeSize; y++)
			{
				theta = (r.Next(0,100)/100f) * 2 * Math.PI;
				//grid[x, y] = new Vector2(r.Next(-1, 1) / 1f, r.Next(-1, 1) / 1f);
				grid[x, y] = new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta));
			}
		}
	}
	private float noise2D(float x, float y)
	{
		// Indices für die Ecken bestimmen
		int x0 = (int)Math.Floor(x);
		int x1 = (int)x0 + 1;
		int y0 = (int)Math.Floor(y);
		int y1 = (int)y0 + 1;

		// Kreuzprodukte berechnen
		float tl = this.dotProduct(x, y, x0,   y0);
		float tr = this.dotProduct(x, y, x0+1, y0);
		float bl = this.dotProduct(x, y, x0,   y0+1);
		float br = this.dotProduct(x, y, x0+1, y0+1);

		float xt = this.linearInterpolate(x-x0, tl, tr);
		float xb = this.linearInterpolate(x-x0, bl, br);
		float v = this.linearInterpolate(y-y0, xt, xb);

		return v;
	}

	private float linearInterpolate(float x, float a, float b)
	{
		return a + fade(x) * (b - a);
	}

	private float fade(float t){
		// Ergebnis ein bisschen abweichen
	return ((6*t - 15)*t + 10)*t*t*t;
}

}
