using SPO_1;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SPO_1
{
    public partial class Form1 : Form
    {
        private TextBox _codeBox;       // предпросмотр кода (слева)
        private TextBox _resultBox;     // результаты анализа (справа)
        private Button _openButton;
        private Label _fileLabel;

        public Form1()
        {
            Text = "Анализ сложности Java-программы (метрика Джилба)";
            Width = 1100;
            Height = 720;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(800, 500);

            // ---------- Верхняя панель: выбор файла ----------
            _openButton = new Button
            {
                Text = "Выбрать .java файл",
                Width = 180,
                Height = 30,
                Margin = new Padding(0, 0, 10, 0)
            };
            _openButton.Click += OpenButton_Click;

            _fileLabel = new Label
            {
                Text = "Файл не выбран",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.DimGray,
                Padding = new Padding(0, 6, 0, 0)
            };

            var topPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 45,
                Padding = new Padding(10, 8, 10, 8),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };
            topPanel.Controls.Add(_openButton);
            topPanel.Controls.Add(_fileLabel);

            // ---------- Основная область: слева код, справа анализ ----------
            _codeBox = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                ReadOnly = true,
                Font = new Font("Consolas", 10),
                Dock = DockStyle.Fill,
                BackColor = Color.White,

            };


            _resultBox = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                WordWrap = false,
                ReadOnly = true,
                Font = new Font("Consolas", 10),
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(250, 250, 250)
            };

            var leftGroup = new GroupBox
            {
                Text = "Загруженный код",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };
            leftGroup.Controls.Add(_codeBox);

            var rightGroup = new GroupBox
            {
                Text = "Результаты анализа",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };
            rightGroup.Controls.Add(_resultBox);

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 75,
                SplitterWidth = 6
            };
            split.Panel1.Controls.Add(leftGroup);
            split.Panel2.Controls.Add(rightGroup);

            // ---------- Сборка формы ----------
            Controls.Add(split);
            Controls.Add(topPanel);
        }

        // ---------- Обработчик выбора файла ----------
        private void OpenButton_Click(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Выберите Java-файл",
                Filter = "Java files (*.java)|*.java|All files (*.*)|*.*"
            };

            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            string path = dlg.FileName;
            string text;
            try
            {
                text = File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Не удалось прочитать файл:\r\n" + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _fileLabel.Text = path;

            _codeBox.Text = text.Replace("\n", "\r\n").Replace("\r\r\n", "\r\n");

            // Автообновление анализа сразу после загрузки
            UpdateAnalysis(text);
        }

        // ---------- Расчёт и вывод ----------
        private void UpdateAnalysis(string source)
        {
            try
            {
                var r = JavaGilbAnalyzer.Analyze(source);

                _resultBox.Text =
                    "=== Метрика Джилба (Java) ===\r\n\r\n" +
                    $"Абсолютная сложность CL       = {r.CL}\r\n" +
                    $"Относительная сложность cl    = {r.Cl:F4}\r\n" +
                    $"Макс. уровень вложенности CLI = {r.CLI}\r\n" +
                    $"Всего операторов (по ';')     = {r.TotalStatements}\r\n\r\n" +
                    "Пояснение:\r\n" +
                    "- CL — сумма всех ветвлений\r\n" +
                    "  (if, else-if, for, while, do, switch, ?:).\r\n" +
                    "- switch с n case-метками учитывается\r\n" +
                    "  как n-1 if, вложенность — n-2.\r\n" +
                    "- default и селектор switch не учитываются.\r\n" +
                    "- cl = CL / количество операторов.\r\n";
            }
            catch (Exception ex)
            {
                _resultBox.Text = "Ошибка анализа:\r\n" + ex.Message;
            }
        }
    }
}