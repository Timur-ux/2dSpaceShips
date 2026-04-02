using UnityEngine;
using System.Collections.Generic;
using System;

namespace WaveFunctionCollapse {
	public class TileMap : MonoBehaviour {
		public int width = 20;
		public int height = 20;
		public int initX = 10;
		public int initY = 10;
		public GameObject cubePrefab;

		private struct BorderData {
			public int i, j;
			public List<Tile> avaliableConfigurations;
			public BorderData(int i, int j) {
				this.i = i;
				this.j = j;
				avaliableConfigurations = new List<Tile>();
			}
		};

		Tile[,] tiles_;
		List<Tile> tileConfigurations_ = new List<Tile>();
		List<BorderData> borderTiles_ = new List<BorderData>();
		List<(Tile.Side sFrom, Tile.Side sTo)> tileConnections_ = new List<(Tile.Side sFrom, Tile.Side sTo)> {
			(Tile.Side.down0, Tile.Side.down3),
			(Tile.Side.left0, Tile.Side.left3),
			(Tile.Side.right0, Tile.Side.right3),
			(Tile.Side.up0, Tile.Side.up3),
			(Tile.Side.up0, Tile.Side.right3),
			(Tile.Side.right0, Tile.Side.down3),
			(Tile.Side.down0, Tile.Side.left3),
			(Tile.Side.left0, Tile.Side.up3),
		};

		public void RegenerateMap() {
			throw new System.InvalidCastException("Not realized yet");
		}

		private void PlaceTile(Tile tile, int i, int j) {
			tiles_[i, j] = tile;
			PlaceBorderIfNotExist(i + 1, j);
			PlaceBorderIfNotExist(i - 1, j);
			PlaceBorderIfNotExist(i, j + 1);
			PlaceBorderIfNotExist(i, j - 1);
		}

		private bool IsConfigValid(Tile tileConfig, (int i, int j) pos) {
			Func<Tile, (int i, int j), Tile.ConnectDirection, bool> adjancentCheck =
				(Tile tile, (int i, int j) pos, Tile.ConnectDirection dir) => {
					if (pos.i < 0 || pos.i >= height || pos.j < 0 || pos.j >= height || tiles_[pos.i, pos.j] == null)
						return true;
					Tile otherTile = tiles_[pos.i, pos.j];
					return tile.CanConnectTo(otherTile, dir);
				};

			return adjancentCheck(tileConfig, (pos.i + 1, pos.j), Tile.ConnectDirection.FromTopToBottom)
					&& adjancentCheck(tileConfig, (pos.i - 1, pos.j), Tile.ConnectDirection.FromBottomToTop)
					&& adjancentCheck(tileConfig, (pos.i, pos.j + 1), Tile.ConnectDirection.FromLeftToRight)
					&& adjancentCheck(tileConfig, (pos.i, pos.j - 1), Tile.ConnectDirection.FromRightToLeft);
		}

		private void PlaceBorderIfNotExist(int i, int j) {
			if (i < 0 || i >= height || j < 0 || j >= height || tiles_[i, j] != null)
				return;
			BorderData border = new BorderData(i, j);
			foreach (var config in tileConfigurations_) {
				if (IsConfigValid(config, (i, j)))
					border.avaliableConfigurations.Add(config);
			}
			borderTiles_.Add(border);
		}


		private void GenerateMap() {
			throw new System.InvalidCastException("Not realized yet");
		}

		private float Entropy(BorderData data) {
			throw new System.InvalidCastException("Not realized yet");
		}

		public void GenerationStep() {
			throw new System.InvalidCastException("Not realized yet");
		}

		private void InitTileConfigurations() {
			foreach (var connection in tileConnections_) {
				Tile tile = ScriptableObject.CreateInstance<Tile>();
				tile.Init(connection.sFrom, connection.sTo);
				tileConfigurations_.Add(tile);
			}
		}

		private void InitTileMap() {
			tiles_ = new Tile[height, width];
		}

		private void InitMapFrame() {
			Tile bottom = ScriptableObject.CreateInstance<Tile>();
			Tile top = ScriptableObject.CreateInstance<Tile>();
			bottom.Init(Tile.Side.down0, Tile.Side.down3);
			top.Init(Tile.Side.up0, Tile.Side.up3);
			for (int x = 0; x < width; ++x) {
				PlaceTile(top, 0, x);
				PlaceTile(bottom, height - 1, x);
			}

			Tile left = ScriptableObject.CreateInstance<Tile>();
			Tile right = ScriptableObject.CreateInstance<Tile>();
			left.Init(Tile.Side.left0, Tile.Side.left3);
			right.Init(Tile.Side.right0, Tile.Side.right3);
			for (int y = 1; y < height - 1; ++y) {
				PlaceTile(left, y, 0);
				PlaceTile(right, y, width - 1);
			}
		}
		private void InitStartTile() {
			Tile initTile = ScriptableObject.CreateInstance<Tile>();
			initTile.Init(Tile.Side.down0, Tile.Side.down3);
			PlaceTile(initTile, initY, initX);
		}

		private void InstantiateCreatedTiles() {
			for (int y0 = 0; y0 < height; ++y0) {
				for (int x0 = 0; x0 < width; ++x0) {
					Tile tile = tiles_[y0, x0];
					if (tile != null)
						tile.CreateInstance(x0, y0, 0, cubePrefab);
				}
			}
		}

		void Start() {
			InitTileConfigurations();
			InitTileMap();
			InitMapFrame();
			InitStartTile();
			GenerateMap();
			InstantiateCreatedTiles();
			Debug.Log(borderTiles_.Count);
		}

		void Update() { }
	}
};
