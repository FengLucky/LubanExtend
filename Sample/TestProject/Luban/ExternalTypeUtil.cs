using System.Numerics;
namespace Langrisser
{
	public static class ExternalTypeUtil
	{
		
		public static Vector3 NewVector3(CfgVector3 vector)
		{
			return new Vector3(vector.X, vector.Y, vector.Z);
		}
	}
}

