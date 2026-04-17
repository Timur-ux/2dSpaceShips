using UnityEngine;
using System.Collections.Generic;
using System;

namespace WaveFunctionCollapse {
	public class TileMap : MonoBehaviour {
		public void RegenerateMap() {
			throw new System.InvalidCastException("Not realized yet");
		}

		private void InitTileMap() {
			tiles_ = new Tile[height, width];
			borderTiles_ = new BorderData[height, width];
		}

		private void PlaceTile(Tile tile, int i, int j) {
			tiles_[i, j] = tile;
			tile.CreateInstance(j, i, cubePrefab);
			if (borderTiles_[i, j] != null) {
				borderTiles_[i, j] = null;
				--borders_;
			}
			PlaceOrUpdateBorder(i + 1, j);
			PlaceOrUpdateBorder(i - 1, j);
			PlaceOrUpdateBorder(i, j + 1);
			PlaceOrUpdateBorder(i, j - 1);
		}

		private bool IsConfigValid(Tile tileConfig, (int i, int j) pos) {
			Func<Tile, (int i, int j), Tile.ConnectDirection, bool> adjancentCheck =
				(Tile tile, (int i, int j) pos, Tile.ConnectDirection dir) => {
					if (pos.i < 0 || pos.i >= height || pos.j < 0 || pos.j >= height || tiles_[pos.i, pos.j] == null)
						return true;
					Tile otherTile = tiles_[pos.i, pos.j];
					return tile.CanConnectTo(otherTile, dir);
				};

			return adjancentCheck(tileConfig, (pos.i + 1, pos.j), Tile.ConnectDirection.FromBottomToTop)
					&& adjancentCheck(tileConfig, (pos.i - 1, pos.j), Tile.ConnectDirection.FromTopToBottom)
					&& adjancentCheck(tileConfig, (pos.i, pos.j + 1), Tile.ConnectDirection.FromLeftToRight)
					&& adjancentCheck(tileConfig, (pos.i, pos.j - 1), Tile.ConnectDirection.FromRightToLeft);
		}

		private void PlaceOrUpdateBorder(int i, int j) {
			if (i < 0 || i >= height || j < 0 || j >= width || tiles_[i, j] != null)
				return;
			BorderData border;
			if (borderTiles_[i, j] == null) {
				border = new BorderData();
				++borders_;
			} else
				border = borderTiles_[i, j];
			if (borderTiles_[i, j] != null)
				border.avaliableConfigurations.Clear();
			foreach (var config in tileConfigurations_) {
				if (IsConfigValid(config, (i, j)))
					border.avaliableConfigurations.Add(config);
			}
			borderTiles_[i, j] = border;
		}

		private void PlaceRandomTile(BorderData data, int i, int j) {
			int choice = random.Next(0, data.avaliableConfigurations.Count - 1);
			Tile tile = data.avaliableConfigurations[choice];
			PlaceTile(tile, i, j);
			borderTiles_[i, j] = null;
		}

		public void GenerationStep() {
			double minEntropy = double.MaxValue;
			foreach (var border in borderTiles_) {
				if (border == null || border.avaliableConfigurations.Count == 0)
					continue;
				minEntropy = minEntropy = Math.Min(minEntropy, border.Entropy(random));
			}

			for (int i = 0; i < height; ++i)
				for (int j = 0; j < width; ++j)
					if (tiles_[i, j] != null || borderTiles_[i, j] == null)
						continue;
					else if (borderTiles_[i, j].Entropy(random) <= minEntropy)
						PlaceRandomTile(borderTiles_[i, j], i, j);
		}

		private bool PlaceTurretIfCan(Tile tile, int i, int j) {
			for (int x = 0; x < 4; ++x)
				for (int y = 0; y < 3; ++y)
					if (tile.IsCellUsed(x, y) && !tile.IsCellUsed(x, y + 1)) {
						Instantiate(turretPrefab_, new Vector3(j * 4 + x, i * 4 + y, 0), Quaternion.identity);
						return true;
					}

			return false;
		}

		private void PlaceTurrets() {
			int placedTurrets = 0;
			int steps = 0;
			while (placedTurrets < turretsToCreate && steps < maxGenSteps) {
				++steps;
				int i = random.Next(0, height - 1);
				int j = random.Next(0, width - 1);
				Tile tile = tiles_[i, j];
				if (tile == null)
					continue;
				if(PlaceTurretIfCan(tile, i, j))
					++placedTurrets;
			}
			if (placedTurrets < turretsToCreate)
				Debug.Log("Not all turrets placed via steps limit reached");
			else
				Debug.Log("All turrets placed");
		}

		private void GenerateMap() {
			int steps = 0;
			while (borders_ > 0 && steps < maxGenSteps) {
				GenerationStep();
				++steps;
			}
			Debug.Log($"Map generation done in {steps} steps. Last borders: {borders_}");
			PlaceTurrets();
		}

		private void InitTileConfigurations() {
			foreach (var connection in tileConnections_) {
				Tile tile = ScriptableObject.CreateInstance<Tile>();
				tile.Init(connection.sFrom, connection.sTo);
				tileConfigurations_.Add(tile);
			}
		}


		private void InitMapFrame() {
			Tile bottom = ScriptableObject.CreateInstance<Tile>();
			Tile top = ScriptableObject.CreateInstance<Tile>();
			bottom.Init(Tile.Side.down0, Tile.Side.down3);
			top.Init(Tile.Side.up0, Tile.Side.up3);
			for (int x = 1; x < width - 1; ++x) {
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

			Tile anchor = ScriptableObject.CreateInstance<Tile>();
			anchor.Init(Tile.Side.left0, Tile.Side.up3);
			PlaceTile(anchor, height - 1, width - 1);

			anchor = ScriptableObject.CreateInstance<Tile>();
			anchor.Init(Tile.Side.up0, Tile.Side.right3);
			PlaceTile(anchor, 0, width - 1);

			anchor = ScriptableObject.CreateInstance<Tile>();
			anchor.Init(Tile.Side.right0, Tile.Side.down3);
			PlaceTile(anchor, 0, 0);

			anchor = ScriptableObject.CreateInstance<Tile>();
			anchor.Init(Tile.Side.down0, Tile.Side.left3);
			PlaceTile(anchor, height - 1, 0);
		}

		private void InitStartTile() {
			Tile initTile = ScriptableObject.CreateInstance<Tile>();
			initTile.Init(Tile.Side.down0, Tile.Side.down3);
			PlaceTile(initTile, initY, initX);
			PlaceTile(initTile, initY, initX-1);
			PlaceTile(initTile, initY, initX+1);
			PlaceTile(initTile, initY+1, initX);
			PlaceTile(initTile, initY+1, initX-1);
			PlaceTile(initTile, initY+1, initX+1);
		}

		private void InstantiateCreatedTiles() {
			for (int y0 = 0; y0 < height; ++y0) {
				for (int x0 = 0; x0 < width; ++x0) {
					Tile tile = tiles_[y0, x0];
					if (tile != null)
						tile.CreateInstance(x0, y0, cubePrefab);
				}
			}
		}

		void Start() {
			InitTileMap();
			InitTileConfigurations();
			InitMapFrame();
			InitStartTile();
			GenerateMap();
			InstantiateCreatedTiles();
		}

		void Update() { }

		public int width = 20;
		public int height = 20;
		public int initX = 10;
		public int initY = 10;
		public GameObject cubePrefab;
		public GameObject turretPrefab_;
		public int turretsToCreate = 0;
		public int maxGenSteps = 100;

		private class BorderData {
			private double entropy_ = -1;
			public List<Tile> avaliableConfigurations;

			public BorderData() {
				avaliableConfigurations = new List<Tile>();
				entropy_ = -1;
			}

			public double Entropy(System.Random random) {
				if (entropy_ != -1)
					return entropy_;

				int n = avaliableConfigurations.Count;
				double noise = (double)random.Next(0, 100) / 1000;
				if (n == 0)
					return 1e9;
				double p = 1 / (double)n;
				entropy_ = -n * p * Math.Log(p) + noise;
				return entropy_;
			}
		};

		System.Random random = new System.Random();
		Tile[,] tiles_;
		BorderData[,] borderTiles_;
		int borders_ = 0;

		List<Tile> tileConfigurations_ = new List<Tile>();
		List<(Tile.Side sFrom, Tile.Side sTo)> tileConnections_ = new List<(Tile.Side sFrom, Tile.Side sTo)> {
			(Tile.Side.down0, Tile.Side.down3),
			(Tile.Side.down0, Tile.Side.down3),
			(Tile.Side.down0, Tile.Side.down3),
			(Tile.Side.down0, Tile.Side.down3),
			(Tile.Side.down0, Tile.Side.down3),
			(Tile.Side.down0, Tile.Side.down3),
			(Tile.Side.down0, Tile.Side.down3),
			(Tile.Side.down0, Tile.Side.down3),
			(Tile.Side.down0, Tile.Side.down3),
			(Tile.Side.down0, Tile.Side.down3),
			(Tile.Side.down0, Tile.Side.down3),
			(Tile.Side.up0, Tile.Side.up3),
			(Tile.Side.up0, Tile.Side.up3),
			(Tile.Side.up0, Tile.Side.up3),
			(Tile.Side.up0, Tile.Side.up3),
			(Tile.Side.up0, Tile.Side.up3),
			(Tile.Side.up0, Tile.Side.up3),
			(Tile.Side.up0, Tile.Side.up3),
			(Tile.Side.up0, Tile.Side.up3),
			(Tile.Side.up0, Tile.Side.up3),
			(Tile.Side.up0, Tile.Side.up3),
			(Tile.Side.left0, Tile.Side.left3),
			(Tile.Side.right0, Tile.Side.right3),
			(Tile.Side.up2, Tile.Side.right1),
			(Tile.Side.right2, Tile.Side.down1),
			(Tile.Side.down2, Tile.Side.left1),
			(Tile.Side.left2, Tile.Side.up1),
		};
	}
};
