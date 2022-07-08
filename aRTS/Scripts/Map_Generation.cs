using Godot;
using System;

public class Map_Generation : Spatial
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		generateTerrain();
	}
	
	private void generateTerrain(){
//TODO: Müssen in Konstanten ausgelagert werden
int terrainSize = 512;
int subdivideDepth = 500;
int subdivideWidth = 500;
int HEIGHT = 60;


// Simplex noise erstellen
OpenSimplexNoise noise = new OpenSimplexNoise();
noise.Period = 80;
noise.Octaves = 6;


		// Erstellen eines neuen Meshes
		PlaneMesh mesh = new PlaneMesh();
		mesh.Size = new Vector2(terrainSize,terrainSize);
		mesh.SubdivideDepth = (subdivideDepth);
		mesh.SubdivideWidth = (subdivideWidth);

		// Mesh nun bearbeitbar machen
		SurfaceTool sTool = new SurfaceTool();
		MeshDataTool dTool = new MeshDataTool();
		sTool.CreateFrom(mesh,0);
		ArrayMesh aMesh =  sTool.Commit();
		Error error = dTool.CreateFromSurface(aMesh, 0);

		Vector3 vertex;

		// Vertex für Vertex durchgehen und mit OpenSimples eine Map kreieren.
		for (int i = 0; i < dTool.GetVertexCount(); i++)
		{

			// Vertex holen...
			vertex = dTool.GetVertex(i);
			// Y anpassen...
			vertex.y = noise.GetNoise3d(vertex.x, vertex.y, vertex.z) * HEIGHT;
			// Vertex wieder zurückgeben
			dTool.SetVertex(i, vertex);
		}

		// Aufräumen
		for (int i = 0; i < aMesh.GetSurfaceCount(); i++)
		{
			aMesh.SurfaceRemove(i);
		}

		dTool.CommitToSurface(aMesh);
		sTool.Begin(Mesh.PrimitiveType.Triangles);
		sTool.CreateFrom(aMesh, 0);
		sTool.GenerateNormals();

		// Mesh Instance kreieren
		MeshInstance mInstance = new MeshInstance();
		mInstance.Mesh = sTool.Commit();

		// Terrain den Shader mitgeben, der benutzt werden soll
		mInstance.SetSurfaceMaterial(0, ResourceLoader.Load("res://Shader/terrain.material") as Material);

		// Dem Mesh einen Collider zuweisen
		mInstance.CreateTrimeshCollision();

		// Mesh der Szene hinzufügen
		AddChild(mInstance);
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
