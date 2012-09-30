using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StrategyClient
{
    class Map
    {
        Image[,] View { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        private ImageSource[] tileSources;

        public Map(Grid mapGrid)
        {
            tileSources = new ImageSource[4];
            tileSources[(int)TileType.Blank] = new BitmapImage(new Uri("/StrategyClient;component/Graphics/blank.png", UriKind.RelativeOrAbsolute));
            tileSources[(int)TileType.Lake] = new BitmapImage(new Uri("/StrategyClient;component/Graphics/lake.png", UriKind.RelativeOrAbsolute));
            tileSources[(int)TileType.Forest] = new BitmapImage(new Uri("/StrategyClient;component/Graphics/forest.png", UriKind.RelativeOrAbsolute));
            tileSources[(int)TileType.TinyVillage] = new BitmapImage(new Uri("/StrategyClient;component/Graphics/v0.png", UriKind.RelativeOrAbsolute));
            View = new Image[11, 11];
            for (int i = 0; i < 121; i++)
            {
                View[i % 11, i / 11] = new Image();
                Image image = View[i % 11, i / 11];
                image.HorizontalAlignment = HorizontalAlignment.Left;
                image.VerticalAlignment = VerticalAlignment.Top;
                image.Width = 80;
                image.Height = 50;
                image.Source = tileSources[(int)TileType.Blank];
                image.Margin = new Thickness(79 * (i % 11), 49 * (i / 11), 0, 0);
                TileInfo info = new TileInfo();
                info.Type = TileType.Blank;
                info.X = i % 11;
                info.Y = i / 11;
                image.Tag = info;
                mapGrid.Children.Add(image);
            }
        }

        public void SetMap(List<string> data)
        {
            Width = int.Parse(data[0]);
            Height = int.Parse(data[1]);
            X = int.Parse(data[2]);
            Y = int.Parse(data[3]);
            data.RemoveRange(0, 4);
            List<TileInfo> infosToBeSet = new List<TileInfo>();
            for (int i = 0; i < 121; i++)
            {
                TileInfo info = View[i % 11, i / 11].Tag as TileInfo;
                info.X = X + i % 11 - 5;
                info.Y = Y + i / 11 - 5;
                int type = int.Parse(data[i]);
                if (type >= 128)
                {
                    View[i % 11, i / 11].Source = tileSources[(int)TileType.TinyVillage]; //TODO: detect village size
                    info.Type = TileType.TinyVillage;
                    infosToBeSet.Add(info);
                }
                else
                {
                    View[i % 11, i / 11].Source = tileSources[type];
                    info.Type = (TileType)type;
                }
                View[i % 11, i / 11].ToolTip = string.Format("[{0}|{1}]", info.X, info.Y);
            }
        }
    }
}