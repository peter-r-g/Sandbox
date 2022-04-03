using Sandbox;
using System.Collections.Generic;

namespace SandboxGame.Entities.AI;

public class NavPath
{
	public Vector3 TargetPosition;
	public readonly List<Vector3> Points = new();

	public bool IsEmpty => Points.Count <= 1;

	public void Update( Vector3 from, Vector3 to )
	{
		var needsBuild = false;

		if ( !TargetPosition.IsNearlyEqual( to, 5 ) )
		{
			TargetPosition = to;
			needsBuild = true;
		}

		if ( needsBuild )
		{
			var fromFixed = NavMesh.GetClosestPoint( from );
			var tofixed = NavMesh.GetClosestPoint( to );

			Points.Clear();
			NavMesh.GetClosestPoint( from );
			NavMesh.BuildPath( fromFixed.Value, tofixed.Value, Points );
		}

		if ( Points.Count <= 1 )
		{
			return;
		}
		
		var deltaToNext = from - Points[1];
		var delta = Points[1] - Points[0];
		var deltaNormal = delta.Normal;

		if ( deltaToNext.WithZ( 0 ).Length < 20 )
		{
			Points.RemoveAt( 0 );
			return;
		}

		// If we're in front of this line then
		// remove it and move on to next one
		if ( deltaToNext.Normal.Dot( deltaNormal ) >= 1.0f )
		{
			Points.RemoveAt( 0 );
		}
	}

	public Vector3 GetDirection( Vector3 position )
	{
		return Points.Count == 1 ? (Points[0] - position).WithZ(0).Normal : (Points[1] - position).WithZ( 0 ).Normal;
	}
}
