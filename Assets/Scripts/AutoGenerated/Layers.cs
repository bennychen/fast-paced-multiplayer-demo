// This class is auto-generated do not modify
namespace Fangtang
{
	public static class Layers
	{
		public const int DEFAULT = 0;
		public const int TRANSPARENT_FX = 1;
		public const int IGNORE_RAYCAST = 2;
		public const int WATER = 4;
		public const int UI = 5;
		public const int CLIENT = 8;
		public const int SERVER = 9;
		public const int PROXY = 10;


		public static int onlyIncluding( int layer )
		{
			int mask = 0;
            mask |= ( 1 << layer );
			return mask;
		}

        public static int onlyIncluding(int layer1, int layer2)
        {
			int mask = 0;
            mask |= ( 1 << layer1 );
            mask |= ( 1 << layer2 );
			return mask;
        }

		public static int everythingBut( int layer )
		{
			return ~onlyIncluding( layer );
		}
	}
}