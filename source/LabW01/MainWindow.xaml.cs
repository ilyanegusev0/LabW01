using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;


namespace LabW01
{
    public partial class MainWindow : Window
    {
        private static readonly Random _random = new Random();

        private Dictionary<string, string> _configurationMap = new Dictionary<string, string>();
        private Dictionary<char, char> _replacedLetters = new Dictionary<char, char>();

        private enum eLanguage
        {
            UNKNOWN,
            UKRAINIAN,
            ENGLISH
        }
        private static eLanguage _language;

        private static string _alphabet;
        private static readonly string _uaAlphabet = "абвгґдеєжзиіїйклмнопрстуфхцчшщьюя";
        private static readonly string _enAlphabet = "abcdefghijklmnopqrstuvwxyz";

        private static string _decryptedText;
        private static string _encryptedText;

        DispatcherTimer _secondTimer;
        private static int _seconds = 0;

        private static bool _successNoticied = false;

        //========================================================================================================================================================================================================

        public MainWindow()
        {
            InitializeComponent();

            string[] lines = ReadAllLinesInFile("text.txt");

            CreateConfigurationMap("config.cfg");

            _decryptedText = SplitText(lines);

            IdentifyLanguage(_decryptedText);

            string shuffledAlphabet = ShuffleAlphabet(_alphabet);

            Dictionary<char, char> substitutionMap = CreateSubstitutionMap(_alphabet, shuffledAlphabet);

            _encryptedText = EncryptText(_decryptedText, substitutionMap);

            _secondTimer = new DispatcherTimer();
            _secondTimer.Interval = TimeSpan.FromSeconds(1);
            _secondTimer.Tick += SecondUpdate;

            UpdateCryptotext(_encryptedText);

            UpdateTime();

        }

        //========================================================================================================================================================================================================

        private string[] ReadAllLinesInFile(string path)
        {
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }

            string[] lines = File.ReadAllLines(path);

            return lines;
        }

        private void CreateConfigurationMap(string path)
        {
            if (!File.Exists(path))
            {
                string text = "texts=0";
                File.WriteAllText(path, text);
            }

            string[] lines = ReadAllLinesInFile(path);

            string[] parts;
            foreach (string line in lines)
            {
                parts = line.Split('=');

                _configurationMap[parts[0].Trim()] = parts[1].Trim();
            }
        }

        private string SplitText(string[] lines)
        {
            int totalTexts = Convert.ToInt32(_configurationMap["texts"]);
            StringBuilder text = new StringBuilder();

            var texts = new List<string>();
            StringBuilder currentText = new StringBuilder();

            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    if (currentText.Length > 0)
                    {
                        texts.Add(currentText.ToString().Trim());
                        currentText.Clear();
                    }
                }
                else
                {
                    currentText.Append(line);
                    currentText.Append("\n");
                }
            }
            if (currentText.Length > 0)
            {
                texts.Add(currentText.ToString().Trim());
            }

            if (totalTexts == 0)
            {
                totalTexts = texts.Count;
            }


            for (int i = totalTexts; i > 0; i--)
            {
                int num = _random.Next(0, texts.Count);

                text.Append(texts[num]);
                text.AppendLine();
                texts.RemoveAt(num);
            }

            return text.ToString().Trim();
        }

        private void IdentifyLanguage(string text)
        {
            int uaLettersCount = 0;
            int enLetterCount = 0;

            foreach (char sym in text)
            {
                char letter = char.ToLower(sym);

                if (char.IsLetter(sym))
                {
                    if (_uaAlphabet.Contains(letter))
                    {
                        uaLettersCount++;
                    }
                    else if (_enAlphabet.Contains(letter))
                    {
                        enLetterCount++;
                    }
                }
            }

            if (uaLettersCount > enLetterCount)
            {
                _language = eLanguage.UKRAINIAN;
            }
            else if (enLetterCount > uaLettersCount)
            {
                _language = eLanguage.ENGLISH;
            }
            else if (uaLettersCount == enLetterCount && uaLettersCount != 0 && enLetterCount != 0)
            {
                switch (_random.Next(2))
                {
                    case 0:
                        _language = eLanguage.UKRAINIAN;
                        break;
                    case 1:
                        _language = eLanguage.ENGLISH;
                        break;
                }
            }
            else
            {
                _language = eLanguage.UNKNOWN;
            }

            switch (_language)
            {

                case eLanguage.UNKNOWN:
                    _alphabet = "";
                    lLanguage.Content = "***";
                    break;
                case eLanguage.UKRAINIAN:
                    _alphabet = _uaAlphabet;
                    lLanguage.Content = "Українська";
                    break;
                case eLanguage.ENGLISH:
                    _alphabet = _enAlphabet;
                    lLanguage.Content = "Англійська";
                    break;
            }
        }

        private string ShuffleAlphabet(string alphabet)
        {
            var shuffledAlphabet = new List<char>(alphabet);

            for (int i = shuffledAlphabet.Count - 1; i >= 0; i--)
            {
                int randomIndex = _random.Next(i + 1);

                while (shuffledAlphabet[randomIndex] == alphabet[i])
                {
                    randomIndex = _random.Next(i + 1);

                    if (i == 0)
                    {
                        randomIndex = _random.Next(shuffledAlphabet.Count);
                        continue;
                    }
                }

                char temp = shuffledAlphabet[randomIndex];
                shuffledAlphabet[randomIndex] = shuffledAlphabet[i];
                shuffledAlphabet[i] = temp;
            }

            return new string(shuffledAlphabet.ToArray());
        }

        private Dictionary<char, char> CreateSubstitutionMap(string originalAlphabet, string shuffledAlphabet)
        {
            var substitutionMap = new Dictionary<char, char>();
            for (int i = 0; i < originalAlphabet.Length; i++)
            {
                substitutionMap[originalAlphabet[i]] = shuffledAlphabet[i];
            }
            return substitutionMap;
        }

        private string EncryptText(string text, Dictionary<char, char> substitutionMap)
        {
            var encryptedText = new StringBuilder();
            foreach (char sym in text)
            {
                char letter = char.ToLower(sym);
                if (substitutionMap.ContainsKey(letter))
                {
                    char encryptedLetter = substitutionMap[letter];
                    if (char.IsUpper(sym))
                    {
                        encryptedLetter = char.ToUpper(encryptedLetter);
                    }
                    encryptedText.Append(encryptedLetter);
                }
                else
                {
                    encryptedText.Append(sym);
                }
            }
            return encryptedText.ToString();
        }

        private void UpdateCryptotext(string text)
        {
            var paragraph = new Paragraph();
            var runs = new List<Run>();

            bool HasEncryptedLetters = false;

            foreach (char sym in text)
            {
                var run = new Run(sym.ToString());
                char letter = char.ToLower(sym);

                if (cbShowRelpacedOnly.IsChecked == true)
                {
                    if (char.IsLetter(sym) && !_replacedLetters.ContainsKey(letter) && _alphabet.Contains(letter))
                    {
                        run.Text = "#";
                        run.Foreground = Brushes.Red;
                    }
                }
                else if (cbShowEncryptedOnly.IsChecked == true)
                {
                    run.Foreground = Brushes.Red;
                }

                if (cbShowEncryptedOnly.IsChecked == false && char.IsLetter(sym) && _replacedLetters.ContainsKey(letter))
                {
                    char decryptedLetter = _replacedLetters[letter];
                    if (char.IsUpper(sym))
                    {
                        decryptedLetter = char.ToUpper(decryptedLetter);
                    }
                    run.Text = decryptedLetter.ToString();
                    run.Foreground = Brushes.Black;
                }
                else if (char.IsLetter(sym))
                {
                    if (!_alphabet.Contains(letter))
                    {
                        run.Foreground = Brushes.Black;
                    }
                    else
                    {
                        HasEncryptedLetters = true;
                        run.Foreground = Brushes.Red;
                    }
                }
                else
                {
                    run.Foreground = Brushes.Black;
                }

                if (cbShowRelpacedOnly.IsChecked == true || cbShowEncryptedOnly.IsChecked == true)
                {
                    btnAdd.IsEnabled = false;
                    btnDelete.IsEnabled = false;
                }
                else
                {
                    btnAdd.IsEnabled = true;
                    btnDelete.IsEnabled = true;
                }

                runs.Add(run);
            }

            paragraph.Inlines.AddRange(runs);
            rtbCryptotext.Document.Blocks.Clear();
            rtbCryptotext.Document.Blocks.Add(paragraph);

            string decryptedText = new TextRange(rtbCryptotext.Document.ContentStart, rtbCryptotext.Document.ContentEnd).Text.Trim();

            if (HasEncryptedLetters == false)
            {
                cbShowRelpacedOnly.IsEnabled = false;
                btnAdd.IsEnabled = false;

                if (_successNoticied == false && !string.IsNullOrEmpty(_decryptedText) && _replacedLetters.Count > 0 && decryptedText == _decryptedText)
                {
                    _secondTimer.Stop();
                    _successNoticied = true;

                    string formattedTime = TimeSpan.FromSeconds(_seconds).ToString(@"hh\:mm\:ss");
                    MessageBox.Show($"Лабораторну роботу успішно виконано!\n\nЗатрачено часу: {formattedTime}", "Успіх!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                if (cbShowEncryptedOnly.IsChecked == false)
                {
                    if (!_secondTimer.IsEnabled)
                    {
                        _secondTimer.Start();
                        _successNoticied = false;
                    }
                }
            }
        }

        private void UpdateTime()
        {
            string formattedTime = TimeSpan.FromSeconds(_seconds).ToString(@"hh\:mm\:ss");
            lTime.Content = formattedTime;
        }

        //========================================================================================================================================================================================================

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string replaceFrom = tbReplaceFrom.Text;
            string replaceTo = tbReplaceTo.Text;
            tbReplaceFrom.Clear();
            tbReplaceTo.Clear();

            if (replaceFrom.Length == 1 && replaceTo.Length == 1)
            {
                char from = replaceFrom[0];
                char to = replaceTo[0];
                from = char.ToLower(from);
                to = char.ToLower(to);

                if (from == to)
                {
                    SystemSounds.Hand.Play();
                    return;
                }

                if (!_alphabet.Contains(from) || !_alphabet.Contains(to))
                {
                    SystemSounds.Hand.Play();
                    return;
                }

                if (_replacedLetters.ContainsKey(from) || _replacedLetters.ContainsValue(to))
                {
                    SystemSounds.Hand.Play();
                    return;
                }

                _replacedLetters.Add(char.ToLower(from), char.ToLower(to));

                lbChanges.Items.Add($"{char.ToUpper(from)} = {char.ToUpper(to)}");

                UpdateCryptotext(_encryptedText);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lbChanges.SelectedItem == null)
            {
                return;
            }

            string selectedItem = lbChanges.SelectedItem.ToString();
            string[] parts = selectedItem.Split('=');
            char from = parts[0].Trim()[0];
            char to = parts[1].Trim()[0];

            _replacedLetters.Remove(char.ToLower(from));

            lbChanges.Items.Remove(lbChanges.SelectedItem);

            UpdateCryptotext(_encryptedText);
        }

        private void cbShowRelpacedOnly_Checked(object sender, RoutedEventArgs e)
        {
            cbShowEncryptedOnly.IsChecked = false;
            UpdateCryptotext(_encryptedText);
        }

        private void cbShowRelpacedOnly_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateCryptotext(_encryptedText);
        }

        private void cbShowEncryptedOnly_Checked(object sender, RoutedEventArgs e)
        {
            cbShowRelpacedOnly.IsChecked = false;
            UpdateCryptotext(_encryptedText);
        }

        private void cbShowEncryptedOnly_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateCryptotext(_encryptedText);
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnShowFrequencies_Click(object sender, RoutedEventArgs e)
        {
            var frequencies = new Dictionary<char, int>();

            foreach (char sym in _encryptedText)
            {
                char letter = char.ToLower(sym);

                if (char.IsLetter(sym) && _alphabet.Contains(letter))
                {
                    if (frequencies.ContainsKey(letter))
                    {
                        frequencies[letter]++;
                    }
                    else
                    {
                        frequencies[letter] = 1;
                    }
                }
            }
            var sortedFrequencies = frequencies.OrderByDescending(kv => kv.Value).Take(10);
            string text = "Перші 10 літер, що найчастіше зустрічаються в тексті:\n\n";
            foreach (var pair in sortedFrequencies)
            {
                text += $"{char.ToUpper(pair.Key)} = {pair.Value}\n";
            }
            MessageBox.Show(text, "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        //========================================================================================================================================================================================================

        private void SecondUpdate(object sender, EventArgs e)
        {
            _seconds++;
            UpdateTime();
        }

    }
}