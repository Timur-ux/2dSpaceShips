using WaveFunctionCollapse;
using UnityEngine;

namespace WaveFunctionCollapse {
	public class TileMap : MonoBehaviour {
		public int width = 20;
		public int height = 20;
		public int initX = 10;
		public int initY = 10;
		public GameObject cubePrefab;
		

		bool initialized_ = false;
		Tile[,] tiles_;

		void Start() {
			initialized_ = true;
			tiles_ = new Tile[height, width];

			// generate frame
			Tile bottom = ScriptableObject.CreateInstance<Tile>();
			Tile top = ScriptableObject.CreateInstance<Tile>();
			bottom.Connect(Tile.Side.down0, Tile.Side.down3);
			top.Connect(Tile.Side.up0, Tile.Side.up3);
			for(int x = 0; x < width; ++x) {
				tiles_[0, x] = top;
				tiles_[height - 1, x] = bottom;
			}

			Tile left = ScriptableObject.CreateInstance<Tile>();
			Tile right = ScriptableObject.CreateInstance<Tile>();
			left.Connect(Tile.Side.left0, Tile.Side.left3);
			right.Connect(Tile.Side.right0, Tile.Side.right3);
			for(int y = 1; y < height-1; ++y) {
				tiles_[y, 0] = left;
				tiles_[y, width - 1] = right;
			}
			
			// init tile
			Tile initTile = ScriptableObject.CreateInstance<Tile>();
			initTile.Connect(Tile.Side.down0, Tile.Side.down3);


			tiles_[initY, initX] = initTile;
			// Some generation logic
			for(int y0 = 0; y0 < height; ++y0) {
				for(int x0 = 0; x0 < width; ++x0) {
					Tile tile = tiles_[y0, x0];
					if(tile != null)
						tile.CreateInstance(x0, y0, 0, cubePrefab);
				}
			}
		}
		void Update() {}
	}
};
