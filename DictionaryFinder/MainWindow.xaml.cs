using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;


namespace DictionaryFinder {
    public partial class MainWindow : Window {
        public MainWindow() {
            myDictionary = new WordDictionary("dict.txt");
            SearchResults = new ObservableCollection<string>();
            InitializeComponent();
        }

        public ObservableCollection<string> SearchResults { get; set; }

        private void SearchField_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            StartSearchTask();
        }

        private void SeqSearchCheckbox_OnChecked(object sender, RoutedEventArgs e) {
            StartSearchTask();
        }

        private async void StartSearchTask()
        {
            tokenSource?.Cancel();
            tokenSource = new CancellationTokenSource();
            SearchResults.Clear();

            string request = SearchField.Text;
            if (request.Length == 0) {
                StatusLabel.Content = "Ready";
                return;
            }

            bool isSeqSearch = SeqSearchCheckbox.IsChecked.Value;

            await Task.Run(() => LoadData(request, isSeqSearch, tokenSource.Token), tokenSource.Token);
        }

        private Task LoadData(string request, bool isSeqSearch, CancellationToken ct)
        {
            Dispatcher.Invoke(() =>
            {
                StatusLabel.Content = "Search in progress...";
            });

            foreach (var res in myDictionary.GetMatches(request, isSeqSearch)) {
                if (ct.IsCancellationRequested) {
                    return Task.CompletedTask;
                }

                Dispatcher.Invoke(() =>
                {
                    if (ct.IsCancellationRequested) {
                        return;
                    }

                    SearchResults.Add(res);

                    if (myListScrollViewer == null) {
                        myListScrollViewer = ResultView.GetChildOfType<ScrollViewer>();
                        if (myListScrollViewer != null) {
                            myListScrollViewer.ScrollChanged += (object o, ScrollChangedEventArgs e) =>
                            {
                                if (e.VerticalChange == 0) {
                                    return;
                                }

                                if (e.VerticalOffset + e.ViewportHeight == e.ExtentHeight) {
                                    Console.Out.WriteLine("end of scroll");
                                }
                            };
                        }

                    }
                });
            
            }

            Dispatcher.Invoke(() => {
                StatusLabel.Content = "Ready";
            });

            return Task.CompletedTask;
        }

        private ScrollViewer myListScrollViewer;
        private CancellationTokenSource tokenSource;
        private WordDictionary myDictionary;
    }
}
