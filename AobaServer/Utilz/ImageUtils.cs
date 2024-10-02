using System.Collections.Generic;
using System.IO;
using System;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace AobaServer.Utilz;

public static class ImageUtils
{
	/// <summary>
	/// Read the meta data of an image and apply the rotation to the underlying image
	/// </summary>
	/// <param name="stream">Image stream</param>
	/// <param name="encoder">Image encoder to use</param>
	/// <returns>Image stream with rotation applied</returns>
	public static Stream FixImageRotation(Stream stream, IImageEncoder encoder)
	{
		using var img = Image.Load(stream);
		img.Mutate(i => i.AutoOrient());
		var result = new MemoryStream();
		img.Save(result, encoder);
		result.Position = 0;
		return result;
	}

	/// <summary>
	/// Generate zoom tiles for a specified image
	/// </summary>
	/// <param name="file">File</param>
	/// <param name="minLevel">Min zoom level</param>
	/// <param name="maxLevel">Max zoom level</param>
	/// <returns></returns>
	public static (GigaImage img, List<(string name, Stream file)> tiles) GenerateZoom(string file, int minLevel = 0, int maxLevel = 0)
	{
		using var fs = new FileStream(file, FileMode.Open);
		return GenerateZoom(fs, minLevel, maxLevel);
	}

	/// <summary>
	/// Generate zoom tiles for a specified image
	/// </summary>
	/// <param name="file">File</param>
	/// <param name="minLevel">Min zoom level</param>
	/// <param name="maxLevel">Max zoom level</param>
	/// <returns></returns>
	public static (GigaImage img, List<(string name, Stream file)> tiles) GenerateZoom(Stream file, int minLevel = 0, int maxLevel = 0)
	{
		file.Position = 0;
		var imgData = Image.Load(file);
		var height = imgData.Height;
		var width = imgData.Width;
		//Calculate min/max levels if not provided
		if (maxLevel == 0)
			maxLevel = CalculateMaxLevel(width, height);
		if (minLevel == 0)
			minLevel = GetClosestLevel(maxLevel, width, height);

		//Generate tiles for each zoom level
		var outputTiles = new List<(string name, Stream file)>();
		for (int i = minLevel; i <= maxLevel; i++)
		{
			var scale = GetLevelScale(i, maxLevel);
			GenerateLevel(i, scale, imgData, outputTiles);
		}

		var img = new GigaImage
		{
			Height = height,
			Width = width,
			MaxLevel = maxLevel,
			MinLevel = minLevel
		};
		return (img, outputTiles);
	}

	/// <summary>
	/// Generate tiles of for specific level
	/// </summary>
	/// <param name="level">Level to generate</param>
	/// <param name="levelScale">Level scale</param>
	/// <param name="srcImage">Source image</param>
	/// <param name="outputTiles">List to append tiles</param>
	private static void GenerateLevel(int level, double levelScale, Image srcImage, List<(string name, Stream file)> outputTiles)
	{
		var size = GetTileCount(levelScale, srcImage.Width, srcImage.Height);
		var iH = srcImage.Height;
		var iW = srcImage.Width;
		for (int y = 0; y < size.y; y++)
		{
			for (int x = 0; x < size.x; x++)
			{
				//Calculate position and scale
				var levelScaleInv = 1 / levelScale;
				var xSize = (int)(256 * levelScaleInv);
				var ySize = (int)(256 * levelScaleInv);
				var xOff = xSize * x;
				var yOff = ySize * y;

				var overflowX = Math.Max(0, (xOff + xSize) - iW);
				var overflowY = Math.Max(0, (yOff + ySize) - iH);

				var tile = srcImage.Clone(i =>
				{
					var w = xSize - overflowX;
					var h = ySize - overflowY;
					if (w != iW && h != iH)
						i.Crop(new Rectangle(xOff, yOff, w, h));

					i.Resize(new ResizeOptions
					{
						Mode = ResizeMode.BoxPad,
						Position = AnchorPositionMode.TopLeft,
						Size = new Size(256, 256),
					});
				});

				var result = new MemoryStream();
				tile.SaveAsPng(result);
				result.Position = 0;
				outputTiles.Add(($"{level}:{x}:{y}", result));
			}
		}
	}

	/// <summary>
	/// Calculates the scale for a specified level
	/// </summary>
	/// <param name="level">Level</param>
	/// <param name="maxLevel">Max level</param>
	/// <returns></returns>
	public static double GetLevelScale(int level, int maxLevel)
	{
		return 1 / Math.Pow(2, maxLevel - level);
	}

	/// <summary>
	/// Calculates the zoom level where the entire image is contained in one tile
	/// </summary>
	/// <param name="maxLevel">The max zoom level</param>
	/// <param name="width">The image width</param>
	/// <param name="height">The image height</param>
	/// <returns></returns>
	public static int GetClosestLevel(int maxLevel, int width, int height)
	{
		for (var i = 1; i <= maxLevel; i++)
		{
			var scale = GetLevelScale(i, maxLevel);
			var (x, y) = GetTileCount(scale, width, height);
			if (x > 1 || y > 1)
				return i - 1;
		}
		return 0;
	}

	/// <summary>
	/// Get tile count for a specific level scale
	/// </summary>
	/// <param name="levelScale">Level scale</param>
	/// <param name="dimX">Image width</param>
	/// <param name="dimY">Image height</param>
	/// <returns></returns>
	public static (int x, int y) GetTileCount(double levelScale, int dimX, int dimY)
	{
		var x = (int)Math.Ceiling(levelScale * dimX / 256);
		var y = (int)Math.Ceiling(levelScale * dimY / 256);
		return (x, y);
	}

	/// <summary>
	/// Calcualte the optimal max zoom level for a given image resolution
	/// </summary>
	/// <param name="width"></param>
	/// <param name="height"></param>
	/// <returns></returns>
	public static int CalculateMaxLevel(int width, int height)
	{
		return (int)Math.Ceiling(Math.Log(Math.Max(width, height)) / Math.Log(2));
	}

	/// <summary>
	/// Scale an image
	/// </summary>
	/// <param name="height"></param>
	/// <param name="width"></param>
	/// <param name="maxSideLength"></param>
	/// <returns></returns>
	public static (int h, int w) Scale(int height, int width, int maxSideLength)
	{
		if (height > width)
		{
			var r = (width / (float)height);
			return (maxSideLength, (int)(maxSideLength * r));
		}
		else
		{
			var r = (height / (float)width);
			return ((int)(maxSideLength * r), maxSideLength);
		}
	}
}

public class GigaImage
{
	[BsonId(IdGenerator = null)]
	public ObjectId Id { get; set; }

	public int Height { get; set; }
	public int Width { get; set; }
	public int MaxLevel { get; set; }
	public int MinLevel { get; set; }
	public Dictionary<string, ObjectId> Tiles { get; set; }
}