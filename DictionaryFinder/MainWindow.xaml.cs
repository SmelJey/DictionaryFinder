using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DictionaryFinder {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            myDictionary = new WordDictionary("dict.txt");
            SearchResults = new ObservableCollection<string>();
            //myTaskFactory = new TaskFactory(tokenSource.Token);
            InitializeComponent();
        }

        public ObservableCollection<string> SearchResults { get; set; }

        private async void SearchField_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            tokenSource?.Cancel();
            tokenSource = new CancellationTokenSource();
            SearchResults.Clear();

            string request = SearchField.Text;
            if (request.Length == 0)
            {
                StatusLabel.Content = "Ready";
                return;
            }
            await Task.Run(() => LoadData(request, tokenSource.Token), tokenSource.Token);
        }

        private Task LoadData(string request, CancellationToken ct)
        {
            Dispatcher.Invoke(() =>
            {
                StatusLabel.Content = "Search in progress...";
            });
            foreach (var res in myDictionary.GetMatches(request)) {
                if (ct.IsCancellationRequested)
                {
                    return Task.CompletedTask;
                }

                Dispatcher.Invoke(() =>
                {
                    if (ct.IsCancellationRequested)
                    {
                        return;
                    }

                    SearchResults.Add(res);
                    
                    if (myListScrollViewer == null) {
                        myListScrollViewer = ResultView.GetChildOfType<ScrollViewer>();
                        if (myListScrollViewer != null)
                        {
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
