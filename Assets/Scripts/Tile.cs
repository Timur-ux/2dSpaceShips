using System;
using UnityEngine;

namespace WaveFunctionCollapse {
	using BitMask = System.Int32;
	// Tile consist of 4x4 points each can be used by wall or platform or be clear
	// Used for map generation by wave function collapse algo
	public class Tile : ScriptableObject {
		public enum Side {
			up0 = 1, // up side left to right
			up1 = 1 << 1,
			up2 = 1 << 2,
			up3 = 1 << 3,
			right0 = 1 << 4, // right side top to bottom
			right1 = 1 << 5,
			right2 = 1 << 6,
			right3 = 1 << 7,
			down0 = 1 << 8, // down side right to left
			down1 = 1 << 9,
			down2 = 1 << 10,
			down3 = 1 << 11,
			left0 = 1 << 12, // left side bottom to top
			left1 = 1 << 13,
			left2 = 1 << 14,
			left3 = 1 << 15,
		};
		public enum SideMask : System.Int32 {
			up = Side.up0 | Side.up1 | Side.up2 | Side.up2,
			right = Side.right0 | Side.right1 | Side.right2 | Side.right2,
			down = Side.down0 | Side.down1 | Side.down2 | Side.down2,
			left = Side.left0 | Side.left1 | Side.left2 | Side.left2,
		}

		// Offset needed to shift SideMask at the begin of the mask
		private enum SideOffset : System.Int32 {
			up = 0,
			right = 4,
			down = 8,
			left = 12
		};

		public struct Coord {
			public int x; // from left to right
			public int y; // from top to bottom
			public Coord(int y_, int x_) {
				x = x_;
				y = y_;
			}

			public BitMask AsOffset() {
				return 1 << (y * 4 + x);
			}

			static public bool operator ==(Coord lhs, Coord rhs) {
				return lhs.x == rhs.x && lhs.y == rhs.y;
			}

			static public bool operator !=(Coord lhs, Coord rhs) {
				return lhs.x != rhs.x || lhs.y != rhs.y;
			}
		};

		private Coord SideToCoord(Side side) {
			switch (side) {
				case Side.up0:
					return new Coord(0, 0);
				case Side.up1:
					return new Coord(0, 1);
				case Side.up2:
					return new Coord(0, 2);
				case Side.up3:
					return new Coord(0, 3);
				case Side.right0:
					return new Coord(0, 3);
				case Side.right1:
					return new Coord(1, 3);
				case Side.right2:
					return new Coord(2, 3);
				case Side.right3:
					return new Coord(3, 3);
				case Side.down0:
					return new Coord(3, 3);
				case Side.down1:
					return new Coord(3, 2);
				case Side.down2:
					return new Coord(3, 1);
				case Side.down3:
					return new Coord(3, 0);
				case Side.left0:
					return new Coord(3, 0);
				case Side.left1:
					return new Coord(2, 0);
				case Side.left2:
					return new Coord(1, 0);
				case Side.left3:
					return new Coord(0, 0);
			}
			throw new ArgumentOutOfRangeException("side");
		}

		public void Connect(Side sFrom, Side sTo) {
			Coord cFrom = SideToCoord(sFrom), cTo = SideToCoord(sTo);

			// Connect sides by cell's chain
			// First connect by x coord then by y
			usedCells_ |= cTo.AsOffset();
			while (cFrom != cTo) {
				usedCells_ |= cFrom.AsOffset();
				if (cFrom.x < cTo.x)
					++cFrom.x;
				else if (cFrom.x > cTo.x)
					--cFrom.x;
				else if (cFrom.y > cTo.y)
					--cFrom.y;
				else if (cFrom.y < cTo.y)
					++cFrom.y;
			}
			// Update used sides
			Coord c = new Coord(0, 0);
			for (int i = 0; i < 4; ++i, ++c.x)
				if ((usedCells_ & c.AsOffset()) != 0)
					usedSides_ |= ((System.Int32)Side.up0 << i);
			for (int i = 0; i < 4; ++i, ++c.y)
				if ((usedCells_ & c.AsOffset()) != 0)
					usedSides_ |= ((System.Int32)Side.left0 << i);
			for (int i = 0; i < 4; ++i, --c.x)
				if ((usedCells_ & c.AsOffset()) != 0)
					usedSides_ |= ((System.Int32)Side.down0 << i);
			for (int i = 0; i < 4; ++i, --c.y)
				if ((usedCells_ & c.AsOffset()) != 0)
					usedSides_ |= ((System.Int32)Side.left0 << i);
		}

		public BitMask UsedSide(SideMask side) {
			return usedSides_ & ((System.Int32)side);
		}

		private BitMask SideWithOffset(BitMask mask, SideMask side) {
			System.Int32 mask_ = (System.Int32)mask;
			switch (side) {
				case SideMask.up:
					return mask_ >> ((System.Int32)SideOffset.up);
				case SideMask.right:
					return mask_ >> ((System.Int32)SideOffset.right);
				case SideMask.down:
					return mask_ >> ((System.Int32)SideOffset.down);
				case SideMask.left:
					return mask_ >> ((System.Int32)SideOffset.left);
			}
			throw new ArgumentOutOfRangeException("side");
		}

		public enum ConnectDirection {
			FromBottomToTop,
			FromTopToBottom,

			FromLeftToRight,
			FromRightToLeft,
		}
		// Connect direction is relative from this tile to other tile
		// Connection allowed if connection side identical
		public bool CanConnectTo(Tile tile, ConnectDirection relativeDirection) {
			SideMask maskThis = SideMask.up, maskOther = SideMask.up;
			switch (relativeDirection) {
				case ConnectDirection.FromBottomToTop:
					maskThis = SideMask.up;
					maskOther = SideMask.down;
					break;
				case ConnectDirection.FromTopToBottom:
					maskThis = SideMask.down;
					maskOther = SideMask.up;
					break;
				case ConnectDirection.FromLeftToRight:
					maskThis = SideMask.right;
					maskOther = SideMask.left;
					break;
				case ConnectDirection.FromRightToLeft:
					maskThis = SideMask.left;
					maskOther = SideMask.right;
					break;
			}
			BitMask usedSideThis = UsedSide(maskThis);
			BitMask usedSideOther = tile.UsedSide(maskOther);
			if (usedSideThis == 0 || usedSideOther == 0)
				return true; // one of sides is empty -- allowed

			// Check adjanced side points
			usedSideThis = SideWithOffset(usedSideThis, maskThis);
			usedSideOther = SideWithOffset(usedSideOther, maskThis);
			// side points from other side placed in opposite order (e.g. see left and right Side)
			// so other side point offset are reversed 
			for (int thisOffset = 0, otherOffset = 3; thisOffset < 4; ++thisOffset, --otherOffset)
				if (((usedSideThis >> thisOffset) & 1) != ((usedSideOther >> otherOffset) & 1))
					return false;
			return true;
		}

		public Tile(Tile other) {
			usedSides_ = other.usedSides_;
			usedCells_ = other.usedCells_;
		}
		public Tile() {}
		public Tile(Side sFrom, Side sTo) {
			Connect(sFrom, sTo);
		}
		public void Init(Side sFrom, Side sTo) {
			usedSides_ = 0;
			usedCells_ = 0;
			Connect(sFrom, sTo);
		}

		public void CreateInstance(int x0, int y0, int z0, GameObject cube) {
			Coord coord = new Coord(0, 0);
			for(int i = 0; i < 4; ++i) {
				for(int j = 0; j < 4; ++j) {
					bool isUsed = (((uint)usedCells_) & ((uint)coord.AsOffset())) != 0;
					++coord.x;
					if(isUsed)
						Instantiate(cube, new Vector3(x0 + coord.x, y0 + coord.y, z0), Quaternion.identity);
				}
				++coord.y;
				coord.x = 0;
			}
		}

		public void print() {
			string message = "\n";
			Coord coord = new Coord(0, 0);
			for(int i = 0; i < 4; ++i) {
				for(int j = 0; j < 4; ++j) {
					bool isUsed = (((uint)usedCells_) & ((uint)coord.AsOffset())) != 0;
					++coord.x;
					if(isUsed)
						message += "x";
					else
						message += "o";
				}
				++coord.y;
				coord.x = 0;
				message += "\n";
			}
			Debug.Log(message);
		}

		private BitMask usedSides_ = 0;
		private BitMask usedCells_ = 0;
	};
};
