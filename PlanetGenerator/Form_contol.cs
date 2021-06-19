using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MapGenerator
{
    public partial class Form_contol : Form
    {
        private Form_grid form_grid; // Окно для визуализации всех клеток карты.

        private int index;
        private int seed; // Сохраненное семя генерации.
        private int mapSize; // Сохраненный размер (ширина) карты

        public double[] NoiseMap;
        public bool noiseMapIsReady; // Готовность карты шумов.
        public bool NoiseMapIsReady
        {
            get => noiseMapIsReady;
            set { noiseMapIsReady = value; HeightMapIsReady = false; }
        }

        public int[] HeightMap;
        public bool heightMapIsReady;
        /// <summary>
        /// Готовность карты высот к отображению.
        /// </summary>
        public bool HeightMapIsReady
        {
            get => heightMapIsReady;
            set { heightMapIsReady = value; ModeTemperatureMapIsReady = false; }
        }

        public int[] BaseTemperatureMap;
        public bool baseTemperatureMapIsReady;
        /// <summary>
        /// Готовность карты начальных температур к отображению
        /// </summary>
        public bool BaseTemperatureMapIsReady
        {
            get => baseTemperatureMapIsReady;
            set { baseTemperatureMapIsReady = value; ModeTemperatureMapIsReady = false; }
        }

        public int[] ModeTemperatureMap;
        public bool modeTemperatureMapIsReady;
        /// <summary>
        /// Готовность карты итоговых температур к отображению
        /// </summary>
        public bool ModeTemperatureMapIsReady
        {
            get => modeTemperatureMapIsReady;
            set { modeTemperatureMapIsReady = value; }
        }

        public Form_contol()
        {
            InitializeComponent();
        }

        readonly Dictionary<string, NoiseMapType> DictNoiseMapType = new Dictionary<string, NoiseMapType>()
        {
            //["Тестовый шум А"] = NoiseMapType.testedA,
            //["Тестовый шум В"] = NoiseMapType.testedB,
            //["Тестовый шум C"] = NoiseMapType.testedC,
            //["Тестовый шум D"] = NoiseMapType.testedD,
            ["1D шум"] = NoiseMapType.simple1d,
            ["2D шум"] = NoiseMapType.simple2d,
            ["3D шум с Z смещением"] = NoiseMapType.simple3d,
            ["3D шум, замкнутый по X"] = NoiseMapType.looped3d,
            ["4D шум, замкнутый по X и Y"] = NoiseMapType.looped4d,
            ["domainWarped2D"] = NoiseMapType.domainWarped2d,
            ["domainWarped3D"] = NoiseMapType.domainWarped3d,
        };

        readonly Dictionary<string, ShowedMapType> DictShowedMapType = new Dictionary<string, ShowedMapType>()
        {
            ["Карта шумов"] = ShowedMapType.Noise,
            ["Карта высот"] = ShowedMapType.Landscape,
            ["Карта базовых температур"] = ShowedMapType.BaseTemperature,
            ["Карта модифицированных температур"] = ShowedMapType.ModeTemperature,
        };

        void Form_contol_Load(object sender, EventArgs e)
        {
            form_grid = new Form_grid();
            form_grid.Show(this);

            mapSize = (int)num_mapSize.Value;
            ResetMapFlags();
            NoiseMap = new double[mapSize * mapSize];
            HeightMap = new int[mapSize * mapSize];
            BaseTemperatureMap = new int[mapSize * mapSize];
            ModeTemperatureMap = new int[mapSize * mapSize];

            foreach (string key in DictNoiseMapType.Keys) cb_noise.Items.Add(key);
            cb_noise.SelectedIndex = 1;
            foreach (string key in DictShowedMapType.Keys) cb_map.Items.Add(key);
            cb_map.SelectedIndex = 0;

            form_grid.SetCanvasSize((int)num_size.Value, (int)num_mapSize.Value);
            PrepareToVisualization();
            Visualize();
        }

        void ResetMapFlags()
        {
            noiseMapIsReady = false;
            heightMapIsReady = false;
            baseTemperatureMapIsReady = false;
            modeTemperatureMapIsReady = false;
        }

        void Visualize()
        {
            try
            {
                Visualize(DictShowedMapType[cb_map.Text]);
            }
            catch (Exception _){ }
        }
        void Visualize(ShowedMapType showedMapType)
        {
            switch (showedMapType)
            {
                case ShowedMapType.Noise: // Карта шума Перлина
                    {
                        int color;
                        for (var x = 0; x < mapSize; x++)
                            for (var y = 0; y < mapSize; y++)
                            {
                                index = x + y * mapSize;
                                if (double.IsNaN(NoiseMap[index])) return;
                                color = (byte)(NoiseMap[index] * 255);
                                form_grid.DrawCell(x, y, Color.FromArgb(color, color, color));

                            }
                        break;
                    }
                case ShowedMapType.Landscape: // Карта высот
                    {
                        for (var x = 0; x < mapSize; x++)
                            for (var y = 0; y < mapSize; y++)
                            {
                                index = x + y * mapSize;
                                form_grid.DrawCell(x, y, GetHeightColor(HeightMap[index]));
                            }

                        break;
                    }
                case ShowedMapType.BaseTemperature: // Карта температур по умолчанию
                    {
                        for (var x = 0; x < mapSize; x++)
                            for (var y = 0; y < mapSize; y++)
                            {
                                index = x + y * mapSize;
                                form_grid.DrawCell(x, y, GetTemperatureColor(BaseTemperatureMap[index]));
                            }

                        break;
                    }
                case ShowedMapType.ModeTemperature: // Карта температур
                    {
                        for (var x = 0; x < mapSize; x++)
                            for (var y = 0; y < mapSize; y++)
                            {
                                index = x + y * mapSize;
                                form_grid.DrawCell(x, y, GetTemperatureColor(ModeTemperatureMap[index]));
                            }

                        break;
                    }
            }
            //form_grid.DrawGrid();
            form_grid.Redraw();
        }

        void PrepareNoiseMap(NoiseMapType noiseMapType)
        {
            if (NoiseMapIsReady) return;
            switch (noiseMapType)
            {

                case NoiseMapType.simple1d:
                    NoiseMap = MapGenerator.NoiseMap_simple1d(seed, mapSize,
                    (double)num_scale.Value, (int)num_yd.Value, (int)num_octaves.Value,
                    (double)num_persistance.Value);
                    break;
                case NoiseMapType.simple2d:
                    NoiseMap = MapGenerator.NoiseMap_simple2d(seed, mapSize,
                    (double)num_scale.Value, (int)num_xd.Value, (int)num_yd.Value, (int)num_octaves.Value,
                    (double)num_persistance.Value);
                    break;
                case NoiseMapType.domainWarped2d:
                    NoiseMap = MapGenerator.NoiseMap_domainWarped2D(seed, mapSize,
                    (double)num_scale.Value, (int)num_xd.Value, (int)num_yd.Value, 
                    (double)num_mode.Value, (double)num_dw11.Value, (double)num_dw12.Value, (double)num_dw21.Value, (double)num_dw22.Value,
                    (int)num_octaves.Value, (float)num_persistance.Value);
                    break;
                case NoiseMapType.domainWarped3d:
                    NoiseMap = MapGenerator.NoiseMap_domainWarped3D(seed, mapSize,
                    (double)num_scale.Value, (int)num_xd.Value, (int)num_yd.Value,
                    (double)num_mode.Value, (double)num_dw11.Value, (double)num_dw12.Value, (double)num_dw13.Value,
                    (double)num_dw21.Value, (double)num_dw22.Value, (double)num_dw23.Value,
                    (double)num_dw31.Value, (double)num_dw32.Value, (double)num_dw33.Value,
                    (int)num_octaves.Value, (double)num_persistance.Value);
                    break;
                case NoiseMapType.simple3d:
                    NoiseMap = MapGenerator.NoiseMap_simple3d(seed, mapSize,
                    (double)num_scale.Value, (int)num_xd.Value, (int)num_yd.Value, (float)num_zd.Value,(int)num_octaves.Value, (double)num_persistance.Value);
                    break;
                case NoiseMapType.looped3d:
                    NoiseMap = MapGenerator.NoiseMap_looped3d(seed, mapSize,
                    (double)num_scale.Value, (int)num_xd.Value, (int)num_yd.Value, (int)num_octaves.Value,
                    (double)num_persistance.Value);
                    break;
                case NoiseMapType.looped4d:
                    NoiseMap = MapGenerator.NoiseMap_looped4d(seed, mapSize,
                    (double)num_scale.Value, (int)num_xd.Value, (int)num_yd.Value, (int)num_octaves.Value,
                    (double)num_persistance.Value);
                    break;
                default: return;
            }
            NoiseMapIsReady = true;
        }
        void PrepareHeightMap()
        {
            if (!HeightMapIsReady)
            {
                //HeightMap = new int[(int)num_mapSize.Value * (int)num_mapSize.Value];
                HeightMap = MapGenerator.HeightMap(NoiseMap, (int)num_multiplier.Value,
                    (int)num_oceanLevel.Value,
                    (int)num_extraBorder.Value, (double)num_extraParam.Value);
                HeightMapIsReady = true;
            }
        }
        void PrepareBaseTemperatureMap()
        {
            if (!BaseTemperatureMapIsReady)
            {
                //BaseTemperatureMap = new int[(int)num_mapSize.Value * (int)num_mapSize.Value];
                BaseTemperatureMap = MapGenerator.BaseTemperatureMap(mapSize,
                    (int)num_temp.Value + 273,
                    (double)num_divisor.Value, (double)num_tempExp.Value, (double)num_equator.Value);
                BaseTemperatureMapIsReady = true;
            }
        }
        void PrepareModeTemperatureMap()
        {
            if (!ModeTemperatureMapIsReady)
            {
                //ModeTemperatureMap = new int[(int)num_mapSize.Value * (int)num_mapSize.Value];
                ModeTemperatureMap = MapGenerator.ModeTemperatureMap(BaseTemperatureMap, HeightMap,
                    (double)num_reduction.Value);
                ModeTemperatureMapIsReady = true;
            }
        }

        /// <summary>
        /// Генерирует запрашиваемую карту при её отсутствии.
        /// </summary>
        void PrepareToVisualization()
        {
            try
            {
                PrepareToVisualization(DictNoiseMapType[cb_noise.Text], DictShowedMapType[cb_map.Text]);
            }
            catch (System.Collections.Generic.KeyNotFoundException _) { }
        }

        /// <summary>
        /// Генерирует запрашиваемую карту при её отсутствии.
        /// </summary>
        /// <param name="noiseMapType">Тип карты шумов.</param>
        /// <param name="showedMapType">Тип отображаемой карты.</param>
        void PrepareToVisualization(NoiseMapType noiseMapType, ShowedMapType showedMapType)
        {
            switch (showedMapType)
            {
                case ShowedMapType.Noise: // Если выбрана карта шумов.
                    {
                        PrepareNoiseMap(noiseMapType);
                        break;
                    }
                case ShowedMapType.Landscape: // Если выбрана карта высот.
                    {
                        PrepareNoiseMap(noiseMapType);
                        PrepareHeightMap();
                        break;
                    }

                case ShowedMapType.BaseTemperature: // Если выбрана карта базовой температуры.
                    {
                        PrepareBaseTemperatureMap();
                        break;
                    }
                case ShowedMapType.ModeTemperature: // Если выбрана карта итоговой температуры.
                    {
                        PrepareNoiseMap(noiseMapType);
                        PrepareHeightMap();
                        PrepareBaseTemperatureMap();
                        PrepareModeTemperatureMap();
                        break;
                    }
            }
        }

        /// <summary>
        /// Обновление активности компонентов настройски шума в зависимости от выбранного типа шума.
        /// </summary>
        void UpdateComponentActivity(NoiseMapType noiseMapType)
        {
            switch (noiseMapType)
            {
                case NoiseMapType.simple1d:
                    num_yd.Enabled = false;
                    num_zd.Enabled = false;
                    break;
                case NoiseMapType.simple3d:
                    num_yd.Enabled = true;
                    num_zd.Enabled = true;
                    break;
                default:
                    num_yd.Enabled = true;
                    num_zd.Enabled = false;
                    break;
            }

            switch (noiseMapType)
            {
                case NoiseMapType.domainWarped2d:
                    gb_domainWarping.Enabled = true;
                    num_dw13.Enabled = false;
                    num_dw23.Enabled = false;
                    num_dw31.Enabled = false;
                    num_dw32.Enabled = false;
                    num_dw33.Enabled = false;
                    break;
                case NoiseMapType.domainWarped3d:
                    gb_domainWarping.Enabled = true;
                    num_dw13.Enabled = true;
                    num_dw23.Enabled = true;
                    num_dw31.Enabled = true;
                    num_dw32.Enabled = true;
                    num_dw33.Enabled = true;
                    break;
                default:
                    gb_domainWarping.Enabled = false;
                    break;
            }
        }

        /// <summary>
        /// Установка новых размеров карты.
        /// </summary>
        private void btn_sizeChange_Click(object sender, EventArgs e)
        {
            if (mapSize != num_mapSize.Value)
            {
                ResetMapFlags();
                mapSize = (int)num_mapSize.Value;
            }
            form_grid.SetCanvasSize((int)num_size.Value, (int)num_mapSize.Value);
            if (cb_sync.Checked)
            {
                PrepareToVisualization();
                Visualize();
            }
        }

        /// <summary>
        /// Генерация и визуализация карты.
        /// </summary>
        private void btn_generateMaps_Click(object sender, EventArgs e)
        {
            //ResetMapFlags();
            PrepareToVisualization();
            Visualize();
        }

        #region Обработчики событий для авто-генерации

        private void cb_noise_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateComponentActivity(DictNoiseMapType[cb_noise.Text]);
            NoiseMapIsReady = false;
            if (cb_sync.Checked)
            {
                PrepareToVisualization();
                Visualize();
            }
        }
        private void comboBox_map_SelectedIndexChanged(object sender, EventArgs e) // Изменение типа отображаемой карты.
        {
            if (cb_sync.Checked)
            {
                PrepareToVisualization();
                Visualize();
            }
        }
        private void сhangedSetting_NoiseMap(object sender, EventArgs e)
        {
            seed = (int)num_seed.Value;
            NoiseMapIsReady = false;
            if (cb_sync.Checked)
            {
                PrepareToVisualization();
                Visualize();
            }
        }
        private void changedSetting_HeightMap(object sender, EventArgs e)
        {
            HeightMapIsReady = false;
            if (cb_sync.Checked)
            {
                PrepareToVisualization();
                Visualize();
            }
        }
        private void changedSetting_TemperatureMap(object sender, EventArgs e)
        {
            BaseTemperatureMapIsReady = false;
            if (cb_sync.Checked)
            {
                PrepareToVisualization();
                Visualize();
            }
        }

        #endregion

        #region Методы получения цвета.

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
            //Console.WriteLine(color.ToString());
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

        #region Словари цветов.

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

        #endregion
    }
}