using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace cControl {
    public partial class Form1 : Form {
        public double[][] InputData; //Хранение входных данных
        public string filepath = ""; //Хранение пути до открываемого файла
        public double[] res; //Хранение выходных данных сигнала
        public int Total = 0; //Хранение количества значений сигналов на входе
        class POINT { public double X, Y; };//Класс для формирования координат.
        class FourArgs { public double a=-50, b=-50, c=200, d=200; } //класс для хранения аргументов принадлежности a, b, c, d.

        class InOut { //Класс для реализации значений входов-выходов.
            public string[] LingVar; public FourArgs[] val; //Лингвистические переменные с типом данных используещие класс выше
            //Лингв.переменные и экземпляры значений реализованы таким образом ввиду косой работы енумераторов и словарей в данном контексте.
            public InOut (string[] names) { //Конструктор класса
                LingVar = names;
                val = new FourArgs[names.Length];
                for (int i = 0; i < names.Length; i++)
                    val[i] = new FourArgs();
            }
            //Заполнение значений по лингвистической переменной
            public void SetByName (string name, double[] vals) { 
                int j = 0;
                for (int i = 0; i < LingVar.Length; i++)
                    if (name == LingVar[i]) { j = i; }
                val[j].a = vals[0]; val[j].b = vals[1]; val[j].c = vals[2]; val[j].d = vals[3];
            }
            //Получение значений по лингвистической переменной
            public FourArgs GetByName (string name) { 
                for (int i = 0; i < LingVar.Length; i++)
                    if (LingVar[i] == name)
                        return val[i];
                return new FourArgs();
            }
            //Реализация подсчёта П-функции графика.
            public double p_func (string name, double x) { //Реализация расчёта координаты для П-функции. Фактически есть возможность обойтись без Z/S/L функции задавая необходимые координаты.
                int j = 0;
                for (int i = 0; i < LingVar.Length; i++) {
                    if (name == LingVar[i]) {
                        j = i;
                    }
                }
                if (x <= val[j].a || x >= val[j].d) { 
                    return 0d; 
                } else if (val[j].b <= x && x <= val[j].c) { 
                    return 1d; 
                } else if (val[j].a < x && x < val[j].b) { 
                    return ((x - val[j].a) / (val[j].b - val[j].a)); 
                }
                return 1 - ((x - val[j].c) / (val[j].d - val[j].c));
            }
        }
        
        //Инициализация формы и установка формы по центру экрана.
        public Form1 () {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }

        //Подсчёт выходных сигналов для точек.
        public void calculate () {
            Total = ReadFile(filepath);
            if (Total == -1) { return; }
            res = new double[Total];
            //Создание экземпляров класса вход-выход.
            //Количество значений зависит от количества элементов массива строк на входе.
            //Таким образом мы описываем параметры фукнции принадлежности при помощи словарей.
            InOut RoomTemp = new InOut(new string[5] { "Очень холодно", "Холодно", "Комфортно", "Тепло", "Жарко" });
            //Заполнение данных определённого лингвистического значений.
            RoomTemp.SetByName("Очень холодно", new double[4] { -1000, -1000, 14, 16 });
            RoomTemp.SetByName("Холодно", new double[4] { 13, 15, 18, 22 });
            RoomTemp.SetByName("Комфортно", new double[4] { 18, 23, 26, 29 });
            RoomTemp.SetByName("Тепло", new double[4] { 25, 27, 29, 31 });
            RoomTemp.SetByName("Жарко", new double[4] { 28, 30, 1000, 1000 });
            
            InOut StreetTemp = new InOut(new string[7] { "Мороз", "Очень холодно", "Холодно", "Прохладно", "Комфортно", "Тепло", "Жарко" });
            StreetTemp.SetByName("Мороз", new double[4] { -1000, -1000, -25, -23 });
            StreetTemp.SetByName("Очень холодно", new double[4] { -30, -24, -15, -10 });
            StreetTemp.SetByName("Холодно", new double[4] { -15, -10, 0, 5 });
            StreetTemp.SetByName("Прохладно", new double[4] { 0, 5, 15, 20 });
            StreetTemp.SetByName("Комфортно", new double[4] { 18, 21, 26, 30 });
            StreetTemp.SetByName("Тепло", new double[4] { 22, 27, 32, 35 });
            StreetTemp.SetByName("Жарко", new double[4] { 30, 35, 1000, 1000 });
            
            InOut CarbonDioxide = new InOut(new string[3] { "Норма", "Высокое", "Критическое" });
            CarbonDioxide.SetByName("Норма", new double[4] { -1000, -1000, 0.06d, 0.08d });
            CarbonDioxide.SetByName("Высокое", new double[4] { 0.06d, 0.08d, 0.1d, 0.15d });
            CarbonDioxide.SetByName("Критическое", new double[4] { 0.1d, 0.15d, 1000, 1000 });
            
            InOut WinOpening = new InOut(new string[4] { "Закрыто", "Приоткрыто", "Открыто наполовину", "Полностью открыто" });
            WinOpening.SetByName("Закрыто", new double[4] { -1000, -1000, 0, 1 });
            WinOpening.SetByName("Приоткрыто", new double[4] { 1, 4, 15, 30 });
            WinOpening.SetByName("Открыто наполовину", new double[4] { 25, 35, 65, 75 });
            WinOpening.SetByName("Полностью открыто", new double[4] { 70, 75, 1000, 1000 });


            double[] TotalResult = new double[Total];
            double RT = 0, ST = 0, CD = 0; //Переменные для хранения значений из файла (RT-RoomTemp, ST-StreetTemp, CD-CarbonDioxide(углекислый газ)).
            for (int SC = 0; SC < Total; SC++) {

                //Перебор входных значений.
                RT = InputData[SC][0];
                ST = InputData[SC][1];
                CD = InputData[SC][2];

                //Создание словарей для каждого сигнала, рассчёт принадлежности по термам лингвистических переменных.
                Dictionary<string, double> RoomDict = new Dictionary<string, double>();
                foreach (string i in RoomTemp.LingVar) {
                    RoomDict.Add(i, RoomTemp.p_func(i, RT));
                }
                Dictionary<string, double> StreetDict = new Dictionary<string, double>();
                foreach (string i in StreetTemp.LingVar) {
                    StreetDict.Add(i, StreetTemp.p_func(i, ST));
                }
                Dictionary<string, double> CarbonDict = new Dictionary<string, double>();
                foreach (string i in CarbonDioxide.LingVar) {
                    CarbonDict.Add(i, CarbonDioxide.p_func(i, CD));
                }
                // Задаём контейнер для хранения результатов обработки правил.
                double[][] Rules = new double[21][];
                for (int i = 0; i < 21; i++) {
                    Rules[i] = new double[2];
                }

                // Обработка каждого приведённого правила - подсчёт левой части правил.
                Rules[0][0] = CarbonDict["Критическое"];
                Rules[1][0] = Math.Min(Math.Max(RoomDict["Очень холодно"], RoomDict["Холодно"]), Math.Max(StreetDict["Тепло"], StreetDict["Жарко"]));
                Rules[2][0] = Math.Min(RoomDict["Очень холодно"], StreetDict["Комфортно"]);
                Rules[3][0] = Math.Min(CarbonDict["Норма"], RoomDict["Комфортно"]);
                Rules[4][0] = Math.Min(Math.Min(CarbonDict["Норма"], Math.Max(RoomDict["Холодно"], RoomDict["Очень холодно"])), Math.Max(Math.Max(StreetDict["Прохладно"], StreetDict["Холодно"]), Math.Max(StreetDict["Очень холодно"], StreetDict["Мороз"])));
                Rules[5][0] = min(new double[3] { CarbonDict["Норма"], RoomDict["Холодно"], StreetDict["Комфортно"] });
                Rules[6][0] = min(new double[3] { CarbonDict["Норма"], Math.Max(RoomDict["Тепло"], RoomDict["Жарко"]), max(new double[3] { StreetDict["Комфортно"], StreetDict["Тепло"], StreetDict["Жарко"] }) });
                Rules[7][0] = min(new double[3] { CarbonDict["Норма"], RoomDict["Тепло"], Math.Max(StreetDict["Прохладно"], StreetDict["Комфортно"]) });
                Rules[8][0] = min(new double[3] { CarbonDict["Норма"], RoomDict["Тепло"], StreetDict["Холодно"] });
                Rules[9][0] = min(new double[3] { CarbonDict["Норма"], RoomDict["Тепло"], Math.Max(StreetDict["Очень холодно"], StreetDict["Мороз"]) });
                Rules[10][0] = min(new double[3] { CarbonDict["Норма"], RoomDict["Жарко"], Math.Max(StreetDict["Прохладно"], StreetDict["Холодно"]) });
                Rules[11][0] = min(new double[3] { CarbonDict["Норма"], RoomDict["Жарко"], Math.Max(StreetDict["Очень холодно"], StreetDict["Мороз"]) });
                Rules[12][0] = min(new double[3] { CarbonDict["Высокое"], RoomDict["Очень холодно"], Math.Max(StreetDict["Очень холодно"], StreetDict["Мороз"]) });
                Rules[13][0] = min(new double[3] { CarbonDict["Высокое"], Math.Max(RoomDict["Очень холодно"], RoomDict["Холодно"]), Math.Max(StreetDict["Холодно"], StreetDict["Прохладно"]) });
                Rules[14][0] = min(new double[3] { CarbonDict["Высокое"], RoomDict["Холодно"], StreetDict["Мороз"] });
                Rules[15][0] = min(new double[3] { CarbonDict["Высокое"], RoomDict["Холодно"], StreetDict["Очень холодно"] });
                Rules[16][0] = min(new double[3] { CarbonDict["Высокое"], RoomDict["Комфортно"], Math.Max(StreetDict["Очень холодно"], StreetDict["Мороз"]) });
                Rules[17][0] = min(new double[3] { CarbonDict["Высокое"], RoomDict["Комфортно"], max(new double[5] { StreetDict["Прохладно"], StreetDict["Холодно"], StreetDict["Комфортно"], StreetDict["Тепло"], StreetDict["Жарко"] }) });
                Rules[18][0] = min(new double[3] { CarbonDict["Высокое"], Math.Max(RoomDict["Тепло"], RoomDict["Жарко"]), StreetDict["Мороз"] });
                Rules[19][0] = min(new double[3] { CarbonDict["Высокое"], Math.Max(RoomDict["Тепло"], RoomDict["Жарко"]), max(new double[6] { StreetDict["Очень холодно"], StreetDict["Холодно"], StreetDict["Прохладно"], StreetDict["Комфортно"], StreetDict["Тепло"], StreetDict["Жарко"] }) });
                Rules[20][0] = min(new double[3] { CarbonDict["Высокое"], RoomDict["Холодно"], StreetDict["Комфортно"] });

                // Рассчитываем угол открытия окна для текущего условия
                double counter = 0;
                double[] mins = new double[21];
                double MaxValue = 0;
                double SumOfMax = 0;
                double MaxOnCount = 0;
                while (counter < 100) {
                    counter += 0.1d;
                    // Рассчёт правых частей правил.
                    Rules[0][1] = Rules[1][1] = Rules[2][1] = Rules[6][1] = Rules[17][1] = Rules[19][1] = Rules[20][1] = WinOpening.p_func("Полностью открыто", counter);
                    Rules[7][1] = Rules[10][1] = Rules[13][1] = Rules[15][1] = Rules[16][1] = Rules[18][1] = WinOpening.p_func("Открыто наполовину", counter);
                    Rules[5][1] = Rules[8][1] = Rules[11][1] = Rules[12][1] = Rules[14][1] = WinOpening.p_func("Приоткрыто", counter);
                    Rules[3][1] = Rules[4][1] = Rules[9][1] = WinOpening.p_func("Закрыто", counter);
                    // Корректировка нечётких множеств из правых частей правил методом минимума.
                    for (int i = 0; i < 21; i++) {
                        mins[i] = Math.Min(Rules[i][0], Rules[i][1]);
                    }
                    MaxValue = mins.Max(); // Объединения всех правил.
                    // Вспомогательные значения для численного вычисления интеграла, для скаляризации значения путем нахождения центра тяжести.
                    SumOfMax += MaxValue;
                    MaxOnCount += MaxValue * counter;
                }
                // Вычисление центра тяжести.
                TotalResult[SC] = Math.Round(MaxOnCount / SumOfMax);
                res[SC] = TotalResult[SC]; //Сохранение выходного результата.
            }

            // Поинты (координаты точек) необходимы для правильной подачи данных на графики.
            POINT[] RTP = new POINT[Total];
            POINT[] STP = new POINT[Total];
            POINT[] CDP = new POINT[Total];
            POINT[] TRP = new POINT[Total];
            for (int i = 0; i < Total; i++) {
                RTP[i] = new POINT();
                STP[i] = new POINT();
                CDP[i] = new POINT();
                TRP[i] = new POINT();
                RTP[i].X = i; RTP[i].Y = InputData[i][0];
                STP[i].X = i; STP[i].Y = InputData[i][1];
                CDP[i].X = i; CDP[i].Y = InputData[i][2];
                TRP[i].X = i; TRP[i].Y = TotalResult[i];
            }

            //Заполнение осей диаграмм.
            ChartFormation(chart1, 0, Total, 0, 35); // График температуры в комнате.
            ChartFormation(chart2, 0, Total, -30, 35); // График температуры на улице.
            ChartFormation(chart3, 0, Total, 0, 1.5d);  // График уровня углекислого газа.
            ChartFormation(chart4, 0, Total, 0, 100);  // График открытия окна.
            // Подача данных о выходном сигнале на график.
            OutputSignal(chart1, 0, "RoomTemp", RTP);
            OutputSignal(chart2, 0, "StreetTemp", STP);
            OutputSignal(chart3, 0, "CO2 Status", CDP);
            OutputSignal(chart4, 0, "WinOpen Graph", TRP);
        }

        // Функция для формирования осей диаграммы.
        private void ChartFormation (Chart chart, int xMin, int xMax, double yMin, double yMax) {
            //Задача предела для осей диаграммы
            chart.ChartAreas[0].AxisX.Minimum = xMin; chart.ChartAreas[0].AxisX.Maximum = xMax;
            chart.ChartAreas[0].AxisY.Minimum = yMin; chart.ChartAreas[0].AxisY.Maximum = yMax;
            //Задача формата вывода подписей для осей диаграммы
            chart.ChartAreas[0].AxisY.LabelStyle.Format = "{0:0.0}";
            chart.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0}";
            chart.Series[0].YValueType = chart.Series[0].XValueType = ChartValueType.Double;
            chart.Series[0].BorderWidth = 3;
            chart.ChartAreas[0].AxisY.IntervalType = chart.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Number;
        }

        //Функция для заполнения сигнала по координатам XY.
        private void OutputSignal (Chart chart, int charAreas, string LineLabel, POINT[] points) {
            //Очищаем график
            chart.Series[charAreas].Points.Clear();
            //Устанавливаем наименование линии.
            chart.Series[charAreas].Name = LineLabel;
            //Устанавливаем точки на диаграмме из массива точек POINT
            foreach (POINT point in points) {
                chart.Series[charAreas].Points.AddXY(point.X, point.Y);
            }
        }

        //Функция для чтения файла. Файл записывается в csv - текстовый формат данных представляющий описание таблиц.
        //В качестве разделителя используется стандартный разделитель - ";"
        private int ReadFile (string FileName) {
            //считывание текста из файла и разбиение по пробелам в массив строк
            string file_data = " ";
            try {
                file_data = File.ReadAllText(FileName);
            } catch {
                MessageBox.Show("Неполадки при открытии файла. Возможно файл занят другим процессом.");
                return -1;
            }
            //Считываем строки
            string[] strings = file_data.Split('\n');
            string[][] parsedData = new string[strings.Length-2][];
            InputData = new double[strings.Length-2][];

            for (int i = 0; i < strings.Length-2; i++) {
                parsedData[i] = strings[i+1].Split(';');
                InputData[i] = new double[3];
                for (int j = 0; j < 3; j++) {
                    try {
                        InputData[i][j] = Convert.ToDouble(parsedData[i][j].Replace('.', ','));
                    } catch {
                        MessageBox.Show("Возникли ошибки при открытии файла. Проверьте исходные данные на правильность записи.");
                    }
                }
            }
            return strings.Length-2;
        }
        //Функция нахождения минимума среди кучи значений.
        private double min (double[] args) {
            double minimum = args[0];
            for (int i = 0; i < args.Length; i++) {
                if (args[i] < minimum)
                    minimum = args[i];
            }
            return minimum;
        }
        //Функция нахождения максимума среди кучи значений.
        private double max (double[] args) {
            double maximum = args[0];
            for (int i = 0; i < args.Length; i++) {
                if (args[i] > maximum)
                    maximum = args[i];
            }
            return maximum;
        }
        // Реализация подсчёта точек четырёх типов функций. Оставлено в качестве шаблона, не используется.
        private double l_func (double x, double a, double b) { return (x - a) / (b - a); }
        private double p_func (double x, double a, double b, double c, double d) {
            if (x <= a || x >= d) { return 0d; } 
            else if (b <= x && x <= c) { return 1d; } 
            else if (a < x && x < b) { return l_func(x, a, b); }
            return 1 - l_func(x, c, d);
        }
        private double z_func (double x, double a, double b) {
            if (x <= a) { return 1d; } 
            else if (a < x && x < b) { return l_func(x, a, b); } 
            return 0d;
        }
        private double s_func (double x, double a, double b) {
            if (x <= a) { return 0d; }
            else if (a < x && x < b) { return l_func(x, a, b); }
            return 1d;
        }

        // Сохранение и загрузка файла. Реализовано таким образом, чтобы сохранённый файл можно было заново открыть и прочитать.
        private void button1_Click (object sender, EventArgs e) { // Загрузка
            OpenFileDialog OPF = new OpenFileDialog();
            OPF.Filter = "Файлы csv|*.csv";
            if (OPF.ShowDialog() == DialogResult.Cancel) {
                return;
            }
            filepath = OPF.FileName;
            calculate();
        }
        private void button2_Click (object sender, EventArgs e) { // Сохранение
            if (Total == 0) {
                MessageBox.Show("Нет данных для отображения.");
                return;
            }
            SaveFileDialog SFD = new SaveFileDialog();
            SFD.Filter = "Файлы таблиц csv|*.csv";
            if (SFD.ShowDialog() == DialogResult.Cancel) {
                return;
            }
            if (SFD.FileName != null) {
                string fileout = "t дома; t на улице; CO2; Window\n";
                for (int i = 0; i < Total; i++) {
                    fileout += InputData[i][0].ToString().Replace(',', '.') + ";" + InputData[i][1].ToString().Replace(',', '.') + ";" + InputData[i][2].ToString().Replace(',', '.') + ";" + res[i].ToString().Replace(',', '.') + "\n";
                }
                File.WriteAllText(SFD.FileName, fileout, Encoding.UTF8);
            }
        }
    }
}