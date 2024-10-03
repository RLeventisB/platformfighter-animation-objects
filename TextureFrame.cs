using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json.Serialization;

namespace Editor.Objects
{
	public static class TextureManager
	{
		public static readonly Dictionary<string, RegistryData> PathIndexMap = new Dictionary<string, RegistryData>();

		public static void LoadTexture(string path, out IntPtr id)
		{
			object obj = 1;

			lock (obj)
			{
				if (PathIndexMap.TryGetValue(path, out RegistryData data))
				{
					data.Count++;
					id = data.ImguiId;
					PathIndexMap[path] = data;
					ExternalActions.GetTexture(id);

					return;
				}

				Texture2D newTexture = Texture2D.FromFile(ExternalActions.GetGraphicsDevice(), path);

				nint newTextureId = ExternalActions.BindTexture(newTexture);
				FileSystemWatcher watcher = new FileSystemWatcher(Path.GetDirectoryName(path));
				watcher.Filter = Path.GetFileName(path);
				watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
				watcher.EnableRaisingEvents = true;

				PathIndexMap[path] = new RegistryData(path, newTextureId, 1, watcher);

				id = newTextureId;
			}
		}

		public static bool UnloadTexture(string path)
		{
			object obj = 1;

			lock (obj)
			{
				if (PathIndexMap.TryGetValue(path, out RegistryData data))
				{
					data.Count--;

					if (data.Count > 0)
						return false;

					Texture2D texture = ExternalActions.GetTexture(data.ImguiId);
					ExternalActions.UnbindTexture(data.ImguiId);
					texture.Dispose();
					data.Dispose();
					PathIndexMap.Remove(path);

					return true;
				}

				return false;
			}
		}

		public struct RegistryData : IDisposable
		{
			public string Path { get; }
			public IntPtr ImguiId { get; set; }
			public int Count { get; set; }
			public FileSystemWatcher Watcher { get; }

			public RegistryData(string path, nint imguiId, int count, FileSystemWatcher watcher)
			{
				Path = path;
				ImguiId = imguiId;
				Count = count;
				Watcher = watcher;
				watcher.Changed += ReloadTexture;
			}

			public void ReloadTexture(object sender, FileSystemEventArgs args)
			{
				lock (ExternalActions.GetTextureDictionaryForRawUse())
				{
					int attempts = 0;

					while (attempts < 10)
					{
						try
						{
							Texture2D texture = ExternalActions.GetTexture(ImguiId);
							ExternalActions.UnbindTexture(ImguiId);
							texture.Dispose();

							texture = Texture2D.FromFile(ExternalActions.GetGraphicsDevice(), Path);

							ExternalActions.GetTextureDictionaryForRawUse()[ImguiId] = texture;
							attempts = 10;
						}
						catch
						{
							attempts++;
							Thread.Sleep(100);
						}
					}
				}
			}

			public void Dispose()
			{
				Watcher.Dispose();
			}
		}
	}
	public class TextureFrame : IDisposable, IAnimationObject
	{
		public string Path { get; set; }
		public string Name { get; set; }

		public NVector2 Pivot { get; set; }
		public Point FrameSize { get; set; }
		public Point FramePosition { get; set; }

		[JsonConstructor]
		public TextureFrame()
		{
			Name = null;
			Path = null;
			FrameSize = Point.Zero;
			FramePosition = Point.Zero;
			Pivot = NVector2.One;
		}

		public TextureFrame(string name, string path, Point? frameSize = null, Point? framePosition = null, NVector2? pivot = null)
		{
			Name = name;
			Path = path;
			LoadTexture();
			FrameSize = frameSize ?? new Point(Texture.Width, Texture.Height);
			FramePosition = framePosition ?? Point.Zero;
			Pivot = pivot ?? new NVector2(Texture.Width / 2f, Texture.Height / 2f);
		}

		public TextureFrame(string name, Texture2D texture, Point frameSize, Point? framePosition = null, NVector2? pivot = null)
		{
			Name = name;
			Path = "Syntetic";
			FrameSize = frameSize;
			FramePosition = framePosition ?? Point.Zero;
			Pivot = pivot ?? NVector2.Zero;
			TextureId = ExternalActions.BindTexture(texture);
		}

		public Point Size => new Point(Width, Height);
		public int Width => Texture.Width;
		public int Height => Texture.Height;
		public Texture2D Texture => ExternalActions.GetTexture(TextureId);
		[JsonIgnore]
		public nint TextureId { get; private set; }

		public void LoadTexture()
		{
			TextureManager.LoadTexture(Path, out nint id);
			TextureId = id;
		}

		public static implicit operator Texture2D(TextureFrame f)
		{
			return f.Texture;
		}

		public bool IsBeingHovered(Vector2 mouseWorld, int? frame) => false;

		public List<KeyframeableValue> EnumerateKeyframeableValues() => [];

		public void Dispose()
		{
			ExternalActions.UnbindTexture(TextureId);
			Texture?.Dispose();
		}

		public void Remove()
		{
			ExternalActions.OnRemoveTexture(this);

			if (Path == "Syntetic")
			{
				ExternalActions.UnbindTexture(TextureId);
			}
			else
				TextureManager.UnloadTexture(Path);
		}
	}
}