using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MapGenerator
{
    public partial class Form_contol : Form
    {
        public int[] BaseTemperatureMap;
        public bool BaseTemperatureMapIsReady; // Готовность карты начальных температур к отображению.
        private decimal dist; // Сохраненное расстояние до звезды.
        private Form_grid form_grid; // Окно для визуализации всех клеток карты.
        public Form_info form_info; // Окно для вывода подробной информации о клетке.
        public int[] HeightMap;
        public bool HeightMapIsReady; // Готовность карты высот к отображению.
        private int index;
        private int mapSize; // Сохраненный размер (ширина) карты

        public double[] NoiseMap;

        public bool NoiseMapIsReady; // Готовность карты шумов.
        public int[] RainFallMap;
        public bool RainFallMapIsReady; // Готовность карты осадков к отображению.
        public bool RainNoiseMapIsReady; // Готовность карты шумов.
        private int seed; // Сохраненное семя генерации.
        private int starType; // Сохраненный тип звезды.
        public int[] TemperatureMap;
        public bool TemperatureMapIsReady; // Готовность карты итоговых температур к отображению.

        public Form_contol()
        {
            InitializeComponent();
        }

        private void Form_contol_Load(object sender, EventArgs e)
        {
            form_grid = new Form_grid();
            form_grid.Show(this);
            form_info = new Form_info();
            form_info.Show(this);
            form_info.Hide();

            NoiseMapIsReady = false;
            HeightMapIsReady = false;
            BaseTemperatureMapIsReady = false;
            TemperatureMapIsReady = false;
            RainFallMapIsReady = false;

            NoiseMap = new double[(int) num_mapSize.Value * (int) num_mapSize.Value];
            HeightMap = new int[(int) num_mapSize.Value * (int) num_mapSize.Value];
            BaseTemperatureMap = new int[(int) num_mapSize.Value * (int) num_mapSize.Value];
            TemperatureMap = new int[(int) num_mapSize.Value * (int) num_mapSize.Value];
            RainFallMap = new int[(int) num_mapSize.Value * (int) num_mapSize.Value];

            comboBox_map.SelectedIndex = 0;
            comboBox_starType.SelectedIndex = 4;
            comboBox_planetType.SelectedIndex = 0;
            num_temp.Value = 26;

            starType = comboBox_starType.SelectedIndex;
            dist = num_dist.Value;
            mapSize = (int) num_mapSize.Value;

            form_grid.SetCanvasSize((int) num_size.Value, (int) num_mapSize.Value);
            PrepareToVisualization(comboBox_map.SelectedIndex);
            Visualize(comboBox_map.SelectedIndex);
        }

        public void Visualize(int mapIndex)
        {
            switch (mapIndex)
            {
                case 0: // Карта шума Перлина
                {
                    for (var x = 0; x < mapSize; x++)
                    for (var y = 0; y < mapSize; y++)
                    {
                        index = x + y * mapSize;
                        if (double.IsNaN(NoiseMap[index]))
                            return;
                        form_grid.DrawCell(x, y,
                            Color.FromArgb((int) (NoiseMap[index] * 255), (int) (NoiseMap[index] * 255),
                                (int) (NoiseMap[index] * 255)));
                    }

                    break;
                }
                case 1: // Карта высот
                {
                    for (var x = 0; x < mapSize; x++)
                    for (var y = 0; y < mapSize; y++)
                    {
                        index = x + y * mapSize;
                        form_grid.DrawCell(x, y, GetHeightColor(HeightMap[index]));
                    }

                    break;
                }
                case 2: // Карта температур по умолчанию
                {
                    for (var x = 0; x < mapSize; x++)
                    for (var y = 0; y < mapSize; y++)
                    {
                        index = x + y * mapSize;
                        form_grid.DrawCell(x, y, GetTemperatureColor(BaseTemperatureMap[index]));
                    }

                    break;
                }
                case 3: // Карта температур
                {
                    for (var x = 0; x < mapSize; x++)
                    for (var y = 0; y < mapSize; y++)
                    {
                        index = x + y * mapSize;
                        form_grid.DrawCell(x, y, GetTemperatureColor(TemperatureMap[index]));
                    }

                    break;
                }
                case 4: // Карта осадков
                {
                    for (var x = 0; x < mapSize; x++)
                    for (var y = 0; y < mapSize; y++)
                    {
                        index = x + y * mapSize;
                        form_grid.DrawCell(x, y, GetRainfallColor(RainFallMap[index]));
                    }

                    break;
                }
            }

            form_grid.Redraw();
        }

        private void comboBox_map_SelectedIndexChanged(object sender, EventArgs e) // Изменение типа отображаемой карты.
        {
            PrepareToVisualization(comboBox_map.SelectedIndex);
            Visualize(comboBox_map.SelectedIndex);
        }

        public void PrepareToVisualization(int mapType) // Генерируем запрашиваемую карту при её отсутствии
        {
            switch (mapType)
            {
                case 0: // Если выбрана карта шумов.
                {
                    if (!NoiseMapIsReady)
                    {
                        NoiseMap = new double[(int) num_mapSize.Value * (int) num_mapSize.Value];
                        NoiseMap = MapGenerator.GenerateNoiseMap((int) num_seed.Value, (int) num_mapSize.Value,
                            (float) num_scale.Value, (int) num_xd.Value, (int) num_yd.Value, (int) num_octaves.Value,
                            (float) num_persistance.Value);
                        NoiseMapIsReady = true;
                    }

                    break;
                }
                case 1: // Если выбрана карта высот.
                {
                    if (!NoiseMapIsReady)
                    {
                        NoiseMap = new double[(int) num_mapSize.Value * (int) num_mapSize.Value];
                        NoiseMap = MapGenerator.GenerateNoiseMap((int) num_seed.Value, (int) num_mapSize.Value,
                            (float) num_scale.Value, (int) num_xd.Value, (int) num_yd.Value, (int) num_octaves.Value,
                            (float) num_persistance.Value);
                        NoiseMapIsReady = true;
                    }

                    if (!HeightMapIsReady)
                    {
                        HeightMap = new int[(int) num_mapSize.Value * (int) num_mapSize.Value];
                        HeightMap = MapGenerator.GenerateHeightMap(NoiseMap, (int) num_multiplier.Value,
                            (int) num_oceanLevel.Value,
                            (int) num_extraBorder.Value, (double) num_extraParam.Value);
                        HeightMapIsReady = true;
                    }

                    break;
                }

                case 2: // Если выбрана карта базовой температуры.
                {
                    if (!BaseTemperatureMapIsReady)
                    {
                        BaseTemperatureMap = new int[(int) num_mapSize.Value * (int) num_mapSize.Value];
                        BaseTemperatureMap = MapGenerator.GenerateBaseTemperatureMap((int) num_mapSize.Value,
                            (int) num_temp.Value + 273,
                            (double) num_divisor.Value, (double) num_tempExp.Value, (double) num_equator.Value);
                        BaseTemperatureMapIsReady = true;
                    }

                    break;
                }
                case 3: // Если выбрана карта итоговой температуры.
                {
                    if (!NoiseMapIsReady)
                    {
                        NoiseMap = new double[(int) num_mapSize.Value * (int) num_mapSize.Value];
                        NoiseMap = MapGenerator.GenerateNoiseMap((int) num_seed.Value, (int) num_mapSize.Value,
                            (float) num_scale.Value, (int) num_xd.Value, (int) num_yd.Value, (int) num_octaves.Value,
                            (float) num_persistance.Value);
                        NoiseMapIsReady = true;
                    }

                    if (!HeightMapIsReady)
                    {
                        HeightMap = new int[(int) num_mapSize.Value * (int) num_mapSize.Value];
                        HeightMap = MapGenerator.GenerateHeightMap(NoiseMap, (int) num_multiplier.Value,
                            (int) num_oceanLevel.Value,
                            (int) num_extraBorder.Value, (double) num_extraParam.Value);
                        HeightMapIsReady = true;
                    }

                    if (!BaseTemperatureMapIsReady)
                    {
                        BaseTemperatureMap = new int[(int) num_mapSize.Value * (int) num_mapSize.Value];
                        BaseTemperatureMap = MapGenerator.GenerateBaseTemperatureMap((int) num_mapSize.Value,
                            (int) num_temp.Value + 273,
                            (double) num_divisor.Value, (double) num_tempExp.Value, (double) num_equator.Value);
                        BaseTemperatureMapIsReady = true;
                    }

                    if (!TemperatureMapIsReady)
                    {
                        TemperatureMap = new int[(int) num_mapSize.Value * (int) num_mapSize.Value];
                        TemperatureMap = MapGenerator.GenerateTemperatureMap(BaseTemperatureMap, HeightMap,
                            (double) num_reduction.Value);
                        TemperatureMapIsReady = true;
                    }

                    break;
                }
                case 4: // Если выбрана карта осадков.
                {
                    if (!RainNoiseMapIsReady)
                    {
                        RainFallMap = new int[(int) num_mapSize.Value * (int) num_mapSize.Value];
                        RainFallMap = MapGenerator.GenerateMap((int) num_rainSeed.Value, (int) num_mapSize.Value,
                            (float) num_rainScale.Value, (int) num_rainMultiplier.Value);
                        RainFallMapIsReady = true;
                    }

                    break;
                }
            }
        }

        private void btn_sizeChange_Click(object sender, EventArgs e) // Изменение размера карты
        {
            if (mapSize != num_mapSize.Value)
            {
                NoiseMapIsReady = false;
                HeightMapIsReady = false;
                BaseTemperatureMapIsReady = false;
                TemperatureMapIsReady = false;
                RainFallMapIsReady = false;

                mapSize = (int) num_mapSize.Value;
            }

            form_grid.SetCanvasSize((int) num_size.Value, (int) num_mapSize.Value);
            PrepareToVisualization(comboBox_map.SelectedIndex);
            Visualize(comboBox_map.SelectedIndex);
        }

        private void btn_generateMaps_Click(object sender, EventArgs e)
        {
            NoiseMapIsReady = false;
            HeightMapIsReady = false;
            BaseTemperatureMapIsReady = false;
            TemperatureMapIsReady = false;
            RainFallMapIsReady = false;
            PrepareToVisualization(comboBox_map.SelectedIndex);
            Visualize(comboBox_map.SelectedIndex);
        }

        private void сhangedSetting_NoiseMap(object sender, EventArgs e)
        {
            сhangedSetting_NoiseMap();
        }

        private void сhangedSetting_NoiseMap()
        {
            NoiseMapIsReady = false;
            HeightMapIsReady = false;
            TemperatureMapIsReady = false;
            if (cb_sync.Checked)
            {
                PrepareToVisualization(comboBox_map.SelectedIndex);
                Visualize(comboBox_map.SelectedIndex);
            }
        }

        private void changedSetting_HeightMap(object sender, EventArgs e)
        {
            changedSetting_HeightMap();
        }

        private void changedSetting_HeightMap()
        {
            HeightMapIsReady = false;
            TemperatureMapIsReady = false;
            if (cb_sync.Checked)
            {
                PrepareToVisualization(comboBox_map.SelectedIndex);
                Visualize(comboBox_map.SelectedIndex);
            }
        }

        private void changedSetting_TemperatureMap(object sender, EventArgs e)
        {
            changedSetting_TemperatureMap();
        }

        private void changedSetting_TemperatureMap()
        {
            BaseTemperatureMapIsReady = false;
            TemperatureMapIsReady = false;
            if (cb_sync.Checked)
            {
                PrepareToVisualization(comboBox_map.SelectedIndex);
                Visualize(comboBox_map.SelectedIndex);
            }
        }

        private void сhangedSetting_RainNoiseMap(object sender, EventArgs e)
        {
            сhangedSetting_RainNoiseMap();
        }

        private void сhangedSetting_RainNoiseMap()
        {
            RainNoiseMapIsReady = false;
            RainFallMapIsReady = false;
            if (cb_sync.Checked)
            {
                PrepareToVisualization(comboBox_map.SelectedIndex);
                Visualize(comboBox_map.SelectedIndex);
            }
        }

        private void changedSetting_RainMap(object sender, EventArgs e)
        {
            changedSetting_RainMap();
        }

        private void changedSetting_RainMap()
        {
            RainFallMapIsReady = false;
            if (cb_sync.Checked)
            {
                PrepareToVisualization(comboBox_map.SelectedIndex);
                Visualize(comboBox_map.SelectedIndex);
            }
        }

        #region Получение цвета

        private Color GetHeightColor(int height)
        {
            var color = Color.Black;
            HeightColors.Keys.ToList().ForEach(c =>
            {
                if (height >= c) color = HeightColors[c];
            });
            //Console.WriteLine(color.ToString());
            return color;
        }

        private Color GetRainfallColor(int rain)
        {
            var color = Color.Black;
            RainFallColor.Keys.ToList().ForEach(c =>
            {
                if (rain >= c) color = RainFallColor[c];
            });
            return color;
        }

        private Color GetTemperatureColor(int temp)
        {
            var color = Color.Black;
            TemperatureColor.Keys.ToList().ForEach(c =>
            {
                if (temp >= c) color = TemperatureColor[c];
            });
            //Console.WriteLine(color.ToString());
            return color;
        }

        #endregion

        #region Табличные штучки

        private readonly Dictionary<int, Color> HeightColors = new Dictionary<int, Color>
        {
            [-60000] = Color.FromArgb(101, 170, 211),
            [-2500] = Color.FromArgb(126, 207, 224), // -6000
            [-1000] = Color.FromArgb(178, 227, 224), // -4000
            [-500] = Color.FromArgb(203, 236, 243), // -2000
            [-200] = Color.FromArgb(203, 236, 243), // 128,204,168
            [0] = Color.FromArgb(180, 223, 151),
            [200] = Color.FromArgb(242, 250, 128),
            [500] = Color.FromArgb(254, 205, 139),
            [1000] = Color.FromArgb(250, 167, 87),
            [2000] = Color.FromArgb(229, 122, 52),
            [3000] = Color.FromArgb(206, 70, 28),
            [5000] = Color.FromArgb(181, 44, 2)
        };

        private readonly Dictionary<int, Color> TemperatureColor = new Dictionary<int, Color>
        {
            [0] = Color.FromArgb(81, 113, 198),
            [209] = Color.FromArgb(62, 132, 201),
            [217] = Color.FromArgb(16, 99, 141),
            [225] = Color.FromArgb(58, 177, 219),
            [233] = Color.FromArgb(53, 167, 168),
            [241] = Color.FromArgb(129, 193, 166),
            [249] = Color.FromArgb(182, 218, 96),
            [257] = Color.FromArgb(209, 227, 151),
            [265] = Color.FromArgb(213, 231, 181),
            [273] = Color.FromArgb(248, 245, 204),
            [281] = Color.FromArgb(250, 228, 168),
            [289] = Color.FromArgb(241, 196, 111),
            [297] = Color.FromArgb(238, 163, 106),
            [305] = Color.FromArgb(247, 164, 116),
            [315] = Color.FromArgb(247, 128, 59),
            [322] = Color.FromArgb(241, 82, 42),
            [333] = Color.FromArgb(255, 28, 20)
        };

        private readonly Dictionary<int, Color> RainFallColor = new Dictionary<int, Color>
        {
            [0] = Color.FromArgb(243, 170, 91),
            [100] = Color.FromArgb(242, 226, 105),
            [250] = Color.FromArgb(211, 206, 122),
            [500] = Color.FromArgb(213, 237, 153),
            [1000] = Color.FromArgb(168, 221, 179),
            [2000] = Color.FromArgb(159, 222, 237),
            [3000] = Color.FromArgb(87, 164, 210),
            [5000] = Color.FromArgb(73, 50, 166)
        };

        private readonly List<int> StarTemperature = new List<int>
        {
            30000, // O
            10000, // B
            7500, // A
            6000, // F
            5000, // G
            3500, // K
            2000 // M
        };

        #endregion

        #region Обработчики событий для авто-генерации

        #endregion

        #region Космическое

        private void comboBox_starType_SelectedIndexChanged(object sender, EventArgs e) // Изменение типа звезды.
        {
            num_temp.Value *=
                (decimal) StarTemperature[comboBox_starType.SelectedIndex] /
                StarTemperature
                    [starType]; // Изменяем температуру на экваторе пропорционально изменению температуры звезды.
            starType = comboBox_starType.SelectedIndex;
            changedSetting_HeightMap();
        }

        private void num_dist_ValueChanged(object sender, EventArgs e) // Изменение расстояния до звезды.
        {
            num_temp.Value *=
                dist / num_dist
                    .Value; // Изменяем температуру на экваторе пропорционально изменению расстояния до звезды.
            dist = num_dist.Value;
            changedSetting_HeightMap();

            // if (cb_sync.Checked) btn_generateMaps.PerformClick();
        }

        private void comboBox_planetType_SelectedIndexChanged(object sender, EventArgs e) // Изменение типа планеты
        {
        }

        #endregion
    }
}