using Sandbox;

namespace SandboxGame.Tools;

[Library( Constants.Tool.Rope, Group = "construction" )]
public class RopeTool : BaseTool
{
	private Vector3 globalOrigin1;
	private Vector3 localOrigin1;
	private PhysicsBody targetBody;
	private int targetBone;

	public override void Simulate()
	{
		if ( !Host.IsServer )
		{
			return;
		}

		using ( Prediction.Off() )
		{
			if ( !Input.Pressed( InputButton.Attack1 ) )
			{
				return;
			}

			var startPos = Owner.EyePosition;
			var dir = Owner.EyeRotation.Forward;

			var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
				.Ignore( Owner )
				.Run();

			if ( !tr.Hit )
			{
				return;
			}

			if ( !tr.Body.IsValid() )
			{
				return;
			}

			if ( !tr.Entity.IsValid() )
			{
				return;
			}

			if ( tr.Entity is not ModelEntity )
			{
				return;
			}

			if ( !targetBody.IsValid() )
			{
				targetBody = tr.Body;
				targetBone = tr.Bone;
				globalOrigin1 = tr.EndPosition;
				localOrigin1 = tr.Body.Transform.PointToLocal( globalOrigin1 );

				CreateHitEffects( tr.EndPosition );

				return;
			}

			if ( targetBody == tr.Body )
			{
				return;
			}

			var rope = Particles.Create( "particles/rope.vpcf" );

			if ( targetBody.GetEntity().IsWorld )
			{
				rope.SetPosition( 0, localOrigin1 );
			}
			else
			{
				rope.SetEntityBone( 0, targetBody.GetEntity(), targetBone,
					new Transform( localOrigin1 * (1.0f / targetBody.GetEntity().Scale) ) );
			}

			var localOrigin2 = tr.Body.Transform.PointToLocal( tr.EndPosition );

			if ( tr.Entity.IsWorld )
			{
				rope.SetPosition( 1, localOrigin2 );
			}
			else
			{
				rope.SetEntityBone( 1, tr.Body.GetEntity(), tr.Bone,
					new Transform( localOrigin2 * (1.0f / tr.Entity.Scale) ) );
			}

			var spring = PhysicsJoint.CreateLength( targetBody.LocalPoint( localOrigin1 ),
				tr.Body.LocalPoint( localOrigin2 ), tr.EndPosition.Distance( globalOrigin1 ) );
			spring.SpringLinear = new PhysicsSpring( 5.0f, 0.7f );
			spring.EnableAngularConstraint = false;
			spring.Collisions = true;
			spring.MinLength = 0;


			/*var spring = PhysicsJoint.Spring
				.From( targetBody, localOrigin1 )
				.To( tr.Body, localOrigin2 )
				.WithFrequency( 5.0f )
				.WithDampingRatio( 0.7f )
				.WithReferenceMass( targetBody.Mass )
				.WithMinRestLength( 0 )
				.WithMaxRestLength(  )
				.WithCollisionsEnabled()
				.Create(); */

			spring.OnBreak += () =>
			{
				rope?.Destroy( true );
				spring.Remove();
			};

			CreateHitEffects( tr.EndPosition );

			Reset();
		}
	}

	private void Reset()
	{
		targetBody = null;
		targetBone = -1;
		localOrigin1 = default;
	}

	public override void Activate()
	{
		base.Activate();

		Reset();
	}

	public override void Deactivate()
	{
		base.Deactivate();

		Reset();
	}
}
