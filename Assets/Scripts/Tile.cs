using System;
namespace WaveFunctionCollapse {
	using BitMask = int;
	// Tile consist of 4x4 points each can be used by wall or platform or be clear
	// Used for map generation by wave function collapse algo
	public class Tile {
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
		public enum SideMask {
			up = Side.up0 | Side.up1 | Side.up2 | Side.up2,
			right = Side.right0 | Side.right1 | Side.right2 | Side.right2,
			down = Side.down0 | Side.down1 | Side.down2 | Side.down2,
			left = Side.left0 | Side.left1 | Side.left2 | Side.left2,
		}

		// Offset needed to shift SideMask at the begin of the mask
		private enum SideOffset {
			up = 0,
			right = 4,
			down = 8,
			left = 12
		};

		public struct Coord {
			public int x; // from left to right
			public int y; // from top to bottom
			public BitMask AsOffset() {
				return 1 << (y * 4 + x);
			}
		};

		private BitMask usedSides_ = 0;
		private BitMask usedCells_ = 0;

		private Coord SideToCoord(Side side) {
			switch (side) {
				case Side.up0:
					return Coord(0, 0);
				case Side.up1:
					return Coord(0, 1);
				case Side.up2:
					return Coord(0, 2);
				case Side.up3:
					return Coord(0, 3);
				case Side.right0:
					return Coord(0, 3);
				case Side.right1:
					return Coord(1, 3);
				case Side.right2:
					return Coord(2, 3);
				case Side.right3:
					return Coord(3, 3);
				case Side.down0:
					return Coord(3, 3);
				case Side.down1:
					return Coord(3, 2);
				case Side.down2:
					return Coord(3, 1);
				case Side.down3:
					return Coord(3, 0);
				case Side.left0:
					return Coord(3, 0);
				case Side.left1:
					return Coord(2, 0);
				case Side.left2:
					return Coord(1, 0);
				case Side.left3:
					return Coord(0, 0);
			}
			throw ArgumentOutOfRangeException;
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
				if (usedCells_ & c.AsOffset())
					usedSides_ |= (Side.up0 << i);
			for (int i = 0; i < 4; ++i, ++c.y)
				if (usedCells_ & c.AsOffset())
					usedSides_ |= (Side.left0 << i);
			for (int i = 0; i < 4; ++i, --c.x)
				if (usedCells_ & c.AsOffset())
					usedSides_ |= (Side.down0 << i);
			for (int i = 0; i < 4; ++i, --c.y)
				if (usedCells_ & c.AsOffset())
					usedSides_ |= (Side.left0 << i);
		}

		public BitMask UsedSide(SideMask side) {
			return usedSides_ & side;
		}

		private BitMask SideWithOffset(BitMask mask, SideMask side) {
			switch (side) {
				case SideMask.up:
					return mask >> SideOffset.up;
				case SideMask.right:
					return mask >> SideOffset.right;
				case SideMask.down:
					return mask >> SideOffset.down;
				case SideMask.left:
					return mask >> SideOffset.left;
			}
		}

		public enum ConnectDirection {
			FromBottomToTop,
			FromTopToBottom,

			FromLeftToRight,
			FromRightToLeft,
		}
		// Connect direction is relative from this tile to other tile
		// Connection allowed if connection side identical
		public bool CanConnectTo(ref Tile tile, ConnectDirection relativeDirection) {
			SideMask maskThis, maskOther;
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
			BitMask usedSideThis = this.UsedSide(maskThis);
			BitMask usedSideOther = this.UsedSide(maskOther);
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
	};
};
